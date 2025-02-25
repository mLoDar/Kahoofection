using Kahoofection.Scripts;
using Kahoofection.Ressources;
using Kahoofection.Scripts.Kahoot;

using Newtonsoft.Json.Linq;



#pragma warning disable CA1845 // Use span-based 'string.Concat'





namespace Kahoofection.Modules.Information
{
    internal class QuizIdAnswers
    {
        private const string _currentSection = "QuizIdAnswers";

        private static readonly ApplicationSettings.Urls _appUrls = new();



        internal static async Task Start()
        {
            string subSection = "Main";

        LabelMethodEntryPoint:

            Console.Title = $"Kahoofection | {_currentSection}";
            ActivityLogger.Log(_currentSection, subSection, $"Starting module '{_currentSection}'");



            ConsoleHelper.ResetConsole();

            Console.WriteLine("             \u001b[94m┌ \u001b[97mEnter a QuizId        ");
            Console.WriteLine("             \u001b[94m├─────────────────────   ");
            Console.Write("             \u001b[94m└─> \u001b[97m");



            ActivityLogger.Log(_currentSection, subSection, $"Prompted to provide a QuizId, waiting for an input.");

            (bool escapeKeyPressed, string lineContent) = await ConsoleHelper.ReadLine();

            if (escapeKeyPressed == true)
            {
                ActivityLogger.Log(_currentSection, subSection, $"Leaving module, as the input was cancelled via the ESC key.");
                return;
            }

            if (string.IsNullOrWhiteSpace(lineContent))
            {
                ActivityLogger.Log(_currentSection, subSection, "Re-entering the module as an invalid QuizId was provided (Empty string).");
                goto LabelMethodEntryPoint;
            }

            if (Guid.TryParse(lineContent, out Guid convertedQuizId) == false)
            {
                ActivityLogger.Log(_currentSection, subSection, "Re-entering the module as an invalid QuizId was provided (No valid guuid).");
                goto LabelMethodEntryPoint;
            }

            ActivityLogger.Log(_currentSection, subSection, "Received a valid string as a QuizId.");
            ActivityLogger.Log(_currentSection, subSection, $"Input: '{lineContent}'");



            ConsoleHelper.ResetConsole();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("             Loading ...");
            Console.WriteLine("             Please be patient.");



            ActivityLogger.Log(_currentSection, subSection, "Fetching quiz data from the API endpoint with the provided QuizId.");

            string providedQuizId = convertedQuizId.ToString();
            string requestUrl = $"{_appUrls.kahootCheckQuizId.Replace("{quizId}", providedQuizId)}";
            string apiResponse = await WebConnection.CreateRequest(requestUrl);



            ConsoleHelper.ResetConsole();



            if (string.IsNullOrEmpty(apiResponse))
            {
                ActivityLogger.Log(_currentSection, subSection, "Received an invalid response from the API, the response was empty.");
                ActivityLogger.Log(_currentSection, subSection, $"Most likely no quiz with the QuizId '{providedQuizId}' exists.", true);

                string title = "QuizId search failed";
                string description = "No quiz was found. Please try again with a different QuizId.";

                await ConsoleHelper.DisplayInformation(title, description, ConsoleColor.Red);

                goto LabelMethodEntryPoint;
            }



            JObject quizData;
            JArray quizQuestions;

            try
            {
                quizData = JObject.Parse(apiResponse);

                if (quizData == null || quizData.GetValue("error") != null)
                {
                    throw new Exception("Failed to parse data into an object");
                }

                quizQuestions = quizData.GetValue("questions") as JArray ?? [];

                if (quizQuestions == null || quizQuestions == new JArray())
                {
                    throw new Exception("Failed to get quiz questions. 'questions' does not exist or is not an array.");
                }
            }
            catch (Exception exception)
            {
                ActivityLogger.Log(_currentSection, subSection, "Received an invalid response from the API, failed to parse the response.");
                ActivityLogger.Log(_currentSection, subSection, $"Exception: {exception.Message}", true);

                string title = "QuizId answers failed";
                string description = "Please look at the error logs to fix this problem.";

                await ConsoleHelper.DisplayInformation(title, description, ConsoleColor.Red);

                goto LabelMethodEntryPoint;
            }



            int maximumBoxWidth = 91;
            List<string> formattedQuestions = [];

            formattedQuestions.Add(string.Empty);

            for (int questionIndex = 0; questionIndex < quizQuestions.Count; questionIndex++)
            {
                string fetchedType;
                string fetchedTitle;
                List<string> fetchedAnswers;

                try
                {
                    JObject questionData = JObject.Parse(quizQuestions[questionIndex].ToString());

                    (bool successfullyFetchedAnswer, fetchedType, fetchedTitle, fetchedAnswers) = KahootHelper.GetQuestionsAnswer(questionIndex, questionData);

                    if (successfullyFetchedAnswer == false)
                    {
                        throw new Exception("Kahoot helper handled the question, but an error occurred.");
                    }
                }
                catch (Exception exception)
                {
                    ActivityLogger.Log(_currentSection, subSection, $"Failed to fetch details for question '{questionIndex}'.");
                    ActivityLogger.Log(_currentSection, subSection, $"Exception: {exception}", true);
                    
                    formattedQuestions.Add($"\x1B[91m{questionIndex}.) Failed to fetch question\x1B[97m");
                    formattedQuestions.Add(string.Empty);

                    continue;
                }



                string prefix = $"{questionIndex + 1}.) {fetchedType} │ ";
                int maximumTitleLength = maximumBoxWidth - prefix.Length;

                try
                {
                    for (int i = 0; i < fetchedTitle.Length; i += maximumTitleLength)
                    {
                        if (i == 0)
                        {
                            formattedQuestions.Add($"{prefix}{fetchedTitle.Substring(i, Math.Min(maximumTitleLength, fetchedTitle.Length - i))}");
                            continue;
                        }

                        formattedQuestions.Add(new string(' ', prefix.Length - 2) + "│ " + fetchedTitle.Substring(i, Math.Min(maximumTitleLength, fetchedTitle.Length - i)));
                    }

                    formattedQuestions.Add(new string(' ', prefix.Length - 7) + "─────┴─────");

                    foreach (string questionAnswer in fetchedAnswers)
                    {
                        for (int j = 0; j < questionAnswer.Length; j += maximumBoxWidth)
                        {
                            formattedQuestions.Add(questionAnswer.Substring(j, Math.Min(maximumBoxWidth, questionAnswer.Length - j)));
                        }
                    }

                    formattedQuestions.Add(string.Empty);
                }
                catch (Exception exception)
                {
                    ActivityLogger.Log(_currentSection, subSection, "Failed to fetch questions or format them.");
                    ActivityLogger.Log(_currentSection, subSection, $"Excpetion: {exception}", true);
                }

                ActivityLogger.Log(_currentSection, subSection, "Successfully fetched all questions with their details and formatted them for the display box.");
            }



            int viewStartIndex = 0;
            int linesDisplayedAtOnce = 16;

            Console.CursorVisible = false;

            ActivityLogger.Log(_currentSection, subSection, $"Starting to draw display box with {linesDisplayedAtOnce} lines at once.");



        LabelDrawAnswerBox:

            Console.SetCursorPosition(0, 4);

            DrawAnswerBox(maximumBoxWidth, formattedQuestions, viewStartIndex, linesDisplayedAtOnce, providedQuizId);


            ConsoleKey pressedKey = Console.ReadKey(true).Key;

            switch (pressedKey)
            {
                case ConsoleKey.DownArrow:
                    if (viewStartIndex + 1 <= formattedQuestions.Count - linesDisplayedAtOnce)
                    {
                        viewStartIndex += 1;
                    }
                    break;

                case ConsoleKey.UpArrow:
                    if (viewStartIndex - 1 >= 0)
                    {
                        viewStartIndex -= 1;
                    }
                    break;

                case ConsoleKey.Escape:
                    ActivityLogger.Log(_currentSection, subSection, "Returning to the main menu selection via 'ESC'.");
                    return;

                case ConsoleKey.Backspace:
                    ActivityLogger.Log(_currentSection, subSection, "Returning to the main menu via 'BACKSPACE'.");
                    return;

                default:
                    break;
            }



            goto LabelDrawAnswerBox;
        }

