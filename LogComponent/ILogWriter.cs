namespace LogComponent
{
    public interface ILogWriter
    {
        void Write(string text);

        void Flush();

        string Header { get; set; }
    }
}
