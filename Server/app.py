"""
Flask server cho Vietnamese OCR
- /ocr              : OCR chữ in
- /ocr-handwriting  : OCR chữ viết tay
- /health           : kiểm tra server
"""

import os
import io
import contextlib
import tempfile
import time
import uuid

from flask import Flask, jsonify, request
from flask_cors import CORS

from model import OCRModel
from handwriting_model import HandwritingOCRModel


# ============================================================
# FLASK
# ============================================================

app = Flask(__name__)

CORS(
    app,
    resources={
        r"/*": {
            "origins": "*"
        }
    }
)


# ============================================================
# LOAD PRINTED OCR MODEL
# ============================================================

try:

    # Ẩn log dài của model khi khởi tạo
    with contextlib.redirect_stdout(io.StringIO()), \
         contextlib.redirect_stderr(io.StringIO()):

        ocr_model = OCRModel()

    _init_error = None

    print("[OK] Printed OCR model loaded")

except Exception as e:

    ocr_model = None
    _init_error = str(e)

    print("[ERROR] Printed OCR:", e)


# ============================================================
# LOAD HANDWRITING OCR MODEL
# ============================================================

try:

    # Load PaddleOCR + VietOCR ngay khi server start
    with contextlib.redirect_stdout(io.StringIO()), \
         contextlib.redirect_stderr(io.StringIO()):

        handwriting_model = HandwritingOCRModel()

    _handwriting_init_error = None

    print("[OK] Handwriting OCR model loaded")

except Exception as e:

    handwriting_model = None
    _handwriting_init_error = str(e)

    print("[ERROR] Handwriting OCR:", e)


# ============================================================
# TEMP UPLOAD DIRECTORY
# ============================================================

UPLOAD_TMP_DIR = os.path.join(
    tempfile.gettempdir(),
    "ocr_api_uploads"
)

os.makedirs(
    UPLOAD_TMP_DIR,
    exist_ok=True
)


ALLOWED_EXTENSIONS = {
    ".png",
    ".jpg",
    ".jpeg",
    ".bmp",
    ".tif",
    ".tiff",
    ".webp"
}


# ============================================================
# HELPERS
# ============================================================

def _save_upload_to_temp(upload):

    _, ext = os.path.splitext(
        upload.filename or ""
    )

    ext = ext.lower()

    if ext not in ALLOWED_EXTENSIONS:

        return None, (
            f"Định dạng file không được hỗ trợ: '{ext}'. "
            f"Chỉ chấp nhận: "
            f"{', '.join(sorted(ALLOWED_EXTENSIONS))}"
        )

    temp_name = f"{uuid.uuid4().hex}{ext}"

    temp_path = os.path.join(
        UPLOAD_TMP_DIR,
        temp_name
    )

    upload.save(temp_path)

    return temp_path, None


def _str_to_bool(value, default):

    if value is None:
        return default

    return value.strip().lower() in (
        "1",
        "true",
        "yes",
        "on"
    )


# ============================================================
# HEALTH
# ============================================================

@app.route(
    "/health",
    methods=["GET"]
)
def health():

    return jsonify({

        "status":
            "ok"
            if ocr_model is not None
            else "error",

        "printed_ocr": {

            "model_ready":
                ocr_model is not None,

            "error":
                _init_error

        },

        "handwriting_ocr": {

            "model_ready":
                handwriting_model is not None,

            "error":
                _handwriting_init_error

        }

    })


# ============================================================
# OCR CHỮ IN
# ============================================================

