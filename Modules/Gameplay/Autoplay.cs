using Kahoofection.Scripts;
using Kahoofection.Ressources;
using Kahoofection.Scripts.Driver;
using Kahoofection.Scripts.Kahoot;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;





namespace Kahoofection.Modules.Gameplay
{
    internal struct GameAutoplaySettings
    {
        internal int gamePin;
        internal string gameUsername;
        internal Guid quizId;
        internal LegitMode legitMode;
        internal int wrongAnswersPercentage;
    }

    enum LegitMode
    {
        Legit = 0,
        Closet = 1,
        Semi = 2,
        Rage = 3
    }



    internal class Autoplay
    {
        private const string _currentSection = "Autoplay";

        private static readonly ApplicationSettings.Urls _appUrls = new();
        private static readonly ApplicationSettings.Paths _appPaths = new();

        private static string _quizIdApiResponse = string.Empty;

        private static IWebDriver _webDriver;



        internal static async Task Start()
        {
            string subSection = "Main";

        LabelMethodEntryPoint:

            Console.Title = $"Kahoofection | {_currentSection}";
            ActivityLogger.Log(_currentSection, subSection, $"Starting module '{_currentSection}'");


            
            ConsoleHelper.ResetConsole();

            (bool escapeKeyPressed, GameAutoplaySettings gameAutoplaySettings) = await GetGameSettings();

            if (escapeKeyPressed == true)
            {
                ActivityLogger.Log(_currentSection, subSection, $"Leaving module, as the input was cancelled via the ESC key.");
                return;
            }

            ActivityLogger.Log(_currentSection, subSection, $"Successfully created Autoplay settings.");

            

            ConsoleHelper.ResetConsole();

            ActivityLogger.Log(_currentSection, subSection, $"Prompting to configure legit mode for the Autoplay.");

            (escapeKeyPressed, LegitMode legitMode) = GetLegitMode();

            if (escapeKeyPressed == true)
            {
                ActivityLogger.Log(_currentSection, subSection, $"Leaving module, as the input was cancelled via the ESC key.");
                return;
            }

            ActivityLogger.Log(_currentSection, subSection, $"Successfully fetched the Autoplay's legit mode.");

            gameAutoplaySettings.legitMode = legitMode;



            ConsoleHelper.ResetConsole();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("             Loading ...");
            Console.WriteLine("             Please be patient.");



            ActivityLogger.Log(_currentSection, subSection, $"Launching a WebDriver in order to join the game.");

            bool successfullyLaunched = await LaunchWebDriver();

            if (successfullyLaunched == false)
            {
                ActivityLogger.Log(_currentSection, subSection, $"Leaving module, as no WebDriver could be launched.");
                ActivityLogger.Log(_currentSection, subSection, $"Details about this problem should be at the error logs above.");

                ConsoleHelper.ResetConsole();

                string title = "Autoplay failed";
                string description = "Please look at the error logs to fix this problem.";

                await ConsoleHelper.DisplayInformation(title, description, ConsoleColor.Red);

                return;
            }



            ActivityLogger.Log(_currentSection, subSection, $"Successfully launched a WebDriver, trying to join the game.");

            // TODO: Create mechanics to join/play the gmae
        }

        private static async Task<(bool escapeKeyPressed, GameAutoplaySettings gameAutoplaySettings)> GetGameSettings()
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
                return (true, new GameAutoplaySettings());
            }

            ActivityLogger.Log(_currentSection, subSection, $"A GamePin was provided, checking for validity.");
            ActivityLogger.Log(_currentSection, subSection, $"Input: '{providedGamePin}'", true);

            endCursorTop = Console.GetCursorPosition().Top;



            (int gamePin, Exception? exceptionGamePin) = await KahootHelper.CheckGamePin(providedGamePin);
            
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



            Console.WriteLine("                                                                                    ");
            Console.WriteLine("             \u001b[94m┌ \u001b[97mChoose a username: (min. 1, max. 100 characters) ");
            Console.WriteLine("             \u001b[94m├──────────────────────────────────────────────────────      ");

