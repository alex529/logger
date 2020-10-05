namespace LogComponent
{
    public interface ILogWriter
    {
        /// <summary>
        /// Appends string to a file, if day transition happens, a new file is created.
        /// </summary>
        /// <param name="text">Used to specify the text to be appended</param>
        void Write(string text);

        /// <summary>
        /// Writes the remaining buffer data to the file.
        /// </summary>
        void Flush();

        /// <summary>
        /// When creating a new file a header can be added at the beginning.
        /// </summary>
        string Header { get; set; }
    }
}