@app.route(
    "/ocr",
    methods=["POST"]
)
def ocr():

    if ocr_model is None:

        return jsonify({
            "error":
                f"OCR model chưa khởi tạo được: "
                f"{_init_error}"
        }), 500


    if "file" not in request.files:

        return jsonify({
            "error":
                "Thiếu file ảnh, cần gửi "
                "form-data với field 'file'."
        }), 400


    image_file = request.files["file"]


    if not image_file.filename:

        return jsonify({
            "error":
                "File ảnh trống hoặc không hợp lệ."
        }), 400


    include_layout = _str_to_bool(
        request.args.get(
            "include_layout"
        ),
        True
    )


    include_lines = _str_to_bool(
        request.args.get(
            "include_lines"
        ),
        False
    )


    total_start = time.perf_counter()


    temp_path, err = _save_upload_to_temp(
        image_file
    )


    if err:

        return jsonify({
            "error": err
        }), 400


    try:

        try:

            cv_image = ocr_model.load_image(
                temp_path
            )

        except Exception as e:

            return jsonify({
                "error":
                    f"Lỗi đọc ảnh: {e}"
            }), 400


        try:

            result = ocr_model.predict(
                cv_image
            )

        except Exception as e:

            return jsonify({
                "error":
                    f"Lỗi OCR/Layout: {e}"
            }), 500


    finally:

        if os.path.exists(temp_path):

            os.remove(temp_path)


    response = {

        "text":
            result.get(
                "text",
                ""
            ),

        "layout_text":
            result.get(
                "layout_text",
                ""
            ),

        "confidence":
            result.get(
                "confidence"
            ),

        "quality":
            result.get(
                "quality"
            ),

        "score":
            result.get(
                "score"
            ),

        "psm":
            result.get(
                "psm"
            ),

        "ocr_time":
            result.get(
                "time"
            ),

        "total_time":
            time.perf_counter()
            - total_start

    }


    if include_layout:

        response["layout"] = result.get(
            "layout",
            {}
        )


    if include_lines:

        response["lines"] = result.get(
            "lines",
            []
        )


    return jsonify(response)


# ============================================================
# OCR CHỮ VIẾT TAY
# ============================================================

@app.route(
    "/ocr-handwriting",
    methods=["POST"]
)
def ocr_handwriting():

    if handwriting_model is None:

        return jsonify({

            "success": False,

            "error":
                _handwriting_init_error
                or "Handwriting OCR model chưa được khởi tạo"

        }), 500


    image_file = request.files.get("file")


    if image_file is None:

        return jsonify({

            "success": False,

            "error":
                "Không có file"

        }), 400


    if not image_file.filename:

        return jsonify({

            "success": False,

            "error":
                "Tên file không hợp lệ"

        }), 400


    temp_path = None


    try:

        # ----------------------------------------------------
        # SAVE TEMP FILE
        # ----------------------------------------------------

        _, ext = os.path.splitext(
            image_file.filename
        )

        ext = ext.lower()


        if ext not in ALLOWED_EXTENSIONS:

            return jsonify({

                "success": False,

                "error":
                    "Định dạng file không được hỗ trợ"

            }), 400


        temp_path = os.path.join(

            tempfile.gettempdir(),

            f"handwriting_{uuid.uuid4().hex}{ext}"

        )


        image_file.save(
            temp_path
        )


        print(
            "[HANDWRITING] OCR:",
            image_file.filename
        )


        # ----------------------------------------------------
        # DÙNG MODEL ĐÃ LOAD SẴN
        # ----------------------------------------------------

        result = handwriting_model.predict_file(
            temp_path
        )


        return jsonify({

            "success": True,

            "filename":
                image_file.filename,

            "text":
                result.get(
                    "text",
                    ""
                ),

            "layout_text":
                result.get(
                    "layout_text",
                    ""
                ),

            "lines":
                result.get(
                    "lines",
                    []
                ),

            "ocr_time":
                result.get(
                    "ocr_time",
                    0
                ),

            "total_time":
                result.get(
                    "total_time",
                    0
                )

        })


    except Exception as e:

        import traceback

        traceback.print_exc()


        return jsonify({

            "success": False,

            "error":
                str(e)

        }), 500


    finally:

        if temp_path and os.path.exists(
            temp_path
        ):

            try:
                os.remove(temp_path)
            except Exception:
                pass


# ============================================================
# START SERVER
# ============================================================

if __name__ == "__main__":

    print()
    print("=" * 45)
    print("VIETNAMESE OCR SERVER")
    print("=" * 45)

    print("[OK] /ocr")
    print("[OK] /ocr-handwriting")
    print("[OK] /health")

    print()
    print("http://127.0.0.1:8000")
    print("http://192.168.224.49:8000")

    print("=" * 45)


    app.run(
        host="0.0.0.0",
        port=8000,
        debug=False,
        threaded=True
    )