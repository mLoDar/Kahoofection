using System.Text;

using Kahoofection.Scripts;
using Kahoofection.Ressources;





namespace Kahoofection
{
    internal class EntryPoint
    {
        private const string _currentSection = "EntryPoint";

        private static readonly ApplicationSettings.Paths _appPaths = new();
        private static readonly ApplicationSettings.Runtime _appRuntime = new();



        static async Task Main()
        {
            string appVersion = _appRuntime.appVersion;
            string appRelease = _appRuntime.appRelease;

            ActivityLogger.Log(_currentSection, string.Empty, true);
            ActivityLogger.Log(_currentSection, "Starting Kahoofection (C) The only way to play");
            ActivityLogger.Log(_currentSection, $"Version '{appVersion}' | Release '{appRelease}'");



            ActivityLogger.Log(_currentSection, "Trying to enable support for ANSI escape sequence.");

            // TODO: Enable ANSI Support



            ActivityLogger.Log(_currentSection, "Searching for all required folders/files.");

            // TODO: Create needed folders/files



            Console.Title = "Kahoofection";
            Console.OutputEncoding = Encoding.UTF8;



            await MainMenu.Start();



            ActivityLogger.Log(_currentSection, "Shutting down Kahoofection.");

            Environment.Exit(0);
        }
    }
}