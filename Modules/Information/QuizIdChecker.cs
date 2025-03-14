using Kahoofection.Scripts;
using Kahoofection.Ressources;
using Kahoofection.Scripts.Kahoot;

using Newtonsoft.Json.Linq;



#pragma warning disable IDE0057 // Use range operator
#pragma warning disable CA1845 // Use span-based 'string.Concat'





namespace Kahoofection.Modules.Information
{
    internal struct QuizIdCheckData
    {
        internal string title;
        internal string quizId;
        internal string coverUrl;
        internal string creatorId;
        internal string creatorName;
        internal int questionCount;
    }



    class QuizIdChecker
    {
        private const string _currentSection = "QuizIdChecker";



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



            (_, string apiResponse, Exception? quizIdError) = await KahootValidator.ValidQuizId(lineContent);

            if (quizIdError != null)
            {
                ActivityLogger.Log(_currentSection, subSection, "Re-entering the module as an invalid QuizId was provided.");
                ActivityLogger.Log(_currentSection, subSection, $"Exception: {quizIdError.Message}");

                ConsoleHelper.ResetConsole();

                string title = "QuizId check failed";
                string description = "Most likely an invalid QuizId was provided. Please look at the error logs to fix this problem.";

                await ConsoleHelper.DisplayInformation(title, description, ConsoleColor.Red);

                goto LabelMethodEntryPoint;
            }

            ActivityLogger.Log(_currentSection, subSection, "Received a valid string as a QuizId.");
            ActivityLogger.Log(_currentSection, subSection, $"Input: '{lineContent}'");



            ConsoleHelper.ResetConsole();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("             Loading ...");
            Console.WriteLine("             Please be patient.");



            ActivityLogger.Log(_currentSection, subSection, "Trying to parse the API response and get quizzes.");

            (bool successfullyParsed, Exception? occurredError, QuizIdCheckData quizData) = ParseApiResponse(apiResponse);

            if (successfullyParsed == false)
            {
                ActivityLogger.Log(_currentSection, subSection, "Failed to parse the API response.");
                if (occurredError != null)
                {
                    ActivityLogger.Log(_currentSection, subSection, $"Occurred error: {occurredError.Message}", true);
                }
                ActivityLogger.Log(_currentSection, subSection, $"API response: {apiResponse}", true);

                ConsoleHelper.ResetConsole();

                string title = "QuizId check failed";
                string description = "Please look at the error logs to fix this problem.";

                await ConsoleHelper.DisplayInformation(title, description, ConsoleColor.Red);

                goto LabelMethodEntryPoint;
            }



            ConsoleHelper.ResetConsole();



            ActivityLogger.Log(_currentSection, subSection, $"Displaying the formatted search results from the API endpoint.");

            DisplayQuizData(quizData);

            ActivityLogger.Log(_currentSection, subSection, $"Displayed the formatted quiz data for the current QuizId.");



            (int cursorLeft, int cursorTop) = Console.GetCursorPosition();

            int options = 2;
            int currentPosition = 1;

            Console.CursorVisible = false;



            ActivityLogger.Log(_currentSection, subSection, $"Displaying menu for next options.");

        LabelDisplayMenu:

            Console.SetCursorPosition(cursorLeft, cursorTop);

            Console.WriteLine("                                  ");
            Console.WriteLine("                                  ");
            Console.WriteLine("                                  ");
            Console.WriteLine("             \u001b[94m┌ \u001b[97mOptions            ");
            Console.WriteLine("             \u001b[94m└──────────────────┐ \u001b[97m");
            Console.WriteLine("             {0} QuizId checker   ", $"[\u001b[94m{(currentPosition == 1 ? ">" : " ")}\u001b[97m]");
            Console.WriteLine("             {0} Main menu        ", $"[\u001b[94m{(currentPosition == 2 ? ">" : " ")}\u001b[97m]");

            

            ConsoleKey pressedKey = Console.ReadKey(true).Key;

            switch (pressedKey)
            {
                case ConsoleKey.DownArrow:
                    if (currentPosition + 1 <= options)
                    {
                        currentPosition += 1;
                    }
                    break;

                case ConsoleKey.UpArrow:
                    if (currentPosition - 1 > 0)
                    {
                        currentPosition -= 1;
                    }
                    break;

                case ConsoleKey.Escape:
                    return;

                default:
                    break;
            }



            if (pressedKey != ConsoleKey.Enter)
            {
                goto LabelDisplayMenu;
            }



            ActivityLogger.Log(_currentSection, subSection, $"Waiting for option choice.");

            switch (currentPosition)
            {
                case 1:
                    ActivityLogger.Log(_currentSection, subSection, $"Restarting module.");
                    Console.CursorVisible = true;
                    goto LabelMethodEntryPoint;

                case 2:
                    ActivityLogger.Log(_currentSection, subSection, $"Returning to the main menu.");
                    return;

                default:
                    break;
            }

            goto LabelDisplayMenu;
        }

        private static (bool successfullyParsed, Exception? occurredError, QuizIdCheckData quizData) ParseApiResponse(string apiResponse)
        {
            try
            {
                JObject foundData = JObject.Parse(apiResponse);

                QuizIdCheckData quizIdCheckData = new()
                {
                    title = foundData["title"]?.ToString() ?? "-",
                    quizId = foundData["uuid"]?.ToString() ?? "-",
                    coverUrl = foundData["cover"]?.ToString() ?? "-",
                    creatorId = foundData["creator"]?.ToString() ?? "-",
                    creatorName = foundData["creator_username"]?.ToString() ?? "-"
                };
                
                JArray? quizQuestions = (JArray?)foundData?.SelectToken("questions");

                if (quizQuestions == null)
                {
                    quizIdCheckData.questionCount = 0;
                }
                else
                {
                    quizIdCheckData.questionCount = quizQuestions.Count;
                }

                return (true, null, quizIdCheckData);
            }
            catch (Exception exception)
            {
                return (false, exception, new());
            }
        }

        private static void DisplayQuizData(QuizIdCheckData quizData)
        {
            string quizTitle = quizData.title.Length > 40 ? quizData.title.Substring(0, 36) + " ..." : quizData.title;
            string formattedQuestionCount;
            int spaceFillHelper = 65 - 19 - quizData.questionCount.ToString().Length;

            if (quizData.questionCount > 999)
            {
                formattedQuestionCount = "                                          \u001b[97mTotal Questions: 999+ \u001b[94m│";
            }
            else
            {
                formattedQuestionCount = new string(' ', spaceFillHelper) + "\u001b[97mTotal Questions: " + quizData.questionCount + " \u001b[94m│";
            }



            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("             \u001b[94m┌─ \u001b[97mQuizIdCheck");
            Console.WriteLine("             \u001b[94m├─────");
            Console.WriteLine("             \u001b[94m│ \u001b[97mTitle: {0}", quizTitle);
            Console.WriteLine("             \u001b[94m│ \u001b[97mQuizId: {0}", quizData.quizId);
            Console.WriteLine("             \u001b[94m└──────────────────────────────────────────────────┐");
            Console.WriteLine(formattedQuestionCount);
            Console.WriteLine("             \u001b[94m┌──────────────────────────────────────────────────┘");
            Console.WriteLine("             \u001b[94m│ \u001b[97mCreatorId: {0}", quizData.creatorId);
            Console.WriteLine("             \u001b[94m│ \u001b[97mUsername: {0}", quizData.creatorName);
            Console.WriteLine("             \u001b[94m└─────");
        }
    }
}