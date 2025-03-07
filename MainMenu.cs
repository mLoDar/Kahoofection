using Kahoofection.Scripts;
using Kahoofection.Modules.Gameplay;
using Kahoofection.Modules.Information;





namespace Kahoofection
{
    internal class MainMenu
    {
        private const string _currentSection = "MainMenu";

        private static int _navigationXPosition = 1;
        private static int _navigationYPosition = 1;

        private static readonly Random _numberGenerator = new();

        private static string _sessionPhrase = string.Empty;



        private static readonly Dictionary<int, string> _kahoofectionOptions = new()
        {
            { 1, "QuizIdByName" },
            { 2, "QuizIdChecker" },
            { 3, "QuizIdAnswers" },
            { 4, "PinBruteforce" },
            { 5, "GamePinSpammer" },
            { 6, "AutoplayWithGamePin" }
        };

        private static readonly int[,] _menuOptionsGrid = new int[,]
        {
            { 0, 0, 0, 0 },
            { 0, 1, 4, 0 },
            { 0, 2, 5, 0 },
            { 0, 3, 6, 0 },
            { 0, 0, 0, 0 },
        };

        private static readonly string[] wisdomPhrases =
        [
            "ᶠᵃᵏᵉ ʸᵒᵘʳ ᶜᵒⁿᶠⁱᵈᵉⁿᶜᵉ ˡⁱᵏᵉ ᵉᵛᵉʳʸᵒⁿᵉ ᵉˡˢᵉ",
            "ˡⁱᶠᵉ ⁱˢ ˡⁱᵏᵉ ᵃ ᵍᵃᵐᵉ, ʷᵉ ᵖᵃʸ ᵗᵒ ᵏᵉᵉᵖ ᵘᵖ",
            "ʷᵃᵗᵉʳ ʰᵃˢ ⁿᵒ ᵉᶠᶠᵉᶜᵗ ᵒⁿ ᶠᵃᵏᵉ ᶠˡᵒʷᵉʳˢ",
            "ʸᵒᵘ ᵐⁱˢˢ ᵉᵛᵉʳʸ ˢʰᵒᵗ ʸᵒᵘ ᵈᵒⁿ'ᵗ ᵗᵃᵏᵉ",
        ];



        internal static async Task Start()
        {
            string subSection = "Main";

            ActivityLogger.Log(_currentSection, subSection, "Entering main menu.");

            ConsoleHelper.ResetConsole();



            ActivityLogger.Log(_currentSection, subSection, "Choosing a new phrase for the main menu.");

            try
            {
                int maxPhraseLength = 47;

                while (true)
                {
                    int newNumber = _numberGenerator.Next(0, wisdomPhrases.Length);
                    _sessionPhrase = wisdomPhrases[newNumber];

                    if (_sessionPhrase.Length >= 1 || _sessionPhrase.Length <= maxPhraseLength)
                    {
                        break;
                    }
                }

                ActivityLogger.Log(_currentSection, subSection, "A new phrase for the main menu was choosen:");
                ActivityLogger.Log(_currentSection, subSection, _sessionPhrase, true);
            }
            catch
            {
                ActivityLogger.Log(_currentSection, subSection, "Failed to get a new phrase for the main menu. Using a placeholder for now.");
                _sessionPhrase = "ᵐᵃᵈᵉ ᵗᵒ ˡᵃˢᵗ";
            }



        LabelDrawUi:

            Console.SetCursorPosition(0, 4);



            ActivityLogger.Log(_currentSection, subSection, "Starting to draw the main menu.");

            DisplayMenu();

            ActivityLogger.Log(_currentSection, subSection, "Displayed main menu, waiting for key input.");



            ConsoleKey pressedKey = Console.ReadKey(true).Key;

            switch (pressedKey)
            {
                case ConsoleKey.DownArrow:
                    if (ValidOptionChange(_navigationXPosition, _navigationYPosition + 1))
                    {
                        _navigationYPosition += 1;
                    }
                    break;

                case ConsoleKey.UpArrow:
                    if (ValidOptionChange(_navigationXPosition, _navigationYPosition - 1))
                    {
                        _navigationYPosition -= 1;
                    }
                    break;

                case ConsoleKey.RightArrow:
                    if (ValidOptionChange(_navigationXPosition + 1, _navigationYPosition))
                    {
                        _navigationXPosition += 1;
                    }
                    break;

                case ConsoleKey.LeftArrow:
                    if (ValidOptionChange(_navigationXPosition - 1, _navigationYPosition))
                    {
                        _navigationXPosition -= 1;
                    }
                    break;

                case ConsoleKey.Escape:
                    return;

                default:
                    break;
            }



            if (pressedKey != ConsoleKey.Enter)
            {
                goto LabelDrawUi;
            }



            int selectedMenuOption = _menuOptionsGrid[_navigationYPosition, _navigationXPosition];

            ActivityLogger.Log(_currentSection, subSection, $"Menu option {selectedMenuOption} ({_kahoofectionOptions[selectedMenuOption]}) was selected, redirecting ...");



            Console.CursorVisible = true;

            switch (selectedMenuOption)
            {
                case 1:
                    await QuizIdByName.Start();
                    break;

                case 2:
                    await QuizIdChecker.Start();
                    break;

                case 3:
                    await QuizIdAnswers.Start();
                    break;

                case 4:
                    await PinBruteforcer.Start();
                    break;

                case 5:
                    await GamePinSpammer.Start();
                    break;

                case 6:
                    // TODO: Redirect to AutoplayWithGamePin
                    break;
            }

            Console.CursorVisible = false;



            ActivityLogger.Log(_currentSection, subSection, $"Redrawing main menu after returning from the selected menu option.");

            Console.Clear();
            goto LabelDrawUi;
        }

        private static void DisplayMenu()
        {
            string stateQuizIdName = $"[\u001b[94m{(_navigationXPosition == 1 && _navigationYPosition == 1 ? ">" : " ")}\u001b[97m]";
            string stateQuizIdChecker = $"[\u001b[94m{(_navigationXPosition == 1 && _navigationYPosition == 2 ? ">" : " ")}\u001b[97m]";
            string stateQuizIdAnswers = $"[\u001b[94m{(_navigationXPosition == 1 && _navigationYPosition == 3 ? ">" : " ")}\u001b[97m]";
            string stateBruteforcePins = $"[\u001b[94m{(_navigationXPosition == 2 && _navigationYPosition == 1 ? ">" : " ")}\u001b[97m]";
            string stateSpamGameByPin = $"[\u001b[94m{(_navigationXPosition == 2 && _navigationYPosition == 2 ? ">" : " ")}\u001b[97m]";
            string stateAutoplayByPin = $"[\u001b[94m{(_navigationXPosition == 2 && _navigationYPosition == 3 ? ">" : " ")}\u001b[97m]";

            (string lowerLine, string upperLine) = FormatPhraseLine();


            
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(@"              _  __ __  _  _  __   __  ___ ___ ________ _  __  __  _    ");
            Console.WriteLine(@"             | |/ //  \| || |/__\ /__\| __| __/ _/_   _| |/__\|  \| |   ");
            Console.WriteLine(@"             |   <| /\ | >< | \/ | \/ | _|| _| \__ | | | | \/ | | ' |   ");
            Console.WriteLine(@"             |_|\_\_||_|_||_|\__/ \__/|_| |___\__/ |_| |_|\__/|_|\__|   ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("                                                                         ");
            Console.WriteLine("                                                                         ");
            Console.WriteLine("                                                                         ");
            Console.WriteLine("             \u001b[94m┌ \u001b[97mInformation                  \u001b[94m┌ \u001b[97mGameplay");
            Console.WriteLine("             \u001b[94m└──────────────────┐           └────────────────────────┐\u001b[97m");
            Console.WriteLine("             {0} QuizId by name             {1} Bruteforce game pins     ", stateQuizIdName, stateBruteforcePins);
            Console.WriteLine("             {0} QuizId checker             {1} Spam game by pin         ", stateQuizIdChecker, stateSpamGameByPin);
            Console.WriteLine("             {0} QuizId answers             {1} Autoplay by pin          ", stateQuizIdAnswers, stateAutoplayByPin);
            Console.WriteLine("                                                                         ");
            Console.WriteLine("                                                                         ");
            Console.WriteLine("                                                                         ");
            Console.WriteLine("                                                                         ");
            Console.WriteLine("                                                                         ");
            Console.WriteLine("             \u001b[94m┌───────────────────────────┬───────────────────────────┐   ");
            Console.WriteLine("             \u001b[94m│ \u001b[97mNavigate using ARROW KEYS \u001b[94m│ \u001b[97mConfirm choice with ENTER \u001b[94m│   ");
            Console.WriteLine("             {0}", upperLine);
            Console.WriteLine("             {0}", lowerLine);
        }

        private static bool ValidOptionChange(int newOptionX, int newOptionY)
        {
            if (newOptionX < 1 || newOptionY < 1)
            {
                return false;
            }

            if (newOptionY >= _menuOptionsGrid.GetLength(0) || newOptionX >= _menuOptionsGrid.GetLength(1))
            {
                return false;
            }

            return _menuOptionsGrid[newOptionY, newOptionX] != 0;
        }

        private static (string lowerLine, string upperLine) FormatPhraseLine()
        {
            int totalBarWidth = 57;
            int halfBarWidth = 27;

            string lowerLine = $"└─> {_sessionPhrase} <─┘";
            int spaceLeft = totalBarWidth - lowerLine.Length;
            int halfSpaceLeft = spaceLeft / 2;
            
            lowerLine = $"\u001b[94m{new string(' ', halfSpaceLeft)}\u001b[94m└─> \u001b[97m{_sessionPhrase} \u001b[94m<─┘{new string(' ', halfSpaceLeft)}";
            string upperLine = $"\u001b[94m└{new string('─', halfSpaceLeft - 1)}┬{new string('─', halfBarWidth - halfSpaceLeft)}┴";

            string oddLengthFormat = $"\u001b[94m{new string('─', halfBarWidth - halfSpaceLeft - 1)}┬{new string('─', halfSpaceLeft)}┘";
            string evenLengthFormat = $"\u001b[94m{new string('─', halfBarWidth - halfSpaceLeft)}┬{new string('─', halfSpaceLeft - 1)}┘";

            upperLine += spaceLeft % 2 == 0 ? evenLengthFormat : oddLengthFormat;

            return (lowerLine, upperLine);
        }
    }
}