using Kahoofection.Ressources;

using Newtonsoft.Json.Linq;





namespace Kahoofection.Scripts.Kahoot
{
    internal class KahootValidator
    {
        private const string _currentSection = "KahootValidator";

        private static readonly ApplicationSettings.Urls _appUrls = new();
        private static readonly ApplicationSettings.Runtime _appRuntime = new();



        internal static async Task<(int gamePin, Exception? occurredError)> ValidGamePin(string input)
        {
            string subSection = "CheckGamePin";

            ActivityLogger.Log(_currentSection, subSection, $"Checking input '{input}' for a valid GamePin.");

            input = RegexPatterns.NoNumbers().Replace(input, string.Empty);

            if (string.IsNullOrWhiteSpace(input))
            {
                ActivityLogger.Log(_currentSection, subSection, $"Input is null or whitespace, returning result.");
                return (-1, new Exception("The input does not contain any numbers."));
            }

            if (int.TryParse(input, out int gamePin) == false)
            {
                ActivityLogger.Log(_currentSection, subSection, $"Input is not a valid number, returning result.");
                return (-1, new Exception("The input is not a valid number."));
            }

            if (input.Length > _appRuntime.kahootGamePinFormat.Length)
            {
                ActivityLogger.Log(_currentSection, subSection, $"Input's format does not match GamePin pattern, returning result.");
                return (-1, new Exception($"GamePin's format does not match pattern ({_appRuntime.kahootGamePinFormat.Length} digits)."));
            }



            JObject gameData;

            try
            {
                long millisTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                string sessionUrl = _appUrls.kahootSessionReservation;
                sessionUrl = sessionUrl.Replace("{gamePin}", gamePin.ToString());
                sessionUrl = sessionUrl.Replace("{millisTimestamp}", millisTimestamp.ToString());

                string apiResponse = await WebConnection.CreateRequest(sessionUrl);

                gameData = JObject.Parse(apiResponse);
            }
            catch (Exception exception)
            {
                ActivityLogger.Log(_currentSection, subSection, $"Received invalid game data as response. Failed to parse game data.");
                ActivityLogger.Log(_currentSection, subSection, exception.Message, true);

                return (-1, new Exception("No valid pin was provided"));
            }

            ActivityLogger.Log(_currentSection, subSection, $"The provided input '{input}' contains a valid GamePin!");



            try
            {
                JToken? gameTwoFactorAuth = gameData.SelectToken("twoFactorAuth") ?? throw new Exception("Failed to find information about TwoFactorAuthentication. Invalid Pin?");

                if (gameTwoFactorAuth.ToString().ToLower().Equals("true"))
                {
                    throw new Exception("Pin has TwoFactorAuthentication enabled.");
                }

                if (gameTwoFactorAuth.ToString().ToLower().Equals("false") == false)
                {
                    throw new Exception("Pin has no specifications about TwoFactorAuthentication (not true or false).");
                }
            }
            catch (Exception exception)
            {
                ActivityLogger.Log(_currentSection, subSection, $"Received invalid game data as response. Failed to parse data or find any information about TwoFactorAuthentication.");
                ActivityLogger.Log(_currentSection, subSection, exception.Message, true);

                return (-1, exception);
            }
            return (gamePin, null);
        }

