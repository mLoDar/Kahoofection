using Kahoofection.Scripts;
using Kahoofection.Ressources;

using Newtonsoft.Json.Linq;



#pragma warning disable IDE0270 // Use coalesce expression
#pragma warning disable IDE0057 // Use range operator
#pragma warning disable CA1845 // Use span-based 'string.Concat'





namespace Kahoofection.Modules.Information
{
    internal class QuizIdByName
    {
        private const string _currentSection = "QuizIdByName";

        private static readonly ApplicationSettings.Urls _appUrls = new();



        internal static async Task Start()
        {
        LabelMethodEntryPoint:

            Console.Title = $"Kahoofection | {_currentSection}";
            ActivityLogger.Log(_currentSection, $"Starting module '{_currentSection}'");



            ConsoleHelper.ResetConsole();

            Console.WriteLine("             \u001b[94m┌ \u001b[97mEnter a Quiz name      ");
            Console.WriteLine("             \u001b[94m├─────────────────────   ");
            Console.Write("             \u001b[94m└─> \u001b[97m");
            


            ActivityLogger.Log(_currentSection, $"Prompted to provide a quiz name, waiting for an input.");

            (bool escapeKeyPressed, string lineContent) = await ConsoleHelper.ReadLine();

            if (escapeKeyPressed == true)
            {
                ActivityLogger.Log(_currentSection, $"Leaving module, as the input was cancelled via the ESC key.");
                return;
            }

            if (string.IsNullOrWhiteSpace(lineContent))
            {
                ActivityLogger.Log(_currentSection, "Re-entering the module as an invalid name was provided (Empty string).");
                goto LabelMethodEntryPoint;
            }

            ActivityLogger.Log(_currentSection, "Received a valid string as a quiz name.");
            ActivityLogger.Log(_currentSection, $"Input: '{lineContent}'");



            ConsoleHelper.ResetConsole();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("             Loading ...");
            Console.WriteLine("             Please be patient.");



            Dictionary<string, string> requestParameters = new()
            {
                { "query", lineContent },
                { "limit", "5" },
                { "cursor", "0" },
                { "searchCluster", "1" },
                { "includeExtendedCounters", "false" },
                { "inventoryItemId", "ANY" }
            };



            ActivityLogger.Log(_currentSection, "Searching for quizzes with the provided input at the API endpoint.");

            string requestUrl = _appUrls.kahootQuizSearch;
            string apiResponse = await WebConnection.CreateRequest(requestUrl, requestParameters);



            ConsoleHelper.ResetConsole();



            if (string.IsNullOrEmpty(apiResponse))
            {
                ActivityLogger.Log(_currentSection, "Received an invalid response from the API, the response was empty.");

                string title = "Quiz search failed";
                string description = "Please try again with a different name or look at the error logs to fix this problem.";

                await ConsoleHelper.DisplayInformation(title, description, ConsoleColor.Red);

                goto LabelMethodEntryPoint;
            }



            ActivityLogger.Log(_currentSection, "Trying to parse the API response and get quizzes.");

            (bool successfullyParsed, Exception? occuredError, JArray? foundQuizzes) = ParseApiResponse(apiResponse);

            if (successfullyParsed == false || foundQuizzes == null)
            {
                ActivityLogger.Log(_currentSection, "Failed to parse the API response.");
                if (occuredError != null)
                {
                    ActivityLogger.Log(_currentSection, $"Occured error: {occuredError.Message}", true);
                }
                ActivityLogger.Log(_currentSection, $"API response: {apiResponse}", true);
                
                string title = "Quiz search failed";
                string description = "Please look at the error logs to fix this problem.";

                await ConsoleHelper.DisplayInformation(title, description, ConsoleColor.Red);

                goto LabelMethodEntryPoint;
            }

            if (foundQuizzes.Count == 0)
            {
                ActivityLogger.Log(_currentSection, "Failed to find any quizze for the current search. Restarting the module.");

                string title = "Quiz search failed";
                string description = "There are no quizzes that match your search.";

                await ConsoleHelper.DisplayInformation(title, description, ConsoleColor.Red);

                goto LabelMethodEntryPoint;
            }

            ActivityLogger.Log(_currentSection, $"Successfully parsed the API data and found {foundQuizzes.Count} quizzes.");



            int currentConsoleWidth = Console.BufferWidth;
            
            int longestTitleLength = foundQuizzes.Cast<JObject>()
                .Select(quiz => (JObject?)quiz.SelectToken("card"))
                .Where(quiz_Data => quiz_Data != null)
                .Select(quiz_Data =>
                {
                    string title = quiz_Data?["title"]?.ToString() ?? "-";
                    int difference = title.Length + 65 - currentConsoleWidth;

                    if (title.Length + 65 > currentConsoleWidth)
                    {
                        title = title[..(title.Length - difference - 11)] + " ...";
                    }

                    return title;
                })
                .Max(combo => combo.Length);

            string upperLine = $"┌{new string('─', 40)}─{new string('─', longestTitleLength + 11)}┐";
            string lowerLine = $"└{new string('─', 40)}─{new string('─', longestTitleLength + 11)}┘";



            Console.WriteLine($"             {upperLine}");

            foreach (JObject quiz in foundQuizzes.Cast<JObject>())
            {
                JObject? quizData = (JObject?)quiz.SelectToken("card");

                if (quizData == null)
                {
                    continue;
                }



                string quizId = quizData?["uuid"]?.ToString() ?? "-";
                string quizTitle = quizData?["title"]?.ToString() ?? "-";
                string quizCreator = quizData?["creator_username"]?.ToString() ?? "-";
                
                int titleLineLength = "ᴛɪᴛʟᴇ  ".Length + longestTitleLength;
                int creatorLineLength = $"ʙʏ      {quizCreator}".Length;

                if (quizCreator.Length > quizTitle.Length)
                {
                    quizCreator = quizCreator.Substring(0, quizTitle.Length - 4) + " ...";

                    creatorLineLength = $"ʙʏ      {quizCreator}".Length;
                }

                if (quizTitle.Length + 65 > currentConsoleWidth)
                {
                    int difference = quizTitle.Length + 65 - currentConsoleWidth;
                    quizTitle = quizTitle[..(quizTitle.Length - difference - 11)] + " ...";
                }

                int spaceToFill = longestTitleLength - quizTitle.Length + 1;

                if (spaceToFill < 0)
                {
                    spaceToFill = 0;
                }



                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"             │ '\u001b[92m{quizId}\u001b[97m'   ᴛɪᴛʟᴇ   {quizTitle}{new string(' ', spaceToFill)} │");//
                Console.WriteLine($"             ├───{new string('─', quizId.ToString().Length - 4)}─────┐ ʙʏ      {quizCreator}{new string(' ', longestTitleLength - creatorLineLength + 10)}│");
                Console.WriteLine($"             │   {new string(' ', quizId.ToString().Length + 1)}└───────────{new string('─', longestTitleLength)}┤");
            }
            
            Console.WriteLine($"             {lowerLine}");

            ActivityLogger.Log(_currentSection, $"Formatted all quizzes and displayed them.");



            (int cursorLeft, int cursorTop) = Console.GetCursorPosition();

            int options = 2;
            int currentPosition = 1;

            Console.CursorVisible = false;



            ActivityLogger.Log(_currentSection, $"Displaying a menu for the next options.");

        LabelDisplayMenu:

            Console.SetCursorPosition(cursorLeft, cursorTop);
            
            Console.WriteLine("                                  ");
            Console.WriteLine("             \u001b[94m┌ \u001b[97mOptions            ");
            Console.WriteLine("             \u001b[94m└──────────────────┐\u001b[97m");
            Console.WriteLine("             {0} QuizId by name   ", $"[\u001b[94m{(currentPosition == 1 ? ">" : " ")}\u001b[97m]");
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



            ActivityLogger.Log(_currentSection, $"Waiting for option choice.");

            switch (currentPosition)
            {
                case 1:
                    ActivityLogger.Log(_currentSection, $"Restarting module.");
                    Console.CursorVisible = true;
                    goto LabelMethodEntryPoint;

                case 2:
                    ActivityLogger.Log(_currentSection, $"Returning to the main menu.");
                    return;

                default:
                    break;
            }

            goto LabelDisplayMenu;
        }

        private static (bool successfullyParsed, Exception? occuredError, JArray? foundQuizzes) ParseApiResponse(string apiResponse)
        {
            JObject? foundData;

            try
            {
                foundData = JObject.Parse(apiResponse);

                if (foundData == null)
                {
                    throw new Exception();
                }
            }
            catch (Exception exception)
            {
                return (false, exception, null);
            }


            try
            {
                JArray? quizzes = (JArray?)foundData.SelectToken("entities");

                if (quizzes == null)
                {
                    throw new Exception("Quizzes data was parsed, but is null.");
                }

                return (true, null, quizzes);
            }
            catch (Exception exception)
            {
                return (false, exception, null);
            }
        }
    }
}