using System;
using System.Collections.Generic;
using System.Text;

namespace PCBdesignCADSimuModeling.Models.Loggers
{
    public interface ISimpleLogger
    {
        void Log(object message);
    }


    public class ConsoleSimpleLogger : ISimpleLogger
    {
        public void Log(object message) => Console.WriteLine(message.ToString());
    }


    public class InMemorySimpleLogger : ISimpleLogger
    {
        private StringBuilder StringBuilder { get; } = new();

        public void Log(object message) => StringBuilder.AppendLine(message.ToString());

        public string GetData() => StringBuilder.ToString();
    }


    public class CompositionSimpleLogger : ISimpleLogger
    {
        private readonly List<ISimpleLogger> _loggers;

        public CompositionSimpleLogger(List<ISimpleLogger> loggers)
        {
            _loggers = loggers;
        }

        public void Log(object message) => _loggers.ForEach(logger => logger.Log(message));
    }
}