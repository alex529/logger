using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace LogComponent
{
    public sealed class Logger : ILog, IDisposable
    {
        private readonly CancellationTokenSource _stop = new CancellationTokenSource();
        private readonly CancellationTokenSource _softStop = new CancellationTokenSource();
        private readonly ConcurrentQueue<string> _lines = new ConcurrentQueue<string>();
        private readonly ILogWriter _writer;
        private readonly Task _runner;

        public Logger(ILogWriter writer)
        {
            _writer = writer;
            _runner = Task.Run(DumpLines, _stop.Token);
        }
        public void StopWithFlush()
        {
            _softStop.Cancel();
            _runner.Wait();
            _writer.Flush();
        }

        public void StopWithoutFlush()
        {
            _stop.Cancel();
        }

        public void Write(string text)
        {
            _lines.Enqueue($"{DateTime.Now:yyyy-MM-dd HH:mm:ss:fff} {text}.{Environment.NewLine}"); // if needed the formatter can be injected as well
        }

        public void Dispose()
        {
            _stop.Cancel();
            _stop.Dispose();
            _runner.Dispose();
        }

        private async Task DumpLines()
        {
            while (!_softStop.IsCancellationRequested)
            {
                while (!_lines.IsEmpty && _lines.TryDequeue(out var line))
                {
                    try
                    {
                        _writer.Write(line);
                    }
                    catch (Exception ex) // almost always a bad idea?
                    {
                        // I don't really like this but i can't think of a better way to satisfying requirement number 4
                        // if the try catch is not here the program will not crush, as the exception would be captured by the task
                        // but it will fail on all subsequent calls to write, which maybe should be the intended behavior?
                        Console.WriteLine($"Thrown exception while writhing the following log: {line} LogWriter Exception: {ex.Message}");
                    }
                }
                await Task.Delay(TimeSpan.FromMilliseconds(50), _stop.Token).ConfigureAwait(false);
            }
        }
    }
}
