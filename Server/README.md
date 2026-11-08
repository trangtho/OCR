# Cài đặt và chạy chương trình

## 1. Cài đặt môi trường

### Bước 1: Đi tới thư mục dự án

Di chuyển đến thư mục chứa file chương trình:

```bash
cd .../PythonServer #Đường dẫn đến thư mục PythonServer
```

### Bước 2: Tạo môi trường ảo

**macOS / Linux:**

```bash
python3 -m venv venv
```

**Windows:**

```bash
python -m venv venv
```

Sau khi thực hiện, thư mục `venv` sẽ được tự động tạo trong thư mục dự án:

```text
PythonServer/
├── venv/
...

```

### Bước 3: Kích hoạt môi trường ảo

**macOS / Linux:**

```bash
source venv/bin/activate
```

**Windows - CMD:**

```cmd
venv\Scripts\activate
```

**Windows - PowerShell:**

```powershell
.\venv\Scripts\Activate.ps1
```

Nếu kích hoạt thành công, Terminal sẽ hiển thị:

```text
(venv)
```

---

## 2. Cài đặt thư viện

Sau khi kích hoạt môi trường ảo, cài đặt các thư viện trong `requirements.txt`:

**macOS / Linux:**

```bash
pip3 install -r requirements.txt
```

**Windows:**

```bash
pip install -r requirements.txt
```

---

## 3. Chạy chương trình

### macOS / Linux

```bash
python3 ocrConfig.py.py -i <duongDanAnh>
```

### Windows

```bash
python ocrConfig.py -i <duongDanAnh>
```

Trong đó `<duongDanAnh>` là đường dẫn đến file ảnh cần xử lý.

---

## 4. Thoát môi trường ảo

Sau khi sử dụng xong, có thể thoát môi trường ảo bằng lệnh:

```bash
deactivate
```