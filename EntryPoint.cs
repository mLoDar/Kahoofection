using System.Text;

using Kahoofection.Scripts;
using Kahoofection.Ressources;





namespace Kahoofection
{
    internal class EntryPoint
    {
        private const string _currentSection = "EntryPoint";

        private static readonly ApplicationSettings.Runtime _appRuntime = new();



        static async Task Main()
        {
            string appVersion = _appRuntime.appVersion;
            string appRelease = _appRuntime.appRelease;

            ActivityLogger.Log(_currentSection, string.Empty, true);
            ActivityLogger.Log(_currentSection, "Starting Kahoofection (C) The only way to play");
            ActivityLogger.Log(_currentSection, $"Version '{appVersion}' | Release '{appRelease}'");



            ActivityLogger.Log(_currentSection, "Trying to enable support for ANSI escape sequence.");
            (bool ansiSupportEnabled, Exception occuredError) = ConsoleHelper.EnableAnsiSupport();

            if (ansiSupportEnabled == false)
            {
                ActivityLogger.Log(_currentSection, "[ERROR] Failed to enable ANSI support.");
                ActivityLogger.Log(_currentSection, occuredError.Message, true);

                Console.SetCursorPosition(0, 4);
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("             WARNING\r\n");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("             Failed to enable support for ANSI escape sequences.");
                Console.WriteLine("             This will have side effects on the coloring within the console.\r\n");
                Console.WriteLine("             Please read the manual on how to fix this error!");

                Thread.Sleep(5000);
            }
            else
            {
                ActivityLogger.Log(_currentSection, "Successfully enabled ANSI support!");
            }



            Console.Title = "Kahoofection";
            Console.CursorVisible = false;
            Console.OutputEncoding = Encoding.UTF8;



            ActivityLogger.Log(_currentSection, "Displaying welcome message.");

            await DisplayWelcomeMessage();

            ActivityLogger.Log(_currentSection, "Welcome message was displayed, redirecting to the main menu.");

            await MainMenu.Start();

            ActivityLogger.Log(_currentSection, "Shutting down Kahoofection.");



            Environment.Exit(0);
        }

        private static async Task DisplayWelcomeMessage()
        {
            ConsoleHelper.ResetConsole();



            string welcomeMessage = "Hello stranger! Welcome to:";

            string[] kahoofectionHeader =
            [
                @"              _  __ __  _  _  __   __  ___ ___ ________ _  __  __  _ ",
                @"             | |/ //  \| || |/__\ /__\| __| __/ _/_   _| |/__\|  \| |",
                @"             |   <| /\ | >< | \/ | \/ | _|| _| \__ | | | | \/ | | ' |",
                @"             |_|\_\_||_|_||_|\__/ \__/|_| |___\__/ |_| |_|\__/|_|\__|",
            ];



            Console.Write("             ");
            Console.ForegroundColor = ConsoleColor.White;

            foreach (char c in welcomeMessage)
            {
                Console.Write(c);
                await Task.Delay(10);
            }

            await Task.Delay(500);

            Console.WriteLine();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Blue;
             
            foreach (string line in kahoofectionHeader)
            {
                await Task.Delay(100);
                Console.WriteLine(line);
            }

            await Task.Delay(1500);
        }
    }
}