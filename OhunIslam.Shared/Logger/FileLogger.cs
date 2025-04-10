//using System;
//using System.IO;
//using Microsoft.Extensions.Logging;

//public class FileLoggerProvider : ILoggerProvider
//{
//    private readonly string _filePath;

//    public FileLoggerProvider(string filePath) =>  _filePath = filePath;
    

//    public ILogger CreateLogger(string categoryName) => new FileLogger(_filePath);

//    public void Dispose() { }

//    private class FileLogger : ILogger
//    {
//        private readonly string _filePath;

//        public FileLogger(string filePath) => _filePath = filePath;

//        public IDisposable? BeginScope<TState>(TState state) => null;
//        public bool IsEnabled(LogLevel logLevel) => true;

//        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
//        {
//            var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{logLevel}] {formatter(state, exception)}";
//            if (exception != null)
//                logEntry += $"{Environment.NewLine}File: {exception.Source}";

//            File.WriteAllText(_filePath, logEntry + Environment.NewLine);
//        }
//    }
//}
