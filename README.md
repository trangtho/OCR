# OCR Project - Nhận dạng văn bản từ hình ảnh

> Hệ thống nhận dạng ký tự quang học (OCR) theo mô hình Client - Server: ứng dụng Windows chụp/chọn ảnh, gửi lên server Python để nhận dạng văn bản và xuất kết quả ra file Word.

## Giới thiệu

Đồ án xây dựng một hệ thống OCR hỗ trợ **tiếng Việt**, gồm hai phần:

- **Client**: ứng dụng desktop viết bằng C# (Windows Forms), cho phép người dùng chọn ảnh hoặc chụp ảnh từ camera, gửi đến server và hiển thị kết quả.
- **Server**: dịch vụ Python chạy mô hình học sâu (VGG + Transformer) để nhận dạng văn bản trong ảnh và trả kết quả về cho client.

Mục tiêu: [số hóa tài liệu / trích xuất nội dung từ ảnh chụp / ...].

## Tính năng chính

- Nhận dạng văn bản từ ảnh tải lên hoặc ảnh chụp trực tiếp từ camera
- Hiển thị văn bản nhận dạng ngay trên giao diện
- Xuất kết quả ra file Word (`.docx`)
- Giao diện tùy chỉnh theme (SkyBlue)
- Có bộ cài đặt (installer) cho ứng dụng Client
- [Thêm tính năng khác nếu có]

## Kiến trúc hệ thống

```
┌────────────────┐    ảnh (HTTP)     ┌──────────────────┐
│  Client (C#)   │ ────────────────► │  Server (Python) │
│  WinForms      │                   │  Mô hình OCR     │
│                │ ◄──────────────── │  VGG-Transformer │
└────────────────┘  văn bản kết quả  └──────────────────┘
```

## Công nghệ sử dụng

| Thành phần | Công nghệ |
|------------|-----------|
| Client | C#, .NET, Windows Forms, Inno Setup (đóng gói cài đặt) |
| Server | Python, PyTorch, mô hình VGG-Transformer, [Flask / FastAPI] |
| Xuất file | Thư viện tạo `.docx` trong Client |

## Cấu trúc thư mục

```
OCRProject/
├── Client/
│   └── OCRClient_SkyBlue/
│       └── OCRClient/
│           ├── Forms/        # Giao diện: OcrForm, CameraForm
│           ├── Services/     # OcrService, DocxExporter, AppSettings
│           ├── Models/       # Các lớp dữ liệu
│           ├── Themes/       # UiTheme
│           └── installer/    # Script đóng gói (OCRClient.iss)
├── Server/
│   ├── config/               # Cấu hình mô hình (base.yml, vgg_transformer.yml)
│   ├── model.py              # Nạp và chạy mô hình
│   ├── requirements.txt
│   └── README.md             # Hướng dẫn chi tiết cho Server
└── README.md
```

## Cài đặt và chạy

### 1. Clone project

```bash
git clone https://github.com/username/ocr.git
cd ocr
```

### 2. Chạy Server

```bash
cd Server
python -m venv venv
venv\Scripts\activate
pip install -r requirements.txt
```

Tải file weights của mô hình tại: **[dán link Google Drive]** và đặt vào thư mục `Server/weights/` (file này không được đưa lên GitHub do dung lượng lớn).

Khởi động server:

```bash
python [tên_file_chạy_server].py
```

Server mặc định chạy tại `http://localhost:[cổng]`.

### 3. Chạy Client

1. Mở file `.sln` / `.csproj` trong **Visual Studio** (cần cài workload *.NET desktop development*).
2. Cấu hình địa chỉ server trong phần cài đặt của ứng dụng (mặc định `http://localhost:[cổng]`).
3. Nhấn **F5** để chạy.

Hoặc dùng bộ cài đặt ở `Client/.../installer/` để cài ứng dụng lên máy.

## Hướng dẫn sử dụng

1. Khởi động Server.
2. Mở ứng dụng Client.
3. Chọn ảnh từ máy hoặc mở camera để chụp.
4. Nhấn nút nhận dạng, chờ kết quả hiển thị.
5. Chỉnh sửa văn bản nếu cần và xuất ra file Word.

## Kết quả demo

| Ảnh đầu vào | Văn bản nhận dạng |
|-------------|-------------------|
| [Chèn ảnh]  | [Chèn kết quả]    |

Ảnh chụp giao diện: [chèn ảnh vào thư mục `docs/` rồi dán đường dẫn]

## Hạn chế và hướng phát triển

- Độ chính xác giảm với ảnh mờ, nghiêng, thiếu sáng hoặc chữ viết tay
- Cần chạy Server trước khi dùng Client
- Hướng phát triển: [hỗ trợ thêm ngôn ngữ, cải thiện tiền xử lý ảnh, triển khai server lên cloud, bản web/mobile...]

## Thành viên thực hiện

| Họ tên | MSSV | Vai trò |
|--------|------|---------|
| [Tên]  | [...] | [...]  |

## Giảng viên hướng dẫn

[Tên giảng viên - Môn học - Trường]