            startCursorTop = Console.GetCursorPosition().Top;

        LabelInputGameUsername:

            Console.SetCursorPosition(0, startCursorTop);

            for (int i = 0; i <= endCursorTop - startCursorTop + 3; i++)
            {
                ConsoleHelper.ClearLine();
                Console.WriteLine();
            }

            Console.SetCursorPosition(0, startCursorTop);

            Console.Write("             \u001b[94m└─> \u001b[97m");



            ActivityLogger.Log(_currentSection, subSection, $"Prompted to provide a Username, waiting for an input.");

            (escapeKeyPressed, string gameUsername) = await ConsoleHelper.ReadLine();

            if (escapeKeyPressed == true)
            {
                ActivityLogger.Log(_currentSection, subSection, $"The input process was cancelled via the ESC key, returning.");
                return (true, new GameAutoplaySettings());
            }

            ActivityLogger.Log(_currentSection, subSection, $"A Username was provided, checking for validity.");
            ActivityLogger.Log(_currentSection, subSection, $"Input: '{gameUsername}'", true);

            endCursorTop = Console.GetCursorPosition().Top;



            Exception? exceptionUsername = ValidUsername(gameUsername);

            if (exceptionUsername != null)
            {
                ActivityLogger.Log(_currentSection, subSection, $"An invalid Username was provided, restarting the input prompt.");
                ActivityLogger.Log(_currentSection, subSection, $"Exception: {exceptionUsername.Message}", true);

                Console.WriteLine();
                Console.WriteLine("             \u001b[91mInvalid Username");
                Console.WriteLine("             \u001b[97m" + exceptionUsername.Message);

                await Task.Delay(3000);

                goto LabelInputGameUsername;
            }

            ActivityLogger.Log(_currentSection, subSection, $"Successfully fetched a Username!");



            Console.WriteLine("                                                            ");
            Console.WriteLine("             \u001b[94m┌ \u001b[97mEnter the game's QuizId: ");
            Console.WriteLine("             \u001b[94m├──────────────────────────────      ");

            startCursorTop = Console.GetCursorPosition().Top;

        LabelInputQuizId:

            Console.SetCursorPosition(0, startCursorTop);

            for (int i = 0; i <= endCursorTop - startCursorTop + 3; i++)
            {
                ConsoleHelper.ClearLine();
                Console.WriteLine();
            }

            Console.SetCursorPosition(0, startCursorTop);

            Console.Write("             \u001b[94m└─> \u001b[97m");



            ActivityLogger.Log(_currentSection, subSection, $"Prompted to provide a QuizId, waiting for an input.");

            (escapeKeyPressed, string quizId) = await ConsoleHelper.ReadLine();

            if (escapeKeyPressed == true)
            {
                ActivityLogger.Log(_currentSection, subSection, $"The input process was cancelled via the ESC key, returning.");
                return (true, new GameAutoplaySettings());
            }


            (Guid convertedQuizId, string apiResponse, Exception? quizIdError) = await KahootHelper.ValidQuizId(quizId);

            if (quizIdError != null)
            {
                ActivityLogger.Log(_currentSection, subSection, "Re-entering the module as an invalid QuizId was provided.");
                ActivityLogger.Log(_currentSection, subSection, $"Exception: {quizIdError.Message}");

                goto LabelInputQuizId;
            }
            
            ActivityLogger.Log(_currentSection, subSection, $"Successfully fetched a QuizId!");



            GameAutoplaySettings gameAutoplaySettings = new()
            {
                gamePin = gamePin,
                gameUsername = gameUsername,
                quizId = convertedQuizId,
                wrongAnswersPercentage = 75,
            };

            _quizIdApiResponse = apiResponse;



