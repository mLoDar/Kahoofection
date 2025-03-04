using Kahoofection.Scripts;





namespace Kahoofection.Modules.Gameplay
{
    internal class Autoplay
    {
        private const string _currentSection = "Autoplay";



        internal static async Task Start()
        {
            string subSection = "Main";

        LabelMethodEntryPoint:

            Console.Title = $"Kahoofection | {_currentSection}";
            ActivityLogger.Log(_currentSection, subSection, $"Starting module '{_currentSection}'");



            ConsoleHelper.ResetConsole();
        }
    }
}