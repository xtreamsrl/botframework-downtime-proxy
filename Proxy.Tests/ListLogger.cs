using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Proxy.Tests
{
    public class ListLogger : ILogger
    {
        public IList<string> Logs { get; }

        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => false;

        public ListLogger()
        {
            Logs = new List<string>();
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            string message = formatter(state, exception);
            Logs.Add(message);
        }
        
        internal class NullScope : IDisposable
        {
            public static NullScope Instance { get; } = new NullScope();

            private NullScope()
            {
            }
            
            public void Dispose()
            {
            }
        }
    }
}