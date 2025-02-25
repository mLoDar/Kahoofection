using Kahoofection.Ressources;





namespace Kahoofection.Scripts
{
    internal class ActivityLogger
    {
        private static readonly ApplicationSettings.Paths _appPaths = new();
        private static readonly ApplicationSettings.Runtime _runtime = new();



        internal static void Log(string currentSection, string subSection, string message, bool removePrefix = false)
        {
            try
            {
                DateTime now = DateTime.Now;

                string clientFolder = _appPaths.clientFolder;
                string logFileName = $"runtime-{_runtime.appVersion}-{now:dd-MM-yyyy}.log";
                string logsFolder = _appPaths.logsFolder;



                if (Directory.Exists(clientFolder) == false)
                {
                    Directory.CreateDirectory(clientFolder);
                }

                if (Directory.Exists(logsFolder) == false)
                {
                    Directory.CreateDirectory(logsFolder);
                }

                string logFile = Path.Combine(logsFolder, logFileName);
                string prefix = $"[{DateTime.Now}] - [ProcessId: {Environment.ProcessId}] - [Section: {currentSection} | {subSection}] - ";

                using FileStream fs = new(logFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                using StreamWriter writer = new(fs);



                if (removePrefix == true)
                {
                    writer.WriteLine($"{new string(' ', prefix.Length)}{message}");
                    return;
                }

                writer.WriteLine($"{prefix}{message}");
            }
            catch (Exception exception)
            {
                Console.Clear();
                Console.SetCursorPosition(0, 4);

                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("             WARNING\r\n");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("             Failed to create log entries for the current session.");
                Console.WriteLine("             Please copy the details below as a help to fix this error.");
                Console.WriteLine("             ");
                Console.WriteLine("             Details: ");
                Console.WriteLine($"             {exception.Message}");

                Thread.Sleep(10000);

                Environment.Exit(0);
            }
        }
    }
}