        private static void DrawAnswerBox(int maximumBoxWidth, List<string> formattedQuestions, int viewStartIndex, int linesDisplayedAtOnce, string quizId)
        {
            Console.WriteLine("             \u001b[97m┌─> QuizdId: '\u001b[92m{0}\u001b[97m'", quizId);
            Console.WriteLine("             \u001b[97m├─────────────────────────────────────────────────────────────────────────────────────────────┐   ┬");

            int scrollBarHelper = 0;
            var (scrollBarStart, scrollBarEnd) = GetScrollbarRange(formattedQuestions.Count, linesDisplayedAtOnce, viewStartIndex);
            
            for (int currentViewIndex = viewStartIndex; currentViewIndex < viewStartIndex + linesDisplayedAtOnce && currentViewIndex < formattedQuestions.Count; currentViewIndex++)
            {
                string questionLine = formattedQuestions[currentViewIndex];

                int ansi = ConsoleHelper.GetCombinedAnsiSequenceLength(questionLine);
                int leftOverSpaces = maximumBoxWidth - questionLine.Length + ansi;

                bool helperWithinRange = scrollBarHelper >= scrollBarStart && scrollBarHelper <= scrollBarEnd;

                Console.WriteLine($"             \u001b[97m│ \u001b[97m{questionLine}{new string(' ', leftOverSpaces)} \u001b[97m│   {(helperWithinRange == true ? "\u001b[94m█" : "\u001b[97m│")}");

                scrollBarHelper++;
            }
            
            Console.WriteLine("             \u001b[97m└─────────────────────────────────────────────────────────────────────────────────────────────┘   ┴");
            Console.WriteLine("                                                                                             ");
            Console.WriteLine("             \u001b[94m┌ \u001b[97mNavigate with the ARROW keys                                        Use BACKSPACE to return \u001b[94m┐\u001b[97m");
            Console.WriteLine("             \u001b[94m└────────────────────────────────                                  ───────────────────────────┘\u001b[97m");
        }

        private static (int scrollBarStart, int scrollBarEnd) GetScrollbarRange(int totalQuestions, int linesDisplayedAtOnce, int viewStartIndex)
        {
            int scrollBarHeight = (int)Math.Round((double)linesDisplayedAtOnce / totalQuestions * linesDisplayedAtOnce);

            if (scrollBarHeight <= 0)
            {
                scrollBarHeight = 1;
            }

            double linesPerPart = (double)viewStartIndex / (totalQuestions - linesDisplayedAtOnce);

            int scrollBarStart = (int)Math.Round(linesPerPart * (linesDisplayedAtOnce - scrollBarHeight));
            int scrollBarEnd = scrollBarStart + scrollBarHeight;

            return (scrollBarStart, scrollBarEnd);
        }
    }
}