        internal static async Task<(Guid convertedQuizId, string apiResponse, Exception? occurredError)> ValidQuizId(string providedQuizId)
        {
            string subSection = "ValidQuizId";

            ActivityLogger.Log(_currentSection, subSection, "Received a request to validate a string as a QuizId.");
            ActivityLogger.Log(_currentSection, subSection, $"Provided input: {providedQuizId}");



            if (string.IsNullOrWhiteSpace(providedQuizId))
            {
                ActivityLogger.Log(_currentSection, subSection, "An invalid QuizId was provided (Empty string).");

                return (new Guid(), string.Empty, new Exception("An invalid QuizId was provided (Empty string)."));
            }

            if (Guid.TryParse(providedQuizId, out Guid convertedQuizId) == false)
            {
                ActivityLogger.Log(_currentSection, subSection, "An invalid QuizId was provided (No valid UUID).");

                return (new Guid(), string.Empty, new Exception("An invalid QuizId was provided (No valid UUID)."));
            }



            string requestUrl = $"{_appUrls.kahootCheckQuizId.Replace("{quizId}", providedQuizId)}";
            string apiResponse = await WebConnection.CreateRequest(requestUrl);

            if (string.IsNullOrEmpty(apiResponse))
            {
                ActivityLogger.Log(_currentSection, subSection, "Received an invalid response from the API, the response was empty.");
                ActivityLogger.Log(_currentSection, subSection, $"Most likely no quiz with the QuizId '{providedQuizId}' exists.", true);

                return (new Guid(), string.Empty, new Exception("Invalid QuizId was provided (API response was empty)."));
            }



            JObject? foundData;

            try
            {
                foundData = JObject.Parse(apiResponse);

                if (foundData == null)
                {
                    throw new Exception("The parsed quiz data is null.");
                }

                if ((foundData["error"]?.ToString() ?? string.Empty).Equals("NOT_FOUND"))
                {
                    throw new Exception("The provided Guid was not validated as a Kahoot quiz.");
                }
            }
            catch (Exception exception)
            {
                ActivityLogger.Log(_currentSection, subSection, "An invalid QuizId was provided, as the application failed to convert the received API response.");
                ActivityLogger.Log(_currentSection, subSection, $"Exception: {exception.Message}", true);

                return (new Guid(), string.Empty, exception);
            }

            ActivityLogger.Log(_currentSection, subSection, "The provided input was proven as a valid QuizId!");

            return (convertedQuizId, apiResponse, null);
        }

        internal static Exception? ValidSpammerBotName(string providedInput)
        {
            string subSection = "ValidSpammerBotName";

            providedInput = RegexPatterns.AllWhitespaces().Replace(providedInput, string.Empty);

            if (string.IsNullOrWhiteSpace(providedInput))
            {
                ActivityLogger.Log(_currentSection, subSection, "Restarting the prompt for a BotName as the provided input is null or whitespace.");
                return (new Exception("BotName is null or whitespace."));
            }

            if (Enumerable.Range(1, 100).Contains(providedInput.Length) == false)
            {
                ActivityLogger.Log(_currentSection, subSection, "Restarting the prompt for a BotName as the provided input's length is not in the specified range.");
                return (new Exception("BotName's length is not in the specified range."));
            }

            return (null);
        }

        internal static Exception? ValidAutoplayUsername(string providedInput)
        {
            string subSection = "ValidAutoplayUsername";

            providedInput = RegexPatterns.AllWhitespaces().Replace(providedInput, string.Empty);

            if (string.IsNullOrWhiteSpace(providedInput))
            {
                ActivityLogger.Log(_currentSection, subSection, "Restarting the prompt for a Username as the provided input is null or whitespace.");
                return (new Exception("Username is null or whitespace."));
            }

            if (Enumerable.Range(1, 15).Contains(providedInput.Length) == false)
            {
                ActivityLogger.Log(_currentSection, subSection, "Restarting the prompt for a Username as the provided input's length is not in the specified range.");
                return (new Exception("Username's length is not in the specified range."));
            }

            return (null);
        }

        internal static (int gameBotCount, Exception? exception) ValidSpammerBotCount(string providedInput)
        {
            string subSection = "ValidSpammerBotCount";

            providedInput = RegexPatterns.NoNumbers().Replace(providedInput, string.Empty);

            if (string.IsNullOrWhiteSpace(providedInput))
            {
                ActivityLogger.Log(_currentSection, subSection, "Restarting the prompt for a BotCount as the provided input is null or whitespace.");
                return (-1, new Exception("The input does not contain any numbers."));
            }

            if (int.TryParse(providedInput, out int gameBotCount) == false)
            {
                ActivityLogger.Log(_currentSection, subSection, "Restarting the prompt for a BotCount as the provided input is an invalid int.");
                return (-1, new Exception("The input is not a valid number."));
            }

            if (Enumerable.Range(3, 100).Contains(gameBotCount) == false)
            {
                ActivityLogger.Log(_currentSection, subSection, "Restarting the prompt for a BotCount as the provided input is not in the specified range.");
                return (-1, new Exception("BotCount is not within the specified range."));
            }

            return (gameBotCount, null);
        }
    }
}