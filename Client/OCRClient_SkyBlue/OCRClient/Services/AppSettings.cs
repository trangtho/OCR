using System;
using System.IO;
using System.Xml.Serialization;

namespace OCRClient.Services
{
    /// <summary>
    /// Cài đặt người dùng, lưu tại %AppData%\OCRClient\settings.xml
    /// (mỗi máy / mỗi tài khoản Windows có cấu hình riêng).
    /// </summary>
    public class AppSettings
    {
        public const string DefaultServerUrl = "http://127.0.0.1:8000";

        public const int DefaultTimeoutSeconds = 300;

        /// <summary>Thời gian chờ tối đa cho MỖI lần gọi API OCR (giây).</summary>
        public int TimeoutSeconds { get; set; } = DefaultTimeoutSeconds;
        public string ServerUrl { get; set; } = DefaultServerUrl;

        private static AppSettings current;

        private static readonly string FilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "OCRClient",
            "settings.xml");

        public static AppSettings Current
        {
            get
            {
                if (current == null)
                    current = Load();

                return current;
            }
        }

        private static AppSettings Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    using (FileStream stream = File.OpenRead(FilePath))
                    {
                        AppSettings settings = (AppSettings)
                            new XmlSerializer(typeof(AppSettings)).Deserialize(stream);

                        if (string.IsNullOrWhiteSpace(settings.ServerUrl))
                            settings.ServerUrl = DefaultServerUrl;

                        settings.ServerUrl = settings.ServerUrl.Trim().TrimEnd('/');

                        if (settings.TimeoutSeconds < 10)
                            settings.TimeoutSeconds = DefaultTimeoutSeconds;

                        return settings;
                    }
                }
            }
            catch
            {
                // File hỏng -> dùng mặc định
            }

            return new AppSettings();
        }

        public void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath));

            using (FileStream stream = File.Create(FilePath))
            {
                new XmlSerializer(typeof(AppSettings)).Serialize(stream, this);
            }
        }
    }
}
