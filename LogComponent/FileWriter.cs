using System;
using System.IO;
using System.Text;

namespace LogComponent
{
    /// <summary>
    /// It implements the ILogWriter interface and is meant to be used in combination with the Logger Class.
    /// The writer creates or appends to an existing file based on the time-stamp. On day transition a new
    /// file is created and the text starts to be written to the new file.
    /// </summary>
    public sealed class FileWriter : IDisposable, ILogWriter
    {
        private readonly string _folderPath;
        private readonly string _prefix;
        private FileStream _writer;
        private Func<DateTime> _nowProvider;

        /// <summary>
        /// Creates a new file writer
        /// </summary>
        /// <param name="folderPath">Used to specify the folder where the files are going to be created. If the folder doesn't exist it will be created.</param>
        /// <param name="filePrefix">A prefix is added to new files, output file format:<prefix>-yyyyMMdd.log</param>
        public FileWriter(string folderPath, string filePrefix)
        {
            _folderPath = folderPath;
            if (!Directory.Exists(_folderPath)) // if the path for the logger doesn't exist create it
            {
                Directory.CreateDirectory(_folderPath);
            }
            _prefix = filePrefix;
            _nowProvider = () => DateTime.Now;
        }

        /// <inheritdoc/>
        public string Header { get; set; }

        /// <summary>
        /// Change the provider of the current time.
        /// </summary>
        /// <param name="value">Used to specify a new provider of the current times-tamp</param>
        public void SetNowProvider(Func<DateTime> value) => _nowProvider = value;

        /// <inheritdoc/>
        public void Dispose() => _writer.Dispose();

        /// <inheritdoc/>
        public void Flush() => _writer.Flush();

        /// <inheritdoc/>
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
            if (_writer == null) // file exists but the writer wasn't initialized (system restarted or something crashed)
            {
                _writer = new FileStream(fileName, FileMode.Open, FileAccess.Write, FileShare.Read);
            }

            var bytes = Encoding.UTF8.GetBytes(text);
            _writer.Write(bytes, 0, bytes.Length);
        }
    }
}
