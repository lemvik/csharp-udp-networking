namespace Lem.Networking.Utilities.Logging
{
    public interface ILogger
    {
        void LogDebug(string format, params object[] args);

        void LogInformation(string format, params object[] args);

        void LogWarning(string format, params object[] args);

        void LogError(string format, params object[] args);
    }
}