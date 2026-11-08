
import json
import os
import re
import time
import cv2
import numpy as np
import pytesseract
from pytesseract import Output


class OCRModel:

    def __init__(self):

        self.languages = "vie+eng"
        self.psm = 4


    def load_image(self, path):

        image = cv2.imread(path)

        if image is None:

            raise FileNotFoundError(
                f"Cannot read image: {path}"
            )

        return image

    def preprocess(self, image):

        start = time.perf_counter()

        gray = cv2.cvtColor(
            image,
            cv2.COLOR_BGR2GRAY
        )

        elapsed = (
            time.perf_counter()
            - start
        )

        return gray


    def predict(self, image):

        total_start = time.perf_counter()

        processed = self.preprocess(image)

        start = time.perf_counter()

        config = (
            f"--oem 3 "
            f"--psm {self.psm}"
        )

        data = pytesseract.image_to_data(
            processed,
            lang=self.languages,
            config=config,
            output_type=Output.DICT
        )

        ocr_time = (
            time.perf_counter()
            - start
        )

        words = []
        confidence_values = []

        total = len(
            data["text"]
        )

        for i in range(total):

            text = (
                data["text"][i]
                or ""
            ).strip()

            if not text:
                continue

            try:

                confidence = float(
                    data["conf"][i]
                )

            except Exception:

                confidence = -1

            if confidence < 0:
                continue

            x = int(data["left"][i])
            y = int(data["top"][i])
            w = int(data["width"][i])
            h = int(data["height"][i])

            block = int(
                data["block_num"][i]
            )

            paragraph = int(
                data["par_num"][i]
            )

            line = int(
                data["line_num"][i]
            )

            words.append({
                "text": text,
                "x": x,
                "y": y,
                "width": w,
                "height": h,
                "right": x + w,
                "bottom": y + h,
                "confidence": confidence,
                "block": block,
                "paragraph": paragraph,
                "line": line
            })

            confidence_values.append(
                confidence
            )


        words.sort(
            key=lambda item: (
                item["y"],
                item["x"]
            )
        )

        lines = self._group_lines(
            words
        )

        blocks = self._build_blocks(
            lines,
            image.shape[0],
            image.shape[1]
        )


        plain_text = self._build_plain_text(
            lines
        )


        layout = self._detect_layout(
            image,
            lines,
            blocks
        )


        layout_text = self._build_layout_text(
            layout["regions"],
            image.shape[1]
        )

        if confidence_values:

            confidence = (
                sum(confidence_values)
                /
                len(confidence_values)
            )

        else:

            confidence = 0.0

        quality = self._quality(
            processed
        )

        score = (
            confidence * 0.8
            +
            quality * 0.2
        )

        total_time = (
            time.perf_counter()
            - total_start
        )



        return {
            "text": plain_text,
            "layout_text": layout_text,
            "lines": lines,
            "blocks": blocks,
            "layout": layout,
            "confidence": confidence,
            "quality": quality,
            "score": score,
            "mode": "gray",
            "psm": self.psm,
            "time": ocr_time,
            "total_ocr_time": total_time
        }


    def _group_lines(self, words):

        if not words:
            return []

        lines = []

        for word in words:

            center_y = (
                word["y"]
                +
                word["height"] / 2
            )

            selected = None
            best_distance = None

            for line in lines:

                tolerance = max(
                    8,
                    int(line["height"] * 0.65)
                )

                distance = abs(
                    center_y
                    -
                    line["center_y"]
                )

                if distance <= tolerance:

                    if (
                        best_distance is None
                        or
                        distance < best_distance
                    ):

                        best_distance = distance
                        selected = line

            if selected is not None:

                selected["words"].append(
                    word
                )

                selected["center_y"] = (
                    sum(
                        w["y"] + w["height"] / 2
                        for w in selected["words"]
                    )
                    /
                    len(selected["words"])
                )

                selected["height"] = max(
                    selected["height"],
                    word["height"]
                )

            else:

                lines.append({
                    "words": [word],
                    "center_y": center_y,
                    "height": word["height"]
                })

        lines.sort(
            key=lambda line:
                line["center_y"]
        )

        result = []

        for line in lines:

            line["words"].sort(
                key=lambda word:
                    word["x"]
            )

            finalized = self._finalize_line(
                line["words"]
            )

            result.append(
                finalized
            )

        return result


    def _finalize_line(self, words):

        text_parts = []

        for word in words:

            text_parts.append(
                word["text"]
            )

        text = " ".join(
            text_parts
        )

        text = self._clean_text(
            text
        )

        left = min(
            word["x"]
            for word in words
        )

        top = min(
            word["y"]
            for word in words
        )

        right = max(
            word["right"]
            for word in words
        )

        bottom = max(
            word["bottom"]
            for word in words
        )

        height = (
            bottom
            -
            top
        )

        confidence = (
            sum(
                word["confidence"]
                for word in words
            )
            /
            len(words)
        )

        return {
            "text": text,
            "x": left,
            "y": top,
            "width": right - left,
            "height": height,
            "right": right,
            "bottom": bottom,
            "confidence": confidence,
            "words": words
        }


    def _build_blocks(
        self,
        lines,
        image_height,
        image_width
    ):

        if not lines:
            return []

        blocks = []
        current_lines = []

   
        for line in lines:

            if not current_lines:

                current_lines.append(
                    line
                )

                continue

            previous = current_lines[-1]

            vertical_gap = (
                line["y"]
                -
                previous["bottom"]
            )

            previous_height = max(
                previous["height"],
                1
            )


            if vertical_gap <= (
                previous_height * 0.95
            ):

                current_lines.append(
                    line
                )

            else:

                blocks.append(
                    self._finalize_block(
                        current_lines
                    )
                )

                current_lines = [
                    line
                ]

        if current_lines:

            blocks.append(
                self._finalize_block(
                    current_lines
                )
            )

        for block in blocks:

            block["type"] = (
                self._classify_block(
                    block,
                    image_height,
                    image_width
                )
            )

        return blocks


    def _finalize_block(self, lines):

        text = "\n".join(
            line["text"]
            for line in lines
        )

        x = min(
            line["x"]
            for line in lines
        )

        y = min(
            line["y"]
            for line in lines
        )

        right = max(
            line["right"]
            for line in lines
        )

        bottom = max(
            line["bottom"]
            for line in lines
        )

        return {
            "text": text,
            "x": x,
            "y": y,
            "width": right - x,
            "height": bottom - y,
            "right": right,
            "bottom": bottom,
            "lines": lines
        }


    def _classify_block(
        self,
        block,
        image_height,
        image_width
    ):

        text = block["text"].strip()

        if not text:
            return "empty"

        if re.match(
            r"^Bước\s+\d+",
            text,
            re.IGNORECASE
        ):

            return "step"

        if re.match(
            r"^[-•▪●◦*]\s+",
            text
        ):

            return "list"


        if re.match(
            r"^\d{1,3}[.)]\s+",
            text
        ):

            return "list"


        heading_patterns = [
            r"^chương\s+\w+",
            r"^phần\s+\w+",
            r"^mục\s+\d+",
            r"^bài\s+\d+",
            r"^câu\s+\d+",
            r"^tiêu\s+đề"
        ]

        for pattern in heading_patterns:

            if re.match(
                pattern,
                text,
                re.IGNORECASE
            ):

                return "heading"


        letters = re.sub(
            r"[^A-Za-zÀ-ỹĐđ]",
            "",
            text
        )

        if letters:

            upper_count = sum(
                1
                for c in letters
                if c.isupper()
            )

            upper_ratio = (
                upper_count
                /
                len(letters)
            )

            if (
                upper_ratio > 0.85
                and
                len(text) < 120
            ):

                return "heading"


        if (
            len(text) < 70
            and
            block["height"] < 120
        ):


            punctuation_count = len(
                re.findall(
                    r"[,.!?;:]",
                    text
                )
            )

            if punctuation_count <= 1:

                return "heading"


        center_y = (
            block["y"]
            +
            block["height"] / 2
        )

        if (
            center_y < image_height * 0.08
            and
            len(text) < 100
        ):

            return "header"

        if (
            center_y > image_height * 0.93
            and
            len(text) < 100
        ):

            return "footer"

        return "paragraph"

    def _detect_layout(
        self,
        image,
        lines,
        blocks
    ):

        height, width = image.shape[:2]

        regions = []


        columns = self._detect_columns(
            lines,
            width
        )


        for index, block in enumerate(blocks):

            x = int(block["x"])
            y = int(block["y"])
            right = int(block["right"])
            bottom = int(block["bottom"])

            center_x = (
                x + right
            ) / 2

            column_index = (
                self._column_for_x(
                    center_x,
                    columns,
                    width
                )
            )

            alignment = (
                self._detect_alignment(
                    block,
                    width
                )
            )

            region = {
                "id": index + 1,
                "type": block.get(
                    "type",
                    "paragraph"
                ),
                "x": x,
                "y": y,
                "width": int(block["width"]),
                "height": int(block["height"]),
                "right": right,
                "bottom": bottom,
                "confidence": round(
                    float(
                        self._block_confidence(
                            block
                        )
                    ),
                    2
                ),
                "alignment": alignment,
                "column": column_index,
                "text": block["text"],
                "lines": [
                    {
                        "text": line["text"],
                        "x": int(line["x"]),
                        "y": int(line["y"]),
                        "width": int(line["width"]),
                        "height": int(line["height"]),
                        "confidence": round(
                            float(line["confidence"]),
                            2
                        )
                    }
                    for line in block["lines"]
                ]
            }

            regions.append(
                region
            )

        regions = self._reading_order(
            regions
        )

        for order, region in enumerate(
            regions,
            start=1
        ):

            region["reading_order"] = order

        page_type = self._detect_page_type(
            regions,
            width,
            height
        )


        type_counts = {}

        for region in regions:

            region_type = region["type"]

            type_counts[region_type] = (
                type_counts.get(
                    region_type,
                    0
                )
                +
                1
            )

        return {
            "page_width": width,
            "page_height": height,
            "page_type": page_type,
            "columns": columns,
            "regions": regions,
            "region_count": len(regions),
            "type_counts": type_counts
        }


    def _detect_columns(
        self,
        lines,
        image_width
    ):

        if len(lines) < 4:
            return 1

        usable = []

        for line in lines:

            if not line["text"].strip():
                continue

            usable.append(line)

        if len(usable) < 4:
            return 1

        centers = np.array(
            [
                line["x"] + line["width"] / 2
                for line in usable
            ],
            dtype=np.float32
        )

        left_count = int(
            np.sum(
                centers < image_width * 0.50
            )
        )

        right_count = int(
            np.sum(
                centers >= image_width * 0.50
            )
        )

        if (
            left_count >= 3
            and
            right_count >= 3
        ):

            left_x = [
                line["x"]
                for line in usable
                if (
                    line["x"]
                    <
                    image_width * 0.50
                )
            ]

            right_x = [
                line["x"]
                for line in usable
                if (
                    line["x"]
                    >=
                    image_width * 0.50
                )
            ]

            if left_x and right_x:

                left_max = max(
                    line["right"]
                    for line in usable
                    if (
                        line["x"]
                        <
                        image_width * 0.50
                    )
                )

                right_min = min(
                    line["x"]
                    for line in usable
                    if (
                        line["x"]
                        >=
                        image_width * 0.50
                    )
                )

                gap = right_min - left_max

                if gap > image_width * 0.08:
                    return 2

        return 1


    def _column_for_x(
        self,
        center_x,
        columns,
        image_width
    ):

        if columns <= 1:
            return 1

        if center_x < 0:
            return 1

        if center_x < image_width * 0.50:
            return 1

        return 2


    def _detect_alignment(
        self,
        block,
        image_width
    ):

        center = (
            block["x"]
            +
            block["width"] / 2
        )

        page_center = (
            image_width / 2
        )

        tolerance = (
            image_width * 0.10
        )

        if abs(
            center - page_center
        ) <= tolerance:

            if block["width"] < (
                image_width * 0.70
            ):

                return "center"

        if block["x"] < (
            image_width * 0.15
        ):

            return "left"

        if block["right"] > (
            image_width * 0.85
        ):

            return "right"

        return "left"

    def _block_confidence(
        self,
        block
    ):

        values = []

        for line in block["lines"]:

            values.append(
                float(
                    line["confidence"]
                )
            )

        if not values:
            return 0.0

        return (
            sum(values)
            /
            len(values)
        )

    def _reading_order(
        self,
        regions
    ):

        if not regions:
            return []

        max_column = max(
            region.get(
                "column",
                1
            )
            for region in regions
        )

        if max_column > 1:

            ordered = []

            for column in range(
                1,
                max_column + 1
            ):

                column_regions = [
                    region
                    for region in regions
                    if region.get(
                        "column",
                        1
                    ) == column
                ]

                column_regions.sort(
                    key=lambda item: (
                        item["y"],
                        item["x"]
                    )
                )

                ordered.extend(
                    column_regions
                )

            return ordered

        return sorted(
            regions,
            key=lambda item: (
                item["y"],
                item["x"]
            )
        )


    def _detect_page_type(
        self,
        regions,
        width,
        height
    ):

        if not regions:
            return "empty"

        column_values = [
            region.get(
                "column",
                1
            )
            for region in regions
        ]

        if max(column_values) >= 2:
            return "two-column"

        heading_count = sum(
            1
            for region in regions
            if region["type"] == "heading"
        )

        paragraph_count = sum(
            1
            for region in regions
            if region["type"] == "paragraph"
        )

        if (
            heading_count >= 2
            and
            paragraph_count >= 2
        ):

            return "document"

        return "single-column"

    def _build_plain_text(
        self,
        lines
    ):

        result = []

        for line in lines:

            text = line["text"].strip()

            if text:

                result.append(
                    text
                )

        return "\n".join(
            result
        )

   
    def _build_layout_text(
        self,
        regions,
        image_width
    ):

        if not regions:
            return ""

        values = []

        for region in regions:

            for line in region.get(
                "lines",
                []
            ):

                text = line["text"]

                if not text:
                    continue

                value = (
                    line["width"]
                    /
                    max(
                        len(text),
                        1
                    )
                )

                if 2 <= value <= 50:
                    values.append(value)

        if values:

            char_width = (
                sum(values)
                /
                len(values)
            )

        else:

            char_width = 8

        columns = int(
            image_width
            /
            max(
                char_width,
                6
            )
        )

        columns = max(
            60,
            min(
                columns,
                180
            )
        )

        result = []
        previous_region = None

        for region in regions:

            region_type = region.get(
                "type",
                "paragraph"
            )

            if previous_region is not None:

                gap = (
                    region["y"]
                    -
                    previous_region["bottom"]
                )

                previous_height = max(
                    previous_region["height"],
                    1
                )

                if gap > (
                    previous_height * 0.8
                ):

                    result.append("")

            for line in region.get(
                "lines",
                []
            ):

                text = line["text"].strip()

                if not text:
                    continue

                indent = int(
                    (
                        line["x"]
                        /
                        max(
                            image_width,
                            1
                        )
                    )
                    *
                    columns
                )

                indent = max(
                    0,
                    min(
                        indent,
                        columns - 1
                    )
                )

                if region_type == "heading":

                    center = (
                        line["x"]
                        +
                        line["width"] / 2
                    )

                    image_center = (
                        image_width / 2
                    )

                    if abs(
                        center - image_center
                    ) < image_width * 0.15:

                        text_indent = max(
                            0,
                            int(
                                (
                                    columns
                                    -
                                    len(text)
                                )
                                /
                                2
                            )
                        )

                        formatted = (
                            " "
                            *
                            text_indent
                            +
                            text
                        )

                    else:

                        formatted = (
                            " "
                            *
                            indent
                            +
                            text
                        )

                elif region_type in (
                    "list",
                    "step"
                ):

                    formatted = (
                        " "
                        *
                        indent
                        +
                        text
                    )

                else:

                    formatted = (
                        " "
                        *
                        indent
                        +
                        text
                    )

                result.append(
                    formatted.rstrip()
                )

            if region_type in (
                "heading",
                "list",
                "step"
            ):

                result.append("")

            previous_region = region

        cleaned = []
        empty_before = False

        for line in result:

            if line == "":

                if empty_before:
                    continue

                empty_before = True

            else:

                empty_before = False

            cleaned.append(line)

 
        while (
            cleaned
            and
            cleaned[0] == ""
        ):

            cleaned.pop(0)

        while (
            cleaned
            and
            cleaned[-1] == ""
        ):

            cleaned.pop()

        return "\n".join(
            cleaned
        )


    def save_layout_json(
        self,
        layout,
        path
    ):

        directory = os.path.dirname(
            path
        )

        if directory:

            os.makedirs(
                directory,
                exist_ok=True
            )

        with open(
            path,
            "w",
            encoding="utf-8"
        ) as f:

            json.dump(
                layout,
                f,
                ensure_ascii=False,
                indent=2
            )


    def save_layout_text(
        self,
        text,
        path
    ):

        directory = os.path.dirname(
            path
        )

        if directory:

            os.makedirs(
                directory,
                exist_ok=True
            )

        with open(
            path,
            "w",
            encoding="utf-8"
        ) as f:

            f.write(text)

    def draw_layout(
        self,
        image,
        layout
    ):

        canvas = image.copy()

        type_labels = {
            "heading": "HEADING",
            "paragraph": "PARAGRAPH",
            "list": "LIST",
            "step": "STEP",
            "header": "HEADER",
            "footer": "FOOTER",
            "empty": "EMPTY"
        }

        type_colors = {
            "heading": (255, 0, 0),
            "paragraph": (0, 180, 0),
            "list": (0, 140, 255),
            "step": (255, 0, 255),
            "header": (255, 180, 0),
            "footer": (180, 0, 180),
            "empty": (100, 100, 100)
        }

        for region in layout.get(
            "regions",
            []
        ):

            x1 = int(region["x"])
            y1 = int(region["y"])
            x2 = int(region["right"])
            y2 = int(region["bottom"])

            region_type = region.get(
                "type",
                "paragraph"
            )

            color = type_colors.get(
                region_type,
                (0, 200, 0)
            )

            cv2.rectangle(
                canvas,
                (x1, y1),
                (x2, y2),
                color,
                2
            )

            label = (
                f'{region["reading_order"]}. '
                f'{type_labels.get(region_type, region_type)}'
            )

            label_y = max(
                20,
                y1 - 7
            )

            cv2.putText(
                canvas,
                label,
                (x1, label_y),
                cv2.FONT_HERSHEY_SIMPLEX,
                0.55,
                color,
                2,
                cv2.LINE_AA
            )

        return canvas


    def save_layout_image(
        self,
        image,
        layout,
        path
    ):

        directory = os.path.dirname(
            path
        )

        if directory:

            os.makedirs(
                directory,
                exist_ok=True
            )

        canvas = self.draw_layout(
            image,
            layout
        )

        ok = cv2.imwrite(
            path,
            canvas
        )

        if not ok:

            raise IOError(
                f"Cannot save layout image: {path}"
            )

    def _clean_text(
        self,
        text
    ):

        text = text.strip()

        text = re.sub(
            r"[ \t]+",
            " ",
            text
        )

        text = re.sub(
            r"\s+([,.!?;:%])",
            r"\1",
            text
        )

        text = re.sub(
            r"\(\s+",
            "(",
            text
        )

        text = re.sub(
            r"\s+\)",
            ")",
            text
        )

        text = text.replace(
            "`",
            ""
        )

        text = text.replace(
            "ˆ",
            ""
        )

        return text.strip()


    def _quality(
        self,
        gray
    ):

        try:

            variance = cv2.Laplacian(
                gray,
                cv2.CV_64F
            ).var()

            quality = min(
                100,
                variance / 10
            )

            return float(
                quality
            )

        except Exception:

            return 0.0