            return (false, gameAutoplaySettings);
        }

        private static Exception? ValidUsername(string providedInput)
        {
            string subSection = "ValidUsername";

            providedInput = RegexPatterns.AllWhitespaces().Replace(providedInput, string.Empty);

            if (string.IsNullOrWhiteSpace(providedInput))
            {
                ActivityLogger.Log(_currentSection, subSection, "Restarting the prompt for a Username as the provided input is null or whitespace.");
                return (new Exception("Username is null or whitespace."));
            }

            if (Enumerable.Range(1, 100).Contains(providedInput.Length) == false)
            {
                ActivityLogger.Log(_currentSection, subSection, "Restarting the prompt for a Username as the provided input's length is not in the specified range.");
                return (new Exception("Username's length is not in the specified range."));
            }

            return (null);
        }

        private static (bool escapeKeyPressed, LegitMode legitMode) GetLegitMode()
        {
            Console.CursorVisible = false;

            int currentNavigationIndex = 0;
            int menuOptionsCount = 4;

            (int cursorLeft, int cursorTop) = Console.GetCursorPosition();



        LabelDrawOptions:

            Console.SetCursorPosition(cursorLeft, cursorTop);

            string stateLegit = (currentNavigationIndex == 0 ? "\u001b[94m├─>\u001b[92m LEGIT" : "\u001b[94m│  \u001b[92m Legit");
            string stateCloset = (currentNavigationIndex == 1 ? "\u001b[94m├─>\u001b[93m CLOSET" : "\u001b[94m│  \u001b[93m Closet");;
            string stateSemi = (currentNavigationIndex == 2 ? "\u001b[94m├─>\u001b[91m SEMI" : "\u001b[94m│  \u001b[91m Semi");;
            string stateRage = (currentNavigationIndex == 3 ? "\u001b[94m├─>\u001b[31m RAGE" : "\u001b[94m│  \u001b[31m Rage"); ;

            Console.WriteLine("             \u001b[94m┌ \u001b[97mLegit mode                                         ");
            Console.WriteLine("             \u001b[94m│ \u001b[97mHow undetected should the Autoplay be?             ");
            Console.WriteLine("             \u001b[94m├──────────────┬────────────────────────────────────┐");
            Console.WriteLine("             │              │                                    │");
            Console.WriteLine("             {0}      \u001b[94m│ \u001b[97mAnswer delay and incorrect answers \u001b[94m│", stateLegit);
            Console.WriteLine("             {0}     \u001b[94m│ \u001b[97mAnswer delay only                  \u001b[94m│", stateCloset);
            Console.WriteLine("             {0}       \u001b[94m│ \u001b[97mIncorrect answers only             \u001b[94m│", stateSemi);
            Console.WriteLine("             {0}       \u001b[94m│ \u001b[97mNone of the above                  \u001b[94m│", stateRage);
            Console.WriteLine("             │              │                                    │");
            Console.WriteLine("             \u001b[94m└──────────────┴────────────────────────────────────┘");


            ConsoleKey pressedKey = Console.ReadKey(true).Key;

            switch (pressedKey)
            {
                case ConsoleKey.DownArrow:
                    if (currentNavigationIndex + 1 < menuOptionsCount)
                    {
                        currentNavigationIndex += 1;
                    }
                    break;

                case ConsoleKey.UpArrow:
                    if (currentNavigationIndex - 1 >= 0)
                    {
                        currentNavigationIndex -= 1;
                    }
                    break;

                case ConsoleKey.Escape:
                    Console.CursorVisible = true;
                    return (true, LegitMode.Legit);

                default:
                    break;
            }



            if (pressedKey != ConsoleKey.Enter)
            {
                goto LabelDrawOptions;
            }

            Console.CursorVisible = true;

            return (false, (LegitMode)currentNavigationIndex);
        }

        private static async Task<bool> LaunchWebDriver()
        {
            string subSection = "LaunchWebDriver";

            bool firefoxFailed = false;
            bool chromeFailed = false;

            SupportedBrowser browserToInitialize = SupportedBrowser.Firefox;

        LabelMethodBeginning:

            ActivityLogger.Log(_currentSection, subSection, $"Trying to initialize a driver for '{browserToInitialize}'.");

            try
            {
                if (browserToInitialize == SupportedBrowser.Firefox)
                {
                    LaunchFirefox();
                }
                else if (browserToInitialize == SupportedBrowser.Chrome)
                {
                    LaunchChrome();
                }
            }
            catch (Exception exception)
            {
                ActivityLogger.Log(_currentSection, subSection, $"Failed to initialize a driver for '{browserToInitialize}'.");
                ActivityLogger.Log(_currentSection, subSection, $"Exception: {exception.Message}", true);



                if (chromeFailed == true)
                {
                    ActivityLogger.Log(_currentSection, subSection, $"Returning to origin method as no possible driver started.");

                    return false;
                }

                if (firefoxFailed == true)
                {
                    ActivityLogger.Log(_currentSection, subSection, $"Switching to '{SupportedBrowser.Chrome}', as {browserToInitialize} did not work.");

                    browserToInitialize = SupportedBrowser.Chrome;
                    goto LabelMethodBeginning;
                }

                if (browserToInitialize == SupportedBrowser.Firefox)
                {
                    ActivityLogger.Log(_currentSection, subSection, $"Downloading the latest driver version for '{browserToInitialize}' in order to fix the issue.");

                    bool succesfullyDownloaded = await DriverInstaller.DownloadGeckoDriver();

                    if (succesfullyDownloaded == false)
                    {
                        ActivityLogger.Log(_currentSection, subSection, $"Failed to download the driver. See the errors above for more information.");
                    }

                    ConsoleHelper.ResetConsole();

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("             Loading ...");
                    Console.WriteLine("             Please be patient.");

                    firefoxFailed = true;
                    goto LabelMethodBeginning;
                }

                if (browserToInitialize == SupportedBrowser.Chrome)
                {
                    ActivityLogger.Log(_currentSection, subSection, $"Downloading the latest driver version for '{browserToInitialize}' in order to fix the issue.");

                    bool succesfullyDownloaded = await DriverInstaller.DownloadChromeDriver();

                    if (succesfullyDownloaded == false)
                    {
                        ActivityLogger.Log(_currentSection, subSection, $"Failed to download the driver. See the errors above for more information.");
                    }

                    ConsoleHelper.ResetConsole();

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("             Loading ...");
                    Console.WriteLine("             Please be patient.");

                    chromeFailed = true;
                    goto LabelMethodBeginning;
                }
            }

            ActivityLogger.Log(_currentSection, subSection, $"Successfully launched a driver for '{browserToInitialize}'!");

            return true;
        }

        private static void LaunchFirefox()
        {
            string driverPath = _appPaths.driversFolder;

            FirefoxDriverService service = FirefoxDriverService.CreateDefaultService(driverPath);
            service.SuppressInitialDiagnosticInformation = true;
            service.HideCommandPromptWindow = true;
            service.LogLevel = FirefoxDriverLogLevel.Fatal;

            FirefoxOptions options = new();

            options.AddArguments(
                "--disable-extensions",
                "--disable-gpu",
                "--no-sandbox",
                "--disable-dev-shm-usage",
                "--disable-background-networking",
                "--disable-sync",
                "--disable-default-apps",
                "--headless"
            );

            options.LogLevel = FirefoxDriverLogLevel.Fatal;

            _webDriver = new FirefoxDriver(service, options);
        }

        private static void LaunchChrome()
        {
            string driverPath = _appPaths.driversFolder;

            ChromeDriverService service = ChromeDriverService.CreateDefaultService(driverPath);
            service.SuppressInitialDiagnosticInformation = true;
            service.HideCommandPromptWindow = true;

            ChromeOptions options = new();

            options.AddArguments(
                "no-sandbox",
                "log-level=3",
                "headless"
            );

            _webDriver = new ChromeDriver(service, options);
        }
    }
}