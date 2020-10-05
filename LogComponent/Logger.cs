using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace LogComponent
{
    /// <summary>
    /// Thread safe ILog implementation, for each call to Write, the logger appends the current time-stamp(local), and suffixes the text with a new line.
    /// </summary>
    public sealed class Logger : ILog, IDisposable
    {
        private readonly CancellationTokenSource _stop = new CancellationTokenSource();
        private readonly CancellationTokenSource _softStop = new CancellationTokenSource();
        private readonly ConcurrentQueue<string> _lines = new ConcurrentQueue<string>();
        private readonly ILogWriter _writer; //can be extended to use multiple writers
        private readonly Task _runner;

        /// <summary>
        /// Creates a Logger instance.
        /// </summary>
        /// <param name="writer">Used to specify the place where the logs are going to get written</param>
        public Logger(ILogWriter writer)
        {
            _writer = writer;
            _runner = Task.Run(DumpLines, _stop.Token);
        }

        /// <inheritdoc/>
        public void StopWithFlush()
        {
            _softStop.Cancel();
            _runner.Wait();
            _writer.Flush();
        }

        /// <inheritdoc/>
        public void StopWithoutFlush()
        {
            _stop.Cancel();
        }

        /// <inheritdoc/>
        public void Write(string text)
        {
            _lines.Enqueue($"{DateTime.Now:yyyy-MM-dd HH:mm:ss:fff} {text}.{Environment.NewLine}"); // if needed the formatter can be injected as well
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _stop.Cancel();
            _stop.Dispose();
            _runner.Dispose();
        }

        /// <summary>
        /// Recurrent task that consumes the lines form the queue and writes them to a logger writer.
        /// </summary>
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
                        // if that is the case the Exception&IsFaulted(from the runner) fields should be exposed to caller of the API.
                        Console.WriteLine($"Thrown exception while writhing the following log: {line} LogWriter Exception: {ex.Message}");
                    }
                }
                await Task.Delay(TimeSpan.FromMilliseconds(50), _stop.Token).ConfigureAwait(false);
            }
        }
    }
}
