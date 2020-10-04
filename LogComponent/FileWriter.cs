using System;
using System.IO;
using System.Text;

namespace LogComponent
{
    public sealed class FileWriter : IDisposable, ILogWriter
    {
        private readonly string _folderPath;
        private readonly string _prefix;
        private FileStream _writer;
        private Func<DateTime> _nowProvider;

        public FileWriter(string folderPath, string filePrefix)
        {
            _folderPath = folderPath;
            if (!Directory.Exists(_folderPath)) // if the path for the loger doesn't exist create it
            {
                Directory.CreateDirectory(_folderPath);
            }
            _prefix = filePrefix;
            _nowProvider = () => DateTime.Now;
        }

        public string Header { get; set; }

        public void SetNowProvider(Func<DateTime> value) => _nowProvider = value;

        public void Dispose() => _writer.Dispose();

        public void Flush() => _writer.Flush();

        public void Write(string text)
        {
            var fileName = Path.Combine(_folderPath, $"{_prefix}-{_nowProvider():yyyyMMdd}.log");

            if (!File.Exists(fileName)) // check if the file doesn't exists (day transition)
            {
                //cleanup old writer if that is the case
                _writer?.Flush();
                _writer?.Dispose();

                _writer = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read);
                if (!string.IsNullOrEmpty(Header))
                {
                    var header = Encoding.UTF8.GetBytes(Header + Environment.NewLine);
                    _writer.Write(header, 0, header.Length);
                }
            }
            if (_writer == null) // file exists but the writer wasn't initilized (system restarted or something crasshed)
            {
                _writer = new FileStream(fileName, FileMode.Open, FileAccess.Write, FileShare.Read);
            }

            var bytes = Encoding.UTF8.GetBytes(text);
            _writer.Write(bytes, 0, bytes.Length);
        }
    }
}
