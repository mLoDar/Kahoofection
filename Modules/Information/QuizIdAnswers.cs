using Kahoofection.Scripts;
using Kahoofection.Ressources;

using Newtonsoft.Json.Linq;
using Kahoofection.Scripts.Miscellaneous;





namespace Kahoofection.Modules.Information
{
    internal class QuizIdAnswers
    {
        private const string _currentSection = "QuizIdAnswers";

        private static readonly ApplicationSettings.Urls _appUrls = new();



        internal static async Task Start()
        {
        LabelMethodEntryPoint:

            Console.Title = $"Kahoofection | {_currentSection}";
            ActivityLogger.Log(_currentSection, $"Starting module '{_currentSection}'");



            ConsoleHelper.ResetConsole();

            Console.WriteLine("             \u001b[94m┌ \u001b[97mEnter a QuizId        ");
            Console.WriteLine("             \u001b[94m├─────────────────────   ");
            Console.Write("             \u001b[94m└─> \u001b[97m");



            ActivityLogger.Log(_currentSection, $"Prompted to provide a QuizId, waiting for an input.");

            (bool escapeKeyPressed, string lineContent) = await ConsoleHelper.ReadLine();

            if (escapeKeyPressed == true)
            {
                ActivityLogger.Log(_currentSection, $"Leaving module, as the input was cancelled via the ESC key.");
                return;
            }

            if (string.IsNullOrWhiteSpace(lineContent))
            {
                ActivityLogger.Log(_currentSection, "Re-entering the module as an invalid QuizId was provided (Empty string).");
                goto LabelMethodEntryPoint;
            }

            if (Guid.TryParse(lineContent, out Guid convertedQuizId) == false)
            {
                ActivityLogger.Log(_currentSection, "Re-entering the module as an invalid QuizId was provided (No valid guuid).");
                goto LabelMethodEntryPoint;
            }

            ActivityLogger.Log(_currentSection, "Received a valid string as a QuizId.");
            ActivityLogger.Log(_currentSection, $"Input: '{lineContent}'");



            ConsoleHelper.ResetConsole();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("             Loading ...");
            Console.WriteLine("             Please be patient.");



            ActivityLogger.Log(_currentSection, "Fetching quiz data from the API endpoint with the provided QuizId.");

            string providedQuizId = convertedQuizId.ToString();
            string requestUrl = $"{_appUrls.kahootCheckQuizId}{providedQuizId}";
            string apiResponse = await WebConnection.CreateRequest(requestUrl);



            ConsoleHelper.ResetConsole();



            if (string.IsNullOrEmpty(apiResponse))
            {
                ActivityLogger.Log(_currentSection, "Received an invalid response from the API, the response was empty.");
                ActivityLogger.Log(_currentSection, $"Most likely no quiz with the QuizId '{providedQuizId}' exists.", true);

                string title = "QuizId search failed";
                string description = "No quiz was found. Please try again with a different QuizId.";

                await ConsoleHelper.DisplayInformation(title, description, ConsoleColor.Red);

                goto LabelMethodEntryPoint;
            }
        }
    }
}
