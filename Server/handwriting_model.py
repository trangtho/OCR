
import io
import contextlib
import os
import sys
import cv2
import re
import json
import time
import math
import unicodedata
import numpy as np
import torch
import torch.nn as nn
import torch.nn.functional as F

from PIL import Image

from paddleocr import TextDetection
from vietocr.tool.predictor import Predictor
from vietocr.tool.config import Cfg



BASE_DIR = os.path.dirname(os.path.abspath(__file__))

VIETOCR_WEIGHTS = os.path.join(
    BASE_DIR,
    "weights",
    "vgg_transformer.pth"
)



VIETOCR_MODEL = "vgg_transformer"


DET_THRESHOLD = 0.20

FAST_OCR_MODE = True
FAST_SECONDARY_SCORE = 60
OCR_EARLY_ACCEPT_SCORE = 72

PADDING_X = 8
PADDING_Y = 6

MIN_HEIGHT = 32

MAX_UPSCALE = 3.0

DUPLICATE_IOU = 0.70

USE_RGB = True
USE_GRAY = False
USE_CLAHE = True
USE_OTSU = False
USE_ADAPTIVE = False


#LAYOUT_NEURAL_ENABLED = True
LAYOUT_NEURAL_ENABLED = False
LAYOUT_WEIGHTS = os.path.join(BASE_DIR, "weights", "layout_cnn_encoder_decoder.pth")
LAYOUT_IMAGE_SIZE = 224
LAYOUT_EMBED_DIM = 256
LAYOUT_NUM_HEADS = 8
LAYOUT_NUM_LAYERS = 2
LAYOUT_NUM_CLASSES = 4
LAYOUT_CLASSES = ("heading", "paragraph", "list", "step")
LAYOUT_NEURAL_MIN_CONF = 0.60

LOW_SCORE_THRESHOLD = 48

SAVE_CROPS = False

class LayoutCNNEncoder(nn.Module):
    def __init__(self, embed_dim=LAYOUT_EMBED_DIM):
        super().__init__()
        self.features = nn.Sequential(
            nn.Conv2d(3, 32, 3, padding=1), nn.BatchNorm2d(32), nn.ReLU(True), nn.MaxPool2d(2),
            nn.Conv2d(32, 64, 3, padding=1), nn.BatchNorm2d(64), nn.ReLU(True), nn.MaxPool2d(2),
            nn.Conv2d(64, 128, 3, padding=1), nn.BatchNorm2d(128), nn.ReLU(True), nn.MaxPool2d(2),
            nn.Conv2d(128, 256, 3, padding=1), nn.BatchNorm2d(256), nn.ReLU(True),
            nn.AdaptiveAvgPool2d((1, 1))
        )
        self.projection = nn.Linear(256, embed_dim)
    def forward(self, x):
        return self.projection(torch.flatten(self.features(x), 1))

class LayoutEncoder(nn.Module):
    def __init__(self, embed_dim=LAYOUT_EMBED_DIM, num_heads=LAYOUT_NUM_HEADS, num_layers=LAYOUT_NUM_LAYERS):
        super().__init__()
        layer = nn.TransformerEncoderLayer(
            d_model=embed_dim, nhead=num_heads, dim_feedforward=embed_dim * 4,
            dropout=0.10, activation="gelu", batch_first=True, norm_first=True
        )
        self.encoder = nn.TransformerEncoder(layer, num_layers=num_layers)
    def forward(self, x):
        return self.encoder(x)

class LayoutDecoder(nn.Module):
    def __init__(self, embed_dim=LAYOUT_EMBED_DIM, num_classes=LAYOUT_NUM_CLASSES):
        super().__init__()
        self.norm = nn.LayerNorm(embed_dim)
        self.decoder = nn.Sequential(
            nn.Linear(embed_dim, embed_dim // 2), nn.GELU(), nn.Dropout(0.10),
            nn.Linear(embed_dim // 2, num_classes)
        )
    def forward(self, x):
        return self.decoder(self.norm(x))

class LayoutCNNEncoderDecoder(nn.Module):
    def __init__(self):
        super().__init__()
        self.cnn = LayoutCNNEncoder()
        self.encoder = LayoutEncoder()
        self.decoder = LayoutDecoder()
    def forward(self, images):
        tokens = self.cnn(images)
        encoded = self.encoder(tokens.unsqueeze(0)).squeeze(0)
        return self.decoder(encoded)

def load_layout_model():
    if not LAYOUT_NEURAL_ENABLED:
        return None
    if not os.path.isfile(LAYOUT_WEIGHTS):
        print("[LAYOUT] Chưa có checkpoint:", LAYOUT_WEIGHTS)
        print("[LAYOUT] Bỏ qua neural layout -> dùng heuristic.")
        return None
    model = LayoutCNNEncoderDecoder()
    if not os.path.isfile(LAYOUT_WEIGHTS):
        print("[LAYOUT] CNN -> Encoder -> Decoder đã được thêm.")
        print("[LAYOUT] Chưa có checkpoint:", LAYOUT_WEIGHTS)
        print("[LAYOUT] Giữ heuristic layout làm fallback.")
        return None
    try:
        checkpoint = torch.load(LAYOUT_WEIGHTS, map_location="cpu")
        if isinstance(checkpoint, dict):
            checkpoint = checkpoint.get("state_dict", checkpoint.get("model_state_dict", checkpoint))
            checkpoint = {k.replace("module.", "", 1): v for k, v in checkpoint.items()}
        model.load_state_dict(checkpoint, strict=True)
        model.eval()
        print("[LAYOUT] Loaded:", LAYOUT_WEIGHTS)
        return model
    except Exception as e:
        print("[LAYOUT] Load checkpoint failed:", e)
        return None

def layout_crop_to_tensor(image, bbox):
    if bbox is None:
        return None
    x1, y1, x2, y2 = [int(v) for v in bbox]
    x1, y1 = max(0, x1), max(0, y1)
    x2, y2 = min(image.shape[1], x2), min(image.shape[0], y2)
    if x2 <= x1 or y2 <= y1:
        return None
    crop = image[y1:y2, x1:x2]
    if crop is None or crop.size == 0:
        return None
    crop = cv2.cvtColor(crop, cv2.COLOR_BGR2RGB)
    crop = cv2.resize(crop, (LAYOUT_IMAGE_SIZE, LAYOUT_IMAGE_SIZE), interpolation=cv2.INTER_AREA)
    t = torch.from_numpy(crop.astype(np.float32) / 255.0).permute(2,0,1)
    mean = torch.tensor([0.5,0.5,0.5]).view(3,1,1)
    std = torch.tensor([0.5,0.5,0.5]).view(3,1,1)
    return (t - mean) / std

def neural_layout_classify(model, image, final_lines):
    if model is None or not final_lines:
        return final_lines
    tensors, valid = [], []
    for i, line in enumerate(final_lines):
        t = layout_crop_to_tensor(image, line.get("bbox"))
        if t is not None:
            tensors.append(t); valid.append(i)
    if not tensors:
        return final_lines
    with torch.inference_mode():
        probs = F.softmax(model(torch.stack(tensors)), dim=-1)
        conf, idx = torch.max(probs, dim=-1)
    for j, i in enumerate(valid):
        c = float(conf[j]); typ = LAYOUT_CLASSES[int(idx[j])]
        final_lines[i]["neural_layout"] = typ
        final_lines[i]["neural_confidence"] = round(c,4)
        if c >= LAYOUT_NEURAL_MIN_CONF:
            final_lines[i]["layout"] = typ
    return final_lines

def heuristic_layout_type(text, bbox, image_width):
    text = (text or "").strip()
    if not text: return "empty"
    if re.match(r"^(?:Bước|Buoc)\s+\d+", text, re.I): return "step"
    if re.match(r"^[-•▪●◦*]\s*", text): return "list"
    if re.match(r"^\d{1,3}[\.)]\s+", text): return "list"
    if re.match(r"^\d{1,2}\.\s+\S", text) and len(text) <= 120: return "heading"
    letters = re.sub(r"[^A-Za-zÀ-ỹĐđ]", "", text)
    if letters and sum(c.isupper() for c in letters) / len(letters) > 0.85 and len(text) < 120: return "heading"
    if bbox and image_width:
        x1, _, x2, _ = bbox
        if abs((x1+x2)/2-image_width/2) < image_width*0.15 and len(text) < 70: return "heading"
    return "heading" if len(text) < 70 else "paragraph"

def apply_layout_recognition(image, final_lines, image_width, layout_model=None):
    for line in final_lines:
        line["layout"] = heuristic_layout_type(line.get("text", ""), line.get("bbox"), image_width)
    return neural_layout_classify(layout_model, image, final_lines)


VIETNAMESE_CHARS = (
    "a-zA-Z"
    "À-ỹ"
    "Đđ"
)

VALID_CHAR_PATTERN = re.compile(
    rf"[{VIETNAMESE_CHARS}0-9]"
)


def validate_vietocr_weights(path):
    """
    Kiểm tra checkpoint có đúng kiến trúc vgg_transformer hay không.

    Tránh trường hợp dùng nhầm checkpoint RNN/Seq2Seq
    cho model Transformer.
    """

    if not os.path.exists(path):
        raise FileNotFoundError(
            f"\nKhông tìm thấy VietOCR weights:\n{path}\n"
        )

    print("\n[CHECK] Kiểm tra VietOCR weights...")
    print("[WEIGHTS]", path)

    try:
        checkpoint = torch.load(
            path,
            map_location="cpu"
        )
    except Exception as e:
        raise RuntimeError(
            f"Không thể đọc weights: {e}"
        )

    state_dict = checkpoint

    if isinstance(checkpoint, dict):

        if "state_dict" in checkpoint:
            state_dict = checkpoint["state_dict"]

        elif "model" in checkpoint:
            state_dict = checkpoint["model"]

    if not isinstance(state_dict, dict):
        raise RuntimeError(
            "Checkpoint không có state_dict hợp lệ."
        )

    keys = list(state_dict.keys())

    transformer_markers = [
        "transformer.embed_tgt.weight",
        "transformer.pos_enc.pe",
        "transformer.transformer.encoder",
        "transformer.transformer.decoder",
    ]

    found = 0

    for marker in transformer_markers:

        for key in keys:

            if marker in key:
                found += 1
                break

    if found < 2:

        print("\n[ERROR] Weight hiện tại không giống")
        print("        kiến trúc vgg_transformer.")

        print("\nMột số key tìm thấy:")

        for key in keys[:15]:
            print(" ", key)

        raise RuntimeError(
            "\n\nVietOCR weights không tương thích "
            "với vgg_transformer.\n"
            "Hãy thay bằng checkpoint vgg_transformer.pth "
            "chính thức."
        )

    print("[OK] Weight phù hợp với Transformer.")


def load_vietocr():

    print("\n================================================")
    print("LOAD VIETOCR - LOCAL / OFFLINE")
    print("================================================")

    validate_vietocr_weights(
        VIETOCR_WEIGHTS
    )

    VIETOCR_CONFIG = os.path.join(
        BASE_DIR,
        "config",
        "vgg_transformer.yml"
    )

    print("[CONFIG]", VIETOCR_CONFIG)

    if not os.path.isfile(VIETOCR_CONFIG):
        raise FileNotFoundError(
            "\nKhông tìm thấy VietOCR config:\n"
            + VIETOCR_CONFIG
        )

    config = Cfg.load_config_from_file(
        VIETOCR_CONFIG
    )

    config["device"] = "cpu"

    config["weights"] = VIETOCR_WEIGHTS

    config["pretrain"] = None

    if "predictor" not in config:
        config["predictor"] = {}

    config["predictor"]["beamsearch"] = False

    print("[MODEL]", VIETOCR_MODEL)
    print("[WEIGHTS]", VIETOCR_WEIGHTS)
    print("[DEVICE]", config["device"])
    print("[CONFIG] LOCAL")
    print("[MODE] OFFLINE")

    predictor = Predictor(config)

    print("[OK] VietOCR loaded successfully.")

    return predictor


def load_detector():

    print("\n================================================")
    print("LOAD PADDLEOCR DETECTOR")
    print("================================================")

    detector = TextDetection(
        model_name="PP-OCRv5_mobile_det",
        device="cpu",
        enable_mkldnn=False
    )

    print("[OK] PaddleOCR detector loaded.")

    return detector


def order_points(points):

    points = np.asarray(
        points,
        dtype=np.float32
    )

    if points.shape[0] != 4:
        return None

    rect = np.zeros(
        (4, 2),
        dtype=np.float32
    )

    s = points.sum(axis=1)

    rect[0] = points[np.argmin(s)]  
    rect[2] = points[np.argmax(s)] 

    diff = np.diff(
        points,
        axis=1
    )

    rect[1] = points[np.argmin(diff)] 
    rect[3] = points[np.argmax(diff)]

    return rect



def polygon_to_quad(poly):

    poly = np.asarray(
        poly,
        dtype=np.float32
    )

    if len(poly) < 4:
        return None

    if len(poly) == 4:
        return order_points(poly)

    rect = cv2.minAreaRect(poly)

    box = cv2.boxPoints(rect)

    box = np.asarray(
        box,
        dtype=np.float32
    )

    return order_points(box)


def perspective_crop(image, polygon):

    quad = polygon_to_quad(
        polygon
    )

    if quad is None:
        return None

    tl, tr, br, bl = quad

    width_top = np.linalg.norm(
        tr - tl
    )

    width_bottom = np.linalg.norm(
        br - bl
    )

    height_left = np.linalg.norm(
        bl - tl
    )

    height_right = np.linalg.norm(
        br - tr
    )

    width = int(
        max(
            width_top,
            width_bottom
        )
    )

    height = int(
        max(
            height_left,
            height_right
        )
    )

    if width < 5 or height < 5:
        return None

    dst = np.array(
        [
            [0, 0],
            [width - 1, 0],
            [width - 1, height - 1],
            [0, height - 1]
        ],
        dtype=np.float32
    )

    matrix = cv2.getPerspectiveTransform(
        quad,
        dst
    )

    crop = cv2.warpPerspective(
        image,
        matrix,
        (width, height),
        flags=cv2.INTER_CUBIC,
        borderMode=cv2.BORDER_CONSTANT,
        borderValue=(255, 255, 255)
    )

    return crop


def bbox_crop(image, polygon):

    poly = np.asarray(
        polygon,
        dtype=np.float32
    )

    x1 = int(
        np.min(poly[:, 0])
    )

    y1 = int(
        np.min(poly[:, 1])
    )

    x2 = int(
        np.max(poly[:, 0])
    )

    y2 = int(
        np.max(poly[:, 1])
    )

    h, w = image.shape[:2]

    x1 = max(0, x1)
    y1 = max(0, y1)

    x2 = min(w, x2)
    y2 = min(h, y2)

    if x2 <= x1 or y2 <= y1:
        return None

    return image[y1:y2, x1:x2].copy()


def add_white_border(
    image,
    px=PADDING_X,
    py=PADDING_Y
):

    if image is None:
        return None

    return cv2.copyMakeBorder(
        image,
        py,
        py,
        px,
        px,
        cv2.BORDER_CONSTANT,
        value=(255, 255, 255)
    )


def remove_border_noise(image):

    if image is None:
        return None

    h, w = image.shape[:2]

    if h < 10 or w < 10:
        return image

    gray = cv2.cvtColor(
        image,
        cv2.COLOR_BGR2GRAY
    )

    _, binary = cv2.threshold(
        gray,
        245,
        255,
        cv2.THRESH_BINARY_INV
    )

    coords = cv2.findNonZero(
        binary
    )

    if coords is None:
        return image

    x, y, bw, bh = cv2.boundingRect(
        coords
    )

    margin_x = max(
        2,
        int(w * 0.01)
    )

    margin_y = max(
        2,
        int(h * 0.03)
    )

    x1 = max(
        0,
        x - margin_x
    )

    y1 = max(
        0,
        y - margin_y
    )

    x2 = min(
        w,
        x + bw + margin_x
    )

    y2 = min(
        h,
        y + bh + margin_y
    )

    return image[
        y1:y2,
        x1:x2
    ]


def deskew_line(image):

    if image is None:
        return None

    gray = cv2.cvtColor(
        image,
        cv2.COLOR_BGR2GRAY
    )

    binary = cv2.threshold(
        gray,
        0,
        255,
        cv2.THRESH_BINARY_INV
        + cv2.THRESH_OTSU
    )[1]

    kernel = cv2.getStructuringElement(
        cv2.MORPH_RECT,
        (3, 2)
    )

    binary = cv2.morphologyEx(
        binary,
        cv2.MORPH_OPEN,
        kernel
    )

    coords = cv2.findNonZero(
        binary
    )

    if coords is None:
        return image

    if len(coords) < 20:
        return image

    rect = cv2.minAreaRect(
        coords
    )

    angle = rect[-1]

    if angle < -45:
        angle = 90 + angle

    if abs(angle) > 12:
        return image

    if abs(angle) < 0.3:
        return image

    h, w = image.shape[:2]

    center = (
        w // 2,
        h // 2
    )

    matrix = cv2.getRotationMatrix2D(
        center,
        angle,
        1.0
    )

    rotated = cv2.warpAffine(
        image,
        matrix,
        (w, h),
        flags=cv2.INTER_CUBIC,
        borderMode=cv2.BORDER_CONSTANT,
        borderValue=(255, 255, 255)
    )

    return rotated


def resize_for_ocr(image):

    if image is None:
        return None

    h, w = image.shape[:2]

    if h <= 0 or w <= 0:
        return None

    if h < MIN_HEIGHT:

        scale = MIN_HEIGHT / float(h)

        scale = min(
            scale,
            MAX_UPSCALE
        )

        new_w = max(
            1,
            int(w * scale)
        )

        new_h = max(
            1,
            int(h * scale)
        )

        image = cv2.resize(
            image,
            (new_w, new_h),
            interpolation=cv2.INTER_CUBIC
        )

    return image


def unsharp(image):

    blur = cv2.GaussianBlur(
        image,
        (0, 0),
        1.2
    )

    result = cv2.addWeighted(
        image,
        1.20,
        blur,
        -0.20,
        0
    )

    return result


def normalize_lighting(gray):

    gray = gray.astype(
        np.float32
    )

    background = cv2.GaussianBlur(
        gray,
        (0, 0),
        sigmaX=15,
        sigmaY=15
    )

    normalized = (
        gray /
        (background + 1.0)
    ) * 128.0

    normalized = np.clip(
        normalized,
        0,
        255
    ).astype(
        np.uint8
    )

    return normalized


def clahe_process(image):

    gray = cv2.cvtColor(
        image,
        cv2.COLOR_BGR2GRAY
    )

    clahe = cv2.createCLAHE(
        clipLimit=2.0,
        tileGridSize=(8, 8)
    )

    result = clahe.apply(
        gray
    )

    result = unsharp(
        result
    )

    return cv2.cvtColor(
        result,
        cv2.COLOR_GRAY2BGR
    )


def normalized_gray(image):

    gray = cv2.cvtColor(
        image,
        cv2.COLOR_BGR2GRAY
    )

    gray = normalize_lighting(
        gray
    )

    gray = unsharp(
        gray
    )

    return cv2.cvtColor(
        gray,
        cv2.COLOR_GRAY2BGR
    )

def otsu_process(image):

    gray = cv2.cvtColor(
        image,
        cv2.COLOR_BGR2GRAY
    )

    gray = cv2.GaussianBlur(
        gray,
        (3, 3),
        0
    )

    binary = cv2.threshold(
        gray,
        0,
        255,
        cv2.THRESH_BINARY
        + cv2.THRESH_OTSU
    )[1]

    if np.mean(binary) < 100:

        binary = cv2.bitwise_not(
            binary
        )

    return cv2.cvtColor(
        binary,
        cv2.COLOR_GRAY2BGR
    )


def adaptive_process(image):

    gray = cv2.cvtColor(
        image,
        cv2.COLOR_BGR2GRAY
    )

    gray = cv2.GaussianBlur(
        gray,
        (3, 3),
        0
    )

    binary = cv2.adaptiveThreshold(
        gray,
        255,
        cv2.ADAPTIVE_THRESH_GAUSSIAN_C,
        cv2.THRESH_BINARY,
        31,
        11
    )

    if np.mean(binary) < 100:

        binary = cv2.bitwise_not(
            binary
        )

    return cv2.cvtColor(
        binary,
        cv2.COLOR_GRAY2BGR
    )


def generate_candidates(image):
    """Lazy candidates: chỉ tạo preprocessing khi vòng OCR cần."""
    if image is None:
        return

    if USE_RGB:
        yield "rgb", image

    if USE_GRAY:
        gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
        gray = unsharp(gray)
        yield "gray", cv2.cvtColor(gray, cv2.COLOR_GRAY2BGR)

    if USE_CLAHE:
        yield "clahe", clahe_process(image)

    if USE_OTSU:
        yield "otsu", otsu_process(image)

    if USE_ADAPTIVE:
        yield "adaptive", adaptive_process(image)


def predict_vietocr(
    predictor,
    image
):

    if image is None:
        return ""

    rgb = cv2.cvtColor(
        image,
        cv2.COLOR_BGR2RGB
    )

    pil_image = Image.fromarray(
        rgb
    )

    try:

        text = predictor.predict(
            pil_image
        )

        if text is None:
            return ""

        return str(text).strip()

    except Exception as e:

        print(
            "[OCR ERROR]",
            e
        )

        return ""


def normalize_text(text):

    if not text:
        return ""

    text = text.strip()

    text = re.sub(
        r"\s+",
        " ",
        text
    )

    text = re.sub(
        r"\s+([,.!?;:%])",
        r"\1",
        text
    )

    text = re.sub(
        r"([(\[])\s+",
        r"\1",
        text
    )

    text = re.sub(
        r"\s+([)\]])",
        r"\1",
        text
    )

    return text.strip()


def text_quality(text):

    if not text:
        return -100

    text = normalize_text(
        text
    )

    if not text:
        return -100

    score = 0.0

    length = len(text)

    if length == 1:
        score -= 20

    elif length == 2:
        score -= 5

    elif 3 <= length <= 100:
        score += 15

    elif length > 100:
        score += 5


    valid = 0
    alphabetic = 0
    digits = 0
    punctuation = 0
    weird = 0

    for ch in text:

        if VALID_CHAR_PATTERN.fullmatch(ch):
            valid += 1

        category = unicodedata.category(
            ch
        )

        if category.startswith("L"):
            alphabetic += 1

        elif category.startswith("N"):
            digits += 1

        elif category.startswith("P"):
            punctuation += 1

        elif not ch.isspace():
            weird += 1

    ratio = valid / max(
        1,
        length
    )

    score += ratio * 35

    if length > 2:

        alpha_ratio = alphabetic / length

        if alpha_ratio > 0.4:
            score += 10

        if alpha_ratio > 0.7:
            score += 5

    vietnamese_specific = (
        "ăâđêôơư"
        "ĂÂĐÊÔƠƯ"
    )

    viet_count = sum(
        1
        for ch in text
        if ch in vietnamese_specific
    )

    if viet_count > 0:
        score += min(
            12,
            viet_count * 2
        )

    if digits > 0:

        digit_ratio = digits / length

        if digit_ratio > 0.85:
            score -= 3


    if punctuation > 0:

        punctuation_ratio = (
            punctuation /
            max(1, length)
        )

        if punctuation_ratio > 0.35:
            score -= 20

        elif punctuation_ratio > 0.20:
            score -= 8


    score -= weird * 8


    if re.search(
        r"(.)\1\1+",
        text
    ):
        score -= 20

    words = text.split()

    if len(words) >= 2:

        for i in range(
            len(words) - 1
        ):

            if words[i].lower() == words[i + 1].lower():
                score -= 15

    if re.search(
        r"[^A-Za-zÀ-ỹĐđ0-9\s,.!?;:%()\-_/]",
        text
    ):
        score -= 10

    try:

        normalized = unicodedata.normalize(
            "NFC",
            text
        )

        if normalized != text:
            score -= 2

    except Exception:
        pass

    return score


def extract_detection_result(result):

    polygons = []
    scores = []

    try:

        data = result.json

        if callable(data):
            data = data()

        if isinstance(data, str):
            data = json.loads(data)

        if isinstance(data, dict):

            inner = data

            if "res" in data:
                inner = data["res"]

            if isinstance(inner, dict):

                polygons = (
                    inner.get("dt_polys")
                    or inner.get("polys")
                    or inner.get("points")
                    or []
                )

                scores = (
                    inner.get("dt_scores")
                    or inner.get("scores")
                    or []
                )

    except Exception:
        pass

    if not polygons:

        for name in [
            "dt_polys",
            "polys",
            "points"
        ]:

            if hasattr(
                result,
                name
            ):

                value = getattr(
                    result,
                    name
                )

                if value is not None:
                    polygons = value
                    break

    if not scores:

        for name in [
            "dt_scores",
            "scores"
        ]:

            if hasattr(
                result,
                name
            ):

                value = getattr(
                    result,
                    name
                )

                if value is not None:
                    scores = value
                    break

    clean_polygons = []

    for poly in polygons:

        try:

            arr = np.asarray(
                poly,
                dtype=np.float32
            )

            if arr.ndim == 2 and arr.shape[0] >= 4:

                clean_polygons.append(
                    arr
                )

        except Exception:
            continue

    clean_scores = []

    for score in scores:

        try:
            clean_scores.append(
                float(score)
            )
        except Exception:
            clean_scores.append(
                0.0
            )

    return (
        clean_polygons,
        clean_scores
    )

def polygon_bbox(poly):

    poly = np.asarray(
        poly
    )

    x1 = float(
        np.min(poly[:, 0])
    )

    y1 = float(
        np.min(poly[:, 1])
    )

    x2 = float(
        np.max(poly[:, 0])
    )

    y2 = float(
        np.max(poly[:, 1])
    )

    return (
        x1,
        y1,
        x2,
        y2
    )


def bbox_iou(
    a,
    b
):

    ax1, ay1, ax2, ay2 = a
    bx1, by1, bx2, by2 = b

    ix1 = max(
        ax1,
        bx1
    )

    iy1 = max(
        ay1,
        by1
    )

    ix2 = min(
        ax2,
        bx2
    )

    iy2 = min(
        ay2,
        by2
    )

    iw = max(
        0,
        ix2 - ix1
    )

    ih = max(
        0,
        iy2 - iy1
    )

    inter = iw * ih

    area_a = max(
        0,
        ax2 - ax1
    ) * max(
        0,
        ay2 - ay1
    )

    area_b = max(
        0,
        bx2 - bx1
    ) * max(
        0,
        by2 - by1
    )

    union = (
        area_a +
        area_b -
        inter
    )

    if union <= 0:
        return 0.0

    return inter / union


def remove_duplicate_boxes(
    polygons,
    scores
):

    items = []

    for i, poly in enumerate(
        polygons
    ):

        score = (
            scores[i]
            if i < len(scores)
            else 0.0
        )

        items.append(
            (
                poly,
                score
            )
        )

    items.sort(
        key=lambda x: x[1],
        reverse=True
    )

    kept = []

    for poly, score in items:

        bbox = polygon_bbox(
            poly
        )

        duplicate = False

        for kept_poly, _ in kept:

            kept_bbox = polygon_bbox(
                kept_poly
            )

            if bbox_iou(
                bbox,
                kept_bbox
            ) >= DUPLICATE_IOU:

                duplicate = True
                break

        if not duplicate:

            kept.append(
                (
                    poly,
                    score
                )
            )

    return (
        [x[0] for x in kept],
        [x[1] for x in kept]
    )


def box_geometry(poly):

    x1, y1, x2, y2 = polygon_bbox(
        poly
    )

    width = max(
        1.0,
        x2 - x1
    )

    height = max(
        1.0,
        y2 - y1
    )

    cx = (
        x1 + x2
    ) / 2

    cy = (
        y1 + y2
    ) / 2

    return (
        x1,
        y1,
        x2,
        y2,
        width,
        height,
        cx,
        cy
    )


def group_into_lines(
    polygons,
    scores
):

    items = []

    for i, poly in enumerate(
        polygons
    ):

        (
            x1,
            y1,
            x2,
            y2,
            width,
            height,
            cx,
            cy
        ) = box_geometry(poly)

        score = (
            scores[i]
            if i < len(scores)
            else 0.0
        )

        items.append(
            {
                "poly": poly,
                "score": score,
                "x1": x1,
                "y1": y1,
                "x2": x2,
                "y2": y2,
                "width": width,
                "height": height,
                "cx": cx,
                "cy": cy
            }
        )

    items.sort(
        key=lambda x: (
            x["cy"],
            x["x1"]
        )
    )

    lines = []

    for item in items:

        placed = False

        for line in lines:

            avg_cy = np.mean(
                [
                    x["cy"]
                    for x in line
                ]
            )

            avg_height = np.mean(
                [
                    x["height"]
                    for x in line
                ]
            )

            tolerance = max(
                8,
                avg_height * 0.55
            )

            if abs(
                item["cy"] - avg_cy
            ) <= tolerance:

                line_y1 = min(
                    x["y1"]
                    for x in line
                )

                line_y2 = max(
                    x["y2"]
                    for x in line
                )

                overlap = max(
                    0,
                    min(
                        item["y2"],
                        line_y2
                    )
                    -
                    max(
                        item["y1"],
                        line_y1
                    )
                )

                overlap_ratio = overlap / max(
                    1,
                    min(
                        item["height"],
                        avg_height
                    )
                )

                if overlap_ratio > 0.25:

                    line.append(
                        item
                    )

                    placed = True
                    break

        if not placed:

            lines.append(
                [item]
            )

    for line in lines:

        line.sort(
            key=lambda x: x["x1"]
        )

    lines.sort(
        key=lambda line:
        np.mean(
            [
                x["cy"]
                for x in line
            ]
        )
    )

    return lines


def recognize_best(
    predictor,
    crop
):
    if crop is None:
        return {"text": "", "score": -100, "method": "", "image": None, "all_candidates": []}

    crop = remove_border_noise(crop)
    crop = deskew_line(crop)
    crop = resize_for_ocr(crop)
    crop = add_white_border(crop)

    results = []

    for method, candidate_image in generate_candidates(crop):
        text = normalize_text(predict_vietocr(predictor, candidate_image))
        score = text_quality(text)
        results.append({
            "method": method,
            "text": text,
            "score": score,
            "image": candidate_image
        })

        if score >= OCR_EARLY_ACCEPT_SCORE and len(text) >= 2:
            break

        if FAST_OCR_MODE and score >= FAST_SECONDARY_SCORE and len(text) >= 2:
            break

    if not results:
        return {"text": "", "score": -100, "method": "", "image": crop, "all_candidates": []}

    results.sort(key=lambda x: x["score"], reverse=True)
    best = results[0]

    if best["score"] < LOW_SCORE_THRESHOLD:
        sharpen = unsharp(crop)
        text = normalize_text(predict_vietocr(predictor, sharpen))
        score = text_quality(text)
        results.append({
            "method": "strong_sharp",
            "text": text,
            "score": score,
            "image": sharpen
        })
        results.sort(key=lambda x: x["score"], reverse=True)
        best = results[0]

    return {
        "text": best["text"],
        "score": best["score"],
        "method": best["method"],
        "image": best["image"],
        "all_candidates": [
            {"method": r["method"], "text": r["text"], "score": r["score"]}
            for r in results
        ]
    }


def draw_detection(
    image,
    polygons,
    scores
):

    debug = image.copy()

    for i, poly in enumerate(
        polygons
    ):

        pts = np.asarray(
            poly,
            dtype=np.int32
        )

        pts = pts.reshape(
            (-1, 1, 2)
        )

        cv2.polylines(
            debug,
            [pts],
            True,
            (0, 255, 0),
            2
        )

        x, y = pts[0][0]

        score = (
            scores[i]
            if i < len(scores)
            else 0
        )

        cv2.putText(
            debug,
            f"{i + 1}:{score:.2f}",
            (
                int(x),
                max(20, int(y))
            ),
            cv2.FONT_HERSHEY_SIMPLEX,
            0.5,
            (0, 0, 255),
            1,
            cv2.LINE_AA
        )

    return debug


LAYOUT_TYPES = {"heading", "paragraph", "list", "step"}


def layout_clean_text(text):
    """Làm sạch nhẹ văn bản trước khi đưa vào layout."""
    text = normalize_text(text or "")
    text = re.sub(r"[ \t]+", " ", text).strip()
    return text


def _line_geometry(line):
    bbox = line.get("bbox") or [0, 0, 0, 0]
    x1, y1, x2, y2 = [float(v) for v in bbox]
    return {
        "x": x1,
        "y": y1,
        "right": x2,
        "bottom": y2,
        "width": max(0.0, x2 - x1),
        "height": max(1.0, y2 - y1),
    }


def finalize_layout_block(lines):
    """Gom các dòng thành một block văn bản."""
    if not lines:
        return None

    geo = [_line_geometry(line) for line in lines]
    x = min(g["x"] for g in geo)
    y = min(g["y"] for g in geo)
    right = max(g["right"] for g in geo)
    bottom = max(g["bottom"] for g in geo)

    return {
        "text": "\n".join(
            layout_clean_text(line.get("text", ""))
            for line in lines
            if layout_clean_text(line.get("text", ""))
        ).strip(),
        "x": x,
        "y": y,
        "width": right - x,
        "height": bottom - y,
        "right": right,
        "bottom": bottom,
        "lines": lines,
    }


def classify_layout_block(block):
    """Nhận diện loại block bằng hình học + kết quả nhận diện từng dòng."""
    text = layout_clean_text(block.get("text", ""))
    if not text:
        return "empty"

    neural_types = []
    for line in block.get("lines", []):
        typ = line.get("neural_layout")
        conf = float(line.get("neural_confidence", 0.0) or 0.0)
        if typ in LAYOUT_TYPES and conf >= LAYOUT_NEURAL_MIN_CONF:
            neural_types.append(typ)

    if neural_types:
        counts = {}
        for typ in neural_types:
            counts[typ] = counts.get(typ, 0) + 1
        return max(counts, key=counts.get)

    if re.match(r"^(?:Bước|Buoc)\s+\d+", text, re.IGNORECASE):
        return "step"

    if re.match(r"^[-•▪●◦*]\s*", text):
        return "list"

    if re.match(r"^\d{1,3}[\.)]\s+", text):
        return "list"

    letters = re.sub(r"[^A-Za-zÀ-ỹĐđ]", "", text)
    if letters:
        upper_ratio = sum(c.isupper() for c in letters) / len(letters)
        if upper_ratio > 0.85 and len(text) < 120:
            return "heading"

    if len(text) < 70 and block.get("height", 0) < 120:
        return "heading"

    return "paragraph"


def build_layout_blocks(final_lines):
    """Sắp xếp dòng theo vị trí và gom thành các khối bố cục."""
    if not final_lines:
        return []

    ordered = []
    for index, line in enumerate(final_lines):
        text = layout_clean_text(line.get("text", ""))
        if not text:
            continue
        g = _line_geometry(line)
        item = dict(line)
        item.update(g)
        item["_order"] = index
        ordered.append(item)

    ordered.sort(key=lambda x: (x["y"], x["x"]))

    blocks = []
    current = []

    for line in ordered:
        if not current:
            current = [line]
            continue

        previous = current[-1]
        vertical_gap = line["y"] - previous["bottom"]
        previous_height = max(previous["height"], 1.0)

        if vertical_gap <= previous_height * 0.95:
            current.append(line)
        else:
            block = finalize_layout_block(current)
            if block:
                block["type"] = classify_layout_block(block)
                blocks.append(block)
            current = [line]

    if current:
        block = finalize_layout_block(current)
        if block:
            block["type"] = classify_layout_block(block)
            blocks.append(block)

    return blocks


def _estimate_layout_columns(blocks, image_width):
    values = []
    for block in blocks:
        for line in block.get("lines", []):
            text = line.get("text", "")
            if not text:
                continue
            value = line.get("width", 0) / max(len(text), 1)
            if 2 <= value <= 50:
                values.append(value)

    char_width = sum(values) / len(values) if values else 8.0
    columns = int(image_width / max(char_width, 6.0))
    return max(60, min(columns, 180))


def build_layout_text(final_lines, image_width=None):
    """Tạo văn bản cuối cùng có cấu trúc heading/paragraph/list/step."""
    blocks = build_layout_blocks(final_lines)
    if not blocks:
        return ""

    if not image_width:
        image_width = max(
            (float(item.get("right", 0)) for item in blocks),
            default=1.0,
        )

    columns = _estimate_layout_columns(blocks, image_width)
    result = []
    previous_block = None

    for block in blocks:
        block_type = block.get("type", "paragraph")

        if previous_block is not None:
            gap = block["y"] - previous_block["bottom"]
            previous_height = max(previous_block["height"], 1.0)
            if gap > previous_height * 0.8:
                if not result or result[-1] != "":
                    result.append("")

        for line in block.get("lines", []):
            text = layout_clean_text(line.get("text", ""))
            if not text:
                continue

            indent = int((line["x"] / max(float(image_width), 1.0)) * columns)
            indent = max(0, min(indent, columns - 1))

            if block_type == "heading":
                center = line["x"] + line["width"] / 2.0
                image_center = float(image_width) / 2.0
                if abs(center - image_center) < float(image_width) * 0.15:
                    text_indent = max(0, int((columns - len(text)) / 2))
                    formatted = " " * text_indent + text
                else:
                    formatted = " " * indent + text
            else:
                formatted = " " * indent + text

            result.append(formatted.rstrip())

        if block_type in {"heading", "list", "step"}:
            if not result or result[-1] != "":
                result.append("")

        previous_block = block

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

    while cleaned and cleaned[0] == "":
        cleaned.pop(0)
    while cleaned and cleaned[-1] == "":
        cleaned.pop()

    return "\n".join(cleaned)

_MODEL_CACHE = {"detector": None, "predictor": None, "layout_model": None}


#def get_models():
#    if _MODEL_CACHE["detector"] is None:
#        _MODEL_CACHE["detector"] = load_detector()
#    if _MODEL_CACHE["predictor"] is None:
#        _MODEL_CACHE["predictor"] = load_vietocr()
#    if _MODEL_CACHE["layout_model"] is None:
#        _MODEL_CACHE["layout_model"] = load_layout_model()
#    return (_MODEL_CACHE["detector"], _MODEL_CACHE["predictor"], _MODEL_CACHE["layout_model"])
def get_models():

    if _MODEL_CACHE["detector"] is None:
        _MODEL_CACHE["detector"] = load_detector()

    if _MODEL_CACHE["predictor"] is None:
        _MODEL_CACHE["predictor"] = load_vietocr()

    # Bỏ hoàn toàn neural layout
    if LAYOUT_NEURAL_ENABLED:
        if _MODEL_CACHE["layout_model"] is None:
            _MODEL_CACHE["layout_model"] = load_layout_model()
    else:
        _MODEL_CACHE["layout_model"] = None

    return (
        _MODEL_CACHE["detector"],
        _MODEL_CACHE["predictor"],
        _MODEL_CACHE["layout_model"]
    )

def preload_models():
    return get_models()


def _pipeline(image_path):
    if not image_path or not os.path.exists(image_path):
        raise FileNotFoundError(f"Không tìm thấy ảnh: {image_path}")

    total_start = time.perf_counter()
    image = cv2.imread(image_path)
    if image is None:
        raise ValueError("Không đọc được ảnh")

    h, w = image.shape[:2]
    detector, predictor, layout_model = get_models()

    detection_start = time.perf_counter()
    outputs = detector.predict(image_path)
    all_polygons, all_scores = [], []
    for result in outputs:
        polygons, scores = extract_detection_result(result)
        all_polygons.extend(polygons)
        all_scores.extend(scores)

    filtered_polygons, filtered_scores = [], []
    for i, poly in enumerate(all_polygons):
        score = all_scores[i] if i < len(all_scores) else 0
        if score < DET_THRESHOLD:
            continue
        x1, y1, x2, y2, width, height, cx, cy = box_geometry(poly)
        if width < 5 or height < 5 or width / max(height, 1) > 150:
            continue
        filtered_polygons.append(poly)
        filtered_scores.append(score)

    filtered_polygons, filtered_scores = remove_duplicate_boxes(filtered_polygons, filtered_scores)
    lines = group_into_lines(filtered_polygons, filtered_scores)

    final_lines = []
    ocr_start = time.perf_counter()
    line_index = 0

    for line in lines:
        line_index += 1
        line_results = []
        for item in line:
            crop = perspective_crop(image, item["poly"])
            if crop is None:
                crop = bbox_crop(image, item["poly"])
            if crop is None:
                continue
            result = recognize_best(predictor, crop)
            if result["text"]:
                line_results.append(result)

        if line_results:
            line_text = normalize_text(" ".join(r["text"] for r in line_results if r["text"]))
            if line_text:
                final_lines.append({
                    "line": line_index,
                    "text": line_text,
                    "bbox": [
                        float(min(x["x1"] for x in line)),
                        float(min(x["y1"] for x in line)),
                        float(max(x["x2"] for x in line)),
                        float(max(x["y2"] for x in line))
                    ],
                    "layout": "paragraph",
                    "parts": [
                        {
                            "text": r["text"],
                            "method": r["method"],
                            "score": r["score"],
                            "candidates": r.get("all_candidates", []),
                            "bbox": [float(item["x1"]), float(item["y1"]), float(item["x2"]), float(item["y2"])]
                        }
                        for r in line_results
                    ]
                })

    ocr_time = time.perf_counter() - ocr_start
    final_lines = apply_layout_recognition(image, final_lines, w, layout_model)
    final_text = build_layout_text(final_lines, w)
    total_time = time.perf_counter() - total_start

    return {
        "text": final_text,
        "layout_text": final_text,
        "lines": final_lines,
        "ocr_time": ocr_time,
        "total_time": total_time,
        "detection_time": detection_start and (ocr_start - detection_start),
    }


class HandwritingOCRModel:
    def __init__(self):
        self.ready = False
        self.detector, self.predictor, self.layout_model = get_models()
        self.ready = True

    def predict_file(self, path):
        return _pipeline(path)

    def predict(self, image):
        import tempfile
        temp_path = None
        try:
            with tempfile.NamedTemporaryFile(suffix=".png", delete=False) as f:
                temp_path = f.name
            if not cv2.imwrite(temp_path, image):
                raise ValueError("Không thể ghi ảnh tạm")
            return self.predict_file(temp_path)
        finally:
            if temp_path and os.path.exists(temp_path):
                try:
                    os.remove(temp_path)
                except Exception:
                    pass

    def predict_bytes(self, image_bytes):
        buffer = np.frombuffer(image_bytes, dtype=np.uint8)
        image = cv2.imdecode(buffer, cv2.IMREAD_COLOR)
        if image is None:
            raise ValueError("Không thể đọc dữ liệu ảnh")
        return self.predict(image)

    def health(self):
        return {
            "model": "handwriting",
            "detector": "PaddleOCR PP-OCRv5_mobile_det",
            "recognizer": "VietOCR vgg_transformer",
            "device": "cpu",
            "offline": True,
            "ready": self.ready,
        }


OCRHandwritingModel = HandwritingOCRModel


if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Cách dùng: python handwriting_model.py <image>")
        sys.exit(1)
    model = HandwritingOCRModel()
    result = model.predict_file(sys.argv[1])
    print(result["text"])
    print(f"\
Thời gian OCR: {result['ocr_time']:.3f} giây")
    print(f"Tổng thời gian: {result['total_time']:.3f} giây")