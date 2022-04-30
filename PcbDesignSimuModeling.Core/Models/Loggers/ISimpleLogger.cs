using System.Diagnostics;
using System.Text;

namespace PcbDesignSimuModeling.Core.Models.Loggers;

public interface ISimpleLogger
{
    void Log(object data);
}

public class ConsoleSimpleLogger : ISimpleLogger
{
    public void Log(object data) => Console.WriteLine(data.ToString());
}

public class DebugSimpleLogger : ISimpleLogger
{
    public void Log(object data) => Debug.WriteLine(data.ToString());
}

public class InMemorySimpleLogger : ISimpleLogger
{
    private readonly StringBuilder _stringBuilder = new();

    public void Log(object data) => _stringBuilder.AppendLine(data.ToString());

    public string GetData() => _stringBuilder.ToString();
}

    
public abstract class FileSimpleLogger : ISimpleLogger
{   
    protected readonly string FilePath;

    protected FileSimpleLogger(string filePath) => FilePath = filePath;

    public abstract void Log(object data);
}

public class AppendFileSimpleLogger : FileSimpleLogger
{
    public AppendFileSimpleLogger(string filePath) : base(filePath)
    {
    }

    public override void Log(object data)
    {
        using var fileStream = new FileStream(FilePath, FileMode.Append);
        using var streamWriter = new StreamWriter(fileStream);
        streamWriter.WriteLine(data.ToString());
    }
}

public class TruncateFileSimpleLogger : FileSimpleLogger
{
    public TruncateFileSimpleLogger(string filePath) : base(filePath)
    {
    }

    public override void Log(object data)
    {
        if (!File.Exists(FilePath))
        {
            using var fileStream = new FileStream(FilePath, FileMode.OpenOrCreate);
            using var streamWriter = new StreamWriter(fileStream);
            streamWriter.WriteLine(data.ToString());
        }
        else
        {
            using var fileStream = new FileStream(FilePath, FileMode.Truncate);
            using var streamWriter = new StreamWriter(fileStream);
            streamWriter.WriteLine(data.ToString());
        }
    }
}
    
    
public class CompositionSimpleLogger : ISimpleLogger
{
    private readonly List<ISimpleLogger> _loggers;

    public CompositionSimpleLogger(List<ISimpleLogger> loggers)
    {
        _loggers = loggers;
    }

    public void Log(object data) => _loggers.ForEach(logger => logger.Log(data));
}