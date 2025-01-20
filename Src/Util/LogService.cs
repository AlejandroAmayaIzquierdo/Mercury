using NLog;

namespace Mercury.Util;

public class LogService
{
    private static Logger? _instance;

    public static Logger? Instance
    {
        set
        {
            _instance =
                _instance == null
                    ? value
                    : throw new InvalidOperationException("Logger instance has already been set.");
        }
    }

    public static Logger? Get()
    {
        return _instance;
    }
}
