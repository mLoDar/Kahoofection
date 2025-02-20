using Kahoofection.Scripts;





namespace Kahoofection.Modules.Gameplay
{
    internal struct GameSpammerSettings
    {
        internal int gamePin;
        internal int gameBotCount;
        internal string gameBotName;
    }



    internal class GamePinSpammer
    {
        private const string _currentSection = "GamePinSpammer";



        internal static async Task Start()
        {
        LabelMethodEntryPoint:

            Console.Title = $"Kahoofection | {_currentSection}";
            ActivityLogger.Log(_currentSection, $"Starting module '{_currentSection}'");



            ConsoleHelper.ResetConsole();



            // TODO: Start the modules main tasks and so on
        }
    }
}