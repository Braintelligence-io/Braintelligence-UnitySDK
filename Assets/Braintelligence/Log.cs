namespace Braintelligence
{
    internal interface ILogger
    {
        void Info(object obj);
        void Warning(object obj);
        void Error(object obj);
    }

    internal sealed class UnityLogger : ILogger
    {
        public void Info(object obj)
        {
            UnityEngine.Debug.Log(obj);
        }

        public void Warning(object obj)
        {
            UnityEngine.Debug.LogWarning(obj);

        }

        public void Error(object obj)
        {
            UnityEngine.Debug.LogError(obj);
        }
    }

    public static class Log
    {
        public enum Level
        {
            Info, Warning, Error, Off
        }

        private static ILogger _logger;

        private static Level _level;

        public static void Initialize(Level level)
        {
            _level = level;
            _logger = new UnityLogger(); //TODO: Get the logger type as a parameter
        }

        public static void Info(object obj)
        {
            if(_level > Level.Info) return;
            _logger.Info(obj);
        }

        public static void Warning(object obj)
        {
            if(_level > Level.Warning) return;
            _logger.Warning(obj);
        }
        
        public static void Error(object obj)
        {
            if(_level > Level.Error) return;
            _logger.Warning(obj);
        }
    }
}