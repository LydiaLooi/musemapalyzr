using System.Text;

namespace MuseMapalyzr
{

    public enum LogLevel : int
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3
    }
    public class CustomLogger
    {

        private static LogLevel DebugLevel = LogLevel.Debug;

        private static readonly object locker = new object();
        private static CustomLogger instance = null;

        private static string Outfile = "logs/logs.log";
        private static StreamWriter LogWriter;


        // Private constructor to prevent outside instantiation
        private CustomLogger()
        {
            LogWriter = new StreamWriter(Outfile, false, Encoding.UTF8);
        }

        public static CustomLogger Instance
        {
            get
            {
                lock (locker)
                {
                    if (instance == null)
                    {
                        instance = new CustomLogger();
                    }
                    return instance;
                }
            }
        }

        private void WriteToFile(string message)
        {
            LogWriter.WriteLine(message);
            LogWriter.Flush();
        }

        public void Debug(string message)
        {
            if (DebugLevel <= LogLevel.Debug) WriteToFile(message);
        }

        public void Info(string message)
        {
            if (DebugLevel <= LogLevel.Info) WriteToFile(message);
        }

        public void Warning(string message)
        {
            if (DebugLevel <= LogLevel.Warning) WriteToFile(message);
        }

        public void Error(string message)
        {
            if (DebugLevel <= LogLevel.Error) WriteToFile(message);
        }

        // Don't forget to provide a method to properly close the StreamWriter
        public void Close()
        {
            LogWriter.Close();
        }

    }
}