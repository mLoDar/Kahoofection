using Kahoofection.Scripts;
using Kahoofection.Scripts.Kahoot;





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

        private static List<(KahootClient, Task)> _activeBots = [];
        private static List<Task> _spamQueue = [];



        internal static async Task Start()
        {
            string subSection = "Main";

        LabelMethodEntryPoint:

            Console.Title = $"Kahoofection | {_currentSection}";
            ActivityLogger.Log(_currentSection, subSection, $"Starting module '{_currentSection}'");



            ConsoleHelper.ResetConsole();



            ActivityLogger.Log(_currentSection, subSection, $"Prompting for game settings in order to initiate the spammer.");

            (bool escapeKeyPressed, GameSpammerSettings gameSpammerSettings) = await GetGameSettings();

            if (escapeKeyPressed == true)
            {
                ActivityLogger.Log(_currentSection, subSection, $"Leaving module, as the input was cancelled via the ESC key.");
                return;
            }

            ActivityLogger.Log(_currentSection, subSection, $"Successfully created settings for the spammer.");



            Console.CursorVisible = false;
            ConsoleHelper.ResetConsole();

            Console.WriteLine("             \u001b[94m┌ \u001b[92mKahooo-goo!                            ");
            Console.WriteLine("             \u001b[94m│ \u001b[97mThe bots should join any moment ;)     ");
            Console.WriteLine("             \u001b[94m└────────────────────────────────────────          ");
            Console.WriteLine("                                                                          ");
            Console.WriteLine("             \u001b[94m┌ \u001b[97mUse BACKSPACE to restart               ");
            Console.WriteLine("             \u001b[94m└──────────────────────────────                    ");
            Console.WriteLine("                                                                          ");
            Console.WriteLine("             \u001b[94m┌ \u001b[97mUse ESC to leave                       ");
            Console.WriteLine("             \u001b[94m└──────────────────────                            ");



            _spamQueue.Add(InitiateSpammer(gameSpammerSettings));



        LabelReadKey:

            ConsoleKey pressedKey = Console.ReadKey(true).Key;

            switch (pressedKey)
            {
                case ConsoleKey.Escape:
                    ActivityLogger.Log(_currentSection, subSection, $"Leaving module, as the ESC key was pressed after initiating the spammer.");

                    TerminateActiveBots();

                    Console.CursorVisible = true;
                    return;

                case ConsoleKey.Backspace:
                    ActivityLogger.Log(_currentSection, subSection, $"Restarting module, as the BACKSPACE key was pressed after initiating the spammer.");

                    TerminateActiveBots();

                    Console.CursorVisible = true;
                    goto LabelMethodEntryPoint;

                default:
                    goto LabelReadKey;
            }
        }

        private static async Task<(bool escapeKeyPressed, GameSpammerSettings gameSpammerSettings)> GetGameSettings()
        {
            string subSection = "GetGameSettings";

            Console.WriteLine("             \u001b[94m┌ \u001b[97mEnter a GamePin: ");
            Console.WriteLine("             \u001b[94m├──────────────────────      ");

            int startCursorTop = Console.GetCursorPosition().Top;
            int endCursorTop = Console.GetCursorPosition().Top;

        LabelInputGamePin:

            Console.SetCursorPosition(0, startCursorTop);

            for (int i = 0; i <= endCursorTop - startCursorTop + 3; i++)
            {
                ConsoleHelper.ClearLine();
                Console.WriteLine();
            }

            Console.SetCursorPosition(0, startCursorTop);

            Console.Write("             \u001b[94m└─> \u001b[97m");



            ActivityLogger.Log(_currentSection, subSection, $"Prompted to provide a GamePin, waiting for an input.");

            (bool escapeKeyPressed, string providedGamePin) = await ConsoleHelper.ReadLine();
            
            if (escapeKeyPressed == true)
            {
                ActivityLogger.Log(_currentSection, subSection, $"The input process was cancelled via the ESC key, returning.");
                return (true, new GameSpammerSettings());
            }

            ActivityLogger.Log(_currentSection, subSection, $"A GamePin was provided, checking for validity.");
            ActivityLogger.Log(_currentSection, subSection, $"Input: '{providedGamePin}'", true);

            endCursorTop = Console.GetCursorPosition().Top;



            (int gamePin, Exception? exceptionGamePin) = await KahootValidator.ValidGamePin(providedGamePin);

            if (exceptionGamePin != null)
            {
                ActivityLogger.Log(_currentSection, subSection, $"An invalid GamePin was provided, restarting the input prompt.");
                ActivityLogger.Log(_currentSection, subSection, $"Exception: {exceptionGamePin.Message}", true);

                Console.WriteLine();
                Console.WriteLine("             \u001b[91mInvalid GamePin");
                Console.WriteLine("             \u001b[97m" + exceptionGamePin.Message);

                await Task.Delay(3000);

                goto LabelInputGamePin;
            }

            ActivityLogger.Log(_currentSection, subSection, $"Successfully fetched a GamePin!");



            Console.WriteLine("                                                                                 ");
            Console.WriteLine("             \u001b[94m┌ \u001b[97mHow many bots should join? (min. 3, max. 100) ");
            Console.WriteLine("             \u001b[94m├───────────────────────────────────────────────────      ");

            startCursorTop = Console.GetCursorPosition().Top;

        LabelInputGameBotCount:

            Console.SetCursorPosition(0, startCursorTop);

            for (int i = 0; i <= endCursorTop - startCursorTop + 3; i++)
            {
                ConsoleHelper.ClearLine();
                Console.WriteLine();
            }

            Console.SetCursorPosition(0, startCursorTop);

            Console.Write("             \u001b[94m└─> \u001b[97m");



            ActivityLogger.Log(_currentSection, subSection, $"Prompted to provide a BotCount, waiting for an input.");

            (escapeKeyPressed, string providedBotCount) = await ConsoleHelper.ReadLine();
            
            if (escapeKeyPressed == true)
            {
                ActivityLogger.Log(_currentSection, subSection, $"The input process was cancelled via the ESC key, returning.");
                return (true, new GameSpammerSettings());
            }

            ActivityLogger.Log(_currentSection, subSection, $"A BotCount was provided, checking for validity.");
            ActivityLogger.Log(_currentSection, subSection, $"Input: '{providedBotCount}'", true);

            endCursorTop = Console.GetCursorPosition().Top;



            (int gameBotCount, Exception? exceptionBotCount) = KahootValidator.ValidSpammerBotCount(providedBotCount);

            if (exceptionBotCount != null)
            {
                ActivityLogger.Log(_currentSection, subSection, $"An invalid BotCount was provided, restarting the input prompt.");
                ActivityLogger.Log(_currentSection, subSection, $"Exception: {exceptionBotCount.Message}", true);

                Console.WriteLine();
                Console.WriteLine("             \u001b[91mInvalid BotCount");
                Console.WriteLine("             \u001b[97m" + exceptionBotCount.Message);

                await Task.Delay(3000);

                goto LabelInputGameBotCount;
            }

            ActivityLogger.Log(_currentSection, subSection, $"Successfully fetched a BotCount!");



            Console.WriteLine("                                                                                             ");
            Console.WriteLine("             \u001b[94m┌ \u001b[97mChoose a name for the bots: (min. 1, max. 100 characters) ");
            Console.WriteLine("             \u001b[94m├───────────────────────────────────────────────────────────────      ");

            startCursorTop = Console.GetCursorPosition().Top;

        LabelInputGameBotName:

            Console.SetCursorPosition(0, startCursorTop);

            for (int i = 0; i <= endCursorTop - startCursorTop + 3; i++)
            {
                ConsoleHelper.ClearLine();
                Console.WriteLine();
            }

            Console.SetCursorPosition(0, startCursorTop);

            Console.Write("             \u001b[94m└─> \u001b[97m");



            ActivityLogger.Log(_currentSection, subSection, $"Prompted to provide a BotName, waiting for an input.");

            (escapeKeyPressed, string gameBotName) = await ConsoleHelper.ReadLine();

            if (escapeKeyPressed == true)
            {
                ActivityLogger.Log(_currentSection, subSection, $"The input process was cancelled via the ESC key, returning.");
                return (true, new GameSpammerSettings());
            }

            ActivityLogger.Log(_currentSection, subSection, $"A BotName was provided, checking for validity.");
            ActivityLogger.Log(_currentSection, subSection, $"Input: '{gameBotName}'", true);

            endCursorTop = Console.GetCursorPosition().Top;



            Exception? exceptionBotName = KahootValidator.ValidSpammerBotName(gameBotName);

            if (exceptionBotName != null)
            {
                ActivityLogger.Log(_currentSection, subSection, $"An invalid BotName was provided, restarting the input prompt.");
                ActivityLogger.Log(_currentSection, subSection, $"Exception: {exceptionBotName.Message}", true);

                Console.WriteLine();
                Console.WriteLine("             \u001b[91mInvalid BotName");
                Console.WriteLine("             \u001b[97m" + exceptionBotName.Message);

                await Task.Delay(3000);

                goto LabelInputGameBotName;
            }

            ActivityLogger.Log(_currentSection, subSection, $"Successfully fetched a BotName!");



            GameSpammerSettings gameSpammerSettings = new()
            {
                gamePin = gamePin,
                gameBotName = gameBotName,
                gameBotCount = gameBotCount
            };

            ActivityLogger.Log(_currentSection, subSection, $"Returning with the created settings:");
            ActivityLogger.Log(_currentSection, subSection, $"GamePin: {gameSpammerSettings.gamePin}", true);
            ActivityLogger.Log(_currentSection, subSection, $"GameBotName: {gameSpammerSettings.gameBotName}", true);
            ActivityLogger.Log(_currentSection, subSection, $"GameBotCount: {gameSpammerSettings.gameBotCount}", true);
            
            return (false, gameSpammerSettings);
        }
        
        private static async Task InitiateSpammer(GameSpammerSettings gameSpammerSettings)
        {
            for (int i = 0; i < gameSpammerSettings.gameBotCount; i++)
            {
                string currentBotName = $"{gameSpammerSettings.gameBotName}{new string('\u200B', i)}";

                KahootClient kahootClient = new();

                _activeBots.Add((kahootClient, kahootClient.JoinGame(gameSpammerSettings.gamePin, currentBotName)));

                await Task.Delay(100);
            }
        }

        private static void TerminateActiveBots()
        {
            foreach ((KahootClient kahootClient, _) in _activeBots)
            {
                kahootClient.Terminate();
            }

            _activeBots = [];
            _spamQueue = [];
        }
    }
}