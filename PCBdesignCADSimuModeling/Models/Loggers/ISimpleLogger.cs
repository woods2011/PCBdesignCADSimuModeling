using System;
using System.Collections.Generic;

namespace PCBdesignCADSimuModeling.Models.Loggers
{
    public interface ISimpleLogger
    {
        void Log(object message);
    }

    public class ConsoleSimpleLogger : ISimpleLogger
    {
        public void Log(object message)
        {
            Console.WriteLine(message.ToString());
        }
    }

    public class CompositionSimpleLogger : ISimpleLogger
    {
        private readonly List<ISimpleLogger> _loggers;

        public CompositionSimpleLogger(List<ISimpleLogger> loggers)
        {
            _loggers = loggers;
        }

        public void Log(object message) => 
            _loggers.ForEach(logger => logger.Log(message.ToString()));
    }
}