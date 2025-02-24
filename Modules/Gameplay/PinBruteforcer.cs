using Kahoofection.Scripts;





namespace Kahoofection.Modules.Gameplay
{
    internal class PinBruteforcer
    {
        private const string _currentSection = "PinBruteforcer";

        

        internal static async Task Start()
        {
            Console.Title = $"Kahoofection | {_currentSection}";
            ActivityLogger.Log(_currentSection, $"Starting module '{_currentSection}'");



            ConsoleHelper.ResetConsole();
        }
    }
}