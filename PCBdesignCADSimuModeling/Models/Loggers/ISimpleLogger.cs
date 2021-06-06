using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace PCBdesignCADSimuModeling.Models.Loggers
{
    public interface ISimpleLogger
    {
        void Log(object messageStr);
        
        public TimeSpan ModelTime { get; set; } // ToDo: delete
    }
    

    public class CompositionSimpleLogger : ISimpleLogger
    {
        private readonly List<ISimpleLogger> _loggers;

        public CompositionSimpleLogger(List<ISimpleLogger> loggers)
        {
            _loggers = loggers;
        }

        public void Log(object messageStr) => _loggers.ForEach(logger => logger.Log(messageStr));
        public TimeSpan ModelTime { get; set; }
    }

    
    public class ConsoleSimpleLogger : ISimpleLogger
    {
        public void Log(object messageStr) => Console.WriteLine(messageStr.ToString());
        public TimeSpan ModelTime { get; set; }
    }
    
    
    public class DebugSimpleLogger : ISimpleLogger
    {
        public void Log(object messageStr) => Debug.WriteLine(messageStr.ToString());
        public TimeSpan ModelTime { get; set; }
    }


    public class InMemorySimpleLogger : ISimpleLogger
    {
        private StringBuilder StringBuilder { get; } = new();

        public void Log(object message)
        {
            var messageStr = message.ToString();
            if (messageStr is null)
                return;

            const string separator1 = "Технология: ";
            var splited1Num = messageStr.Split(separator1);
            if (splited1Num.Length < 2)
            {
                StringBuilder.AppendLine(messageStr); return;
            }

            const string separator2 = " -";
            var splited2Num = splited1Num[1].Split(separator2);
            if (splited2Num.Length < 2)
            {
                StringBuilder.AppendLine(messageStr); return;
            }

            if (!Int32.TryParse(splited2Num[0], out var num))
            {
                StringBuilder.AppendLine(messageStr); return;
            }
            
            var spaces = Enumerable.Range(1, (num - 1) % 15).Aggregate(String.Empty, (current, _) => current + "---");
            
            var split = messageStr.Split("|");
            if (split.Length < 2)
            {
                StringBuilder.AppendLine(messageStr); return;
            }
            
            StringBuilder.AppendLine($"{split[0]}|{spaces}{split[1]}");
        }

        public TimeSpan ModelTime { get; set; }

        public string GetData() => StringBuilder.ToString();
    }
    
    
    public class FileSimpleLogger : InMemorySimpleLogger
    {
        public void LogToFile(string path)
        {
            var data = GetData();

            using var s = new FileStream(path, FileMode.Append);
            using var sw = new StreamWriter(s);
            sw.WriteLine(data);
        }
        
        public void LogToFileTruncate(string path)
        {
            var data = GetData();
            
            if (!File.Exists(path))
            {
                using var s = new FileStream(path, FileMode.OpenOrCreate);
                using var sw = new StreamWriter(s);
                sw.WriteLine(data);
            }
            else
            {
                using var s = new FileStream(path, FileMode.Truncate);
                using var sw = new StreamWriter(s);
                sw.WriteLine(data);
            }
        }
    }
}