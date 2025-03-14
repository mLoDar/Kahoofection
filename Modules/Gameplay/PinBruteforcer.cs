using Kahoofection.Scripts;
using Kahoofection.Ressources;
using Kahoofection.Scripts.Kahoot;





namespace Kahoofection.Modules.Gameplay
{
    internal class PinBruteforcer
    {
        private const string _currentSection = "PinBruteforcer";

        private static readonly Random _numberGenerator = new();
        private static readonly ApplicationSettings.Urls _appUrls = new();

        private static int _currentAttempt = 0;
        private const int _totalBruteforceAttempts = 50;

        private static Task? _creationTask;
        private static List<string> _validPins = [];
        private static bool _interfaceNeedsRedraw = false;
        private static ConsoleKey _pressedKey = ConsoleKey.None;
        
        

        internal static async Task Start()
        {
            string subSection = "Main";

            Console.Title = $"Kahoofection | {_currentSection}";
            ActivityLogger.Log(_currentSection, subSection, $"Starting module '{_currentSection}'");



            ConsoleHelper.ResetConsole();



            ActivityLogger.Log(_currentSection, subSection, $"Starting the generation of {_totalBruteforceAttempts} pins.");

            _creationTask = new Task(async () =>
            {
                for (int i = 0; i <= _totalBruteforceAttempts; i++)
                {
                    _currentAttempt = i;

                    await GenerateNewPin();
                }
            });

            _creationTask.Start();

            Console.CursorVisible = false;

            

        LabelRedraw:

            Console.SetCursorPosition(0, 4);
            DrawInterface();



        LabelReadKey:

            _pressedKey = ConsoleKey.None;



            CancellationTokenSource cancellationTokenSource = new();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            Task keyPressListener = ListenForKeyPress(cancellationToken);
            Task pinGenerationListener = NewPinGenerated(cancellationToken);

            await Task.WhenAny(keyPressListener, pinGenerationListener);

            cancellationTokenSource.Cancel();



            if (_pressedKey == ConsoleKey.Escape || _pressedKey == ConsoleKey.Backspace)
            {
                ActivityLogger.Log(_currentSection, subSection, $"Leaving module, as the ESC or BACKSPACE key was pressed.");

                _validPins = [];

                return;
            }

            if (_pressedKey != ConsoleKey.None)
            {
                goto LabelReadKey;
            }

            _pressedKey = ConsoleKey.None;

            goto LabelRedraw;
        }

        private static async Task GenerateNewPin()
        {
            string subSection = "GenerateNewPin";

            string generatedPin = _numberGenerator.Next(100000, 999999).ToString();

            (int gamePin, Exception? occurredError) = await KahootValidator.ValidGamePin(generatedPin);

            if (occurredError == null)
            {
                string urlJoinPin = $"{_appUrls.kahootJoinPin}";
                urlJoinPin = urlJoinPin.Replace("{gamePin}", gamePin.ToString());

                _validPins.Add(urlJoinPin);

                ActivityLogger.Log(_currentSection, subSection, $"Successfully found a valid pin after {_currentAttempt} attempts.");
                ActivityLogger.Log(_currentSection, subSection, $"Valid pin: {urlJoinPin}", true);
            }

            _interfaceNeedsRedraw = true;
        }

        private static void DrawInterface()
        {
            int maxPinsToDisplay = 12;

            Console.WriteLine("             \u001b[94m┌─> \u001b[97mAttempt [{0}/{1}]           ", _currentAttempt, _totalBruteforceAttempts);
            Console.WriteLine("             \u001b[94m├──────────────────────────────────┐");

            if (_validPins.Count == 0)
            {
                Console.WriteLine("             \u001b[94m│   \u001b[97mValid pins will show up here   \u001b[94m│");
            }
            else
            {
                for (int i = 0; i < _validPins.Count && i < maxPinsToDisplay; i++)
                {
                    string currentValidPin = _validPins[i];
                    Console.WriteLine("             \u001b[94m│ \u001b[92m√ \u001b[97m{0}   \u001b[94m│", currentValidPin);
                }
            }

            Console.WriteLine("             \u001b[94m└──────────────────────────────────┘");
            Console.WriteLine("                                                           ");
            Console.WriteLine("             \u001b[94m┌ \u001b[97mUse BACKSPACE to return           ");
            Console.WriteLine("             \u001b[94m└──────────────────────────────────┘");
        }

        private static async Task ListenForKeyPress(CancellationToken cancellationToken)
        {
            while (_interfaceNeedsRedraw == false)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                if (Console.KeyAvailable == false)
                {
                    await Task.Delay(50, cancellationToken);
                    continue;
                }

                _pressedKey = Console.ReadKey(true).Key;

                _interfaceNeedsRedraw = false;

                return;
            }
        }

        private static async Task NewPinGenerated(CancellationToken cancellationToken)
        {
            while (_interfaceNeedsRedraw == false)
            {
                await Task.Delay(1000, cancellationToken);
            }

            _interfaceNeedsRedraw = false;
        }
    }
}