using System.Text;
using System.Drawing;

using Kahoofection.Ressources;
using Kahoofection.Modules.Gameplay;

using OpenQA.Selenium;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium.Support.UI;





namespace Kahoofection.Scripts.Kahoot
{
    internal class KahootHelper
    {
        private const string _currentSection = "KahootHelper";

        private static readonly ApplicationSettings.Urls _appUrls = new();
        private static readonly ApplicationSettings.Paths _appPaths = new();
        private static readonly ApplicationSettings.DriverPaths _appDriverPaths = new();



        internal static (bool successfullyFetchedAnswer, string fetchedType, string fetchedTitle, List<string> fetchedAnswers) GetQuestionsAnswer(int questionIndex, JObject questionData)
        {
            string subSection = "GetQuestionsAnswer";

            JToken? questionType = questionData.SelectToken("type");
            JToken? questionTitle = questionData.SelectToken("question");



            if (questionType == null)
            {
                ActivityLogger.Log(_currentSection, subSection, $"Failed to fetch the question type.");
                ActivityLogger.Log(_currentSection, subSection, $"QuestionData: {questionData.ToString(Formatting.None)}", true);

                return (false, string.Empty, string.Empty, []);
            }

            if (questionTitle == null && questionType.ToString().Equals("content") == false)
            {
                ActivityLogger.Log(_currentSection, subSection, $"Failed to fetch the questions title. (Questions type is not 'content')");
                ActivityLogger.Log(_currentSection, subSection, $"QuestionData: {questionData.ToString(Formatting.None)}", true);

                return (false, string.Empty, string.Empty, []);
            }

            if (questionType.ToString().Equals("content"))
            {
                questionTitle = questionData.SelectToken("title") ?? "-";
            }

            if (questionTitle == null)
            {
                ActivityLogger.Log(_currentSection, subSection, $"The questions title is null although it was already fetched.");
                ActivityLogger.Log(_currentSection, subSection, $"QuestionData: {questionData.ToString(Formatting.None)}", true);

                questionTitle = "\u001b[97mFailed to fetch question title";
            }




            bool successfullyFetchedAnswer = true;
            bool answerAdded = false;
            List<string> questionAnswers = [];

            string prefix = $"{questionIndex + 1}.) {questionType} | ";

            switch (questionType.ToString().ToLower())
            {
                case "quiz":
#pragma warning disable CS8604 // Possible null reference argument.

                    bool needToUseImageId = false;

                    if (questionData["choices"].FirstOrDefault(choice => (bool)choice["correct"]) is not JObject firstCorrectChoice)
                    {
                        ActivityLogger.Log(_currentSection, subSection, $"Failed to find a correct choice for question type 'quiz'. (firstCorrectChoice is null)");
                        ActivityLogger.Log(_currentSection, subSection, $"QuestionData: {questionData?.ToString(Formatting.None)}", true);

                        questionAnswers.Add(new string(' ', prefix.Length) + "\u001b[91mFailed to fetch questions answer.\u001b[97m");
                        answerAdded = true;
                        break;
                    }



                    string? questionAnswer = firstCorrectChoice.SelectToken("answer")?.ToString() ?? string.Empty;

                    if (string.IsNullOrWhiteSpace(questionAnswer))
                    {
                        needToUseImageId = true;

                        questionAnswer = firstCorrectChoice.SelectToken("image.id")?.ToString() ?? string.Empty;

                        if (string.IsNullOrWhiteSpace(questionAnswer))
                        {
                            ActivityLogger.Log(_currentSection, subSection, $"Failed to fetch answer for question type 'quiz'. (answer and imageId is null or whitespace)");
                            ActivityLogger.Log(_currentSection, subSection, $"QuestionData: {questionData?.ToString(Formatting.None)}", true);

                            questionAnswers.Add(new string(' ', prefix.Length) + "\u001b[91mFailed to fetch questions answer.\u001b[97m");
                            answerAdded = true;
                            break;
                        }
                    }

#pragma warning restore CS8604 // Possible null reference argument.

                    if (needToUseImageId)
                    {
                        questionAnswers.Add(new string(' ', prefix.Length) + $"\u001b[97mCorrect picture is:");
                        questionAnswers.Add(new string(' ', prefix.Length) + $"\u001b[93m'{_appUrls.kahootImageCdn.Replace("{imageId}", questionAnswer)}'\u001b[97m");
                    }
                    else
                    {
                        questionAnswers.Add(new string(' ', prefix.Length) + $"\u001b[92m{questionAnswer}\u001b[97m");
                    }

                    answerAdded = true;
                    break;

                case "jumble":

                    JArray jumbleChoices = questionData.SelectToken("choices") as JArray ?? [];

                    questionAnswers.Add(new string(' ', prefix.Length) + "Order from top to bottom:");

                    for (int i = 0; i < jumbleChoices.Count; i++)
                    {
                        if (i == 0)
                        {
                            questionAnswers.Add(new string(' ', prefix.Length) + $"\u001b[97m┌{new string('─', i)}> \u001b[92m{jumbleChoices[i]["answer"]}\u001b[97m");
                            continue;
                        }

                        if (i + 1 == jumbleChoices.Count)
                        {
                            questionAnswers.Add(new string(' ', prefix.Length) + $"\u001b[97m└{new string('─', i)}> \u001b[92m{jumbleChoices[i]["answer"]}\u001b[97m");
                            continue;
                        }

                        questionAnswers.Add(new string(' ', prefix.Length) + $"\u001b[97m├{new string('─', i)}> \u001b[92m{jumbleChoices[i]["answer"]}\u001b[97m");
                    }

                    answerAdded = true;
                    break;

                case "pin_it":

                    JObject choiceShapes = questionData["choiceShapes"] as JObject ?? [];
                    JObject imageMetadata = questionData["imageMetadata"] as JObject ?? [];

                    if (choiceShapes == null || choiceShapes == new JObject() || imageMetadata == null || imageMetadata == new JObject())
                    {
                        ActivityLogger.Log(_currentSection, subSection, $"Failed to fetch 'choiceShapes' or 'imageMetadata' for type 'pin_it'. (one or multiple are null)");
                        ActivityLogger.Log(_currentSection, subSection, $"QuestionData: {questionData?.ToString(Formatting.None)}", true);

                        questionAnswers.Add(new string(' ', prefix.Length) + "\u001b[91mFailed to fetch questions answer.\u001b[97m");
                        break;
                    }

                    Point imageDimension;
                    Point waldoDimensionLocation;
                    Point waldoDimensionTolerance;

                    try
                    {
                        JToken? choiceShapesX = choiceShapes.SelectToken("x");
                        JToken? choiceShapesY = choiceShapes.SelectToken("y");
                        JToken? choiceShapesWidth = choiceShapes.SelectToken("width");
                        JToken? choiceShapesHeight = choiceShapes.SelectToken("height");
                        JToken? imageWidth = imageMetadata.SelectToken("width");
                        JToken? imageHeight = imageMetadata.SelectToken("height");

                        imageDimension = new Point(Convert.ToInt32(imageWidth), Convert.ToInt32(imageHeight));
                        waldoDimensionLocation = new Point(Convert.ToInt32(choiceShapesX), Convert.ToInt32(choiceShapesY));
                        waldoDimensionTolerance = new Point(Convert.ToInt32(choiceShapesWidth), Convert.ToInt32(choiceShapesHeight));
                    }
                    catch (Exception exception)
                    {
                        ActivityLogger.Log(_currentSection, subSection, $"Failed to create waldo points for question type 'pin_it'.");
                        ActivityLogger.Log(_currentSection, subSection, $"Exception: {exception.Message}", true);
                        ActivityLogger.Log(_currentSection, subSection, $"QuestionData: {questionData?.ToString(Formatting.None)}", true);
                        ActivityLogger.Log(_currentSection, subSection, $"ChoiceShapes: {choiceShapes?.ToString(Formatting.None)}", true);
                        ActivityLogger.Log(_currentSection, subSection, $"ImageMetadata: {imageMetadata?.ToString(Formatting.None)}", true);

                        questionAnswers.Add(new string(' ', prefix.Length) + "\u001b[91mFailed to fetch questions answer.\u001b[97m");
                        break;
                    }

                    string approximateLocation = DetermineApproximateWaldoLocation(imageDimension, waldoDimensionLocation, waldoDimensionTolerance);

                    questionAnswers.Add(new string(' ', prefix.Length) + $"\u001b[97mThe object in question is in the \u001b[92m'{approximateLocation} area' \u001b[97mof the picture.");

                    answerAdded = true;
                    break;

                case "slider":
                    JObject sliderChoiceRange = questionData["choiceRange"] as JObject ?? [];

                    if (sliderChoiceRange == null || sliderChoiceRange == new JObject())
                    {
                        ActivityLogger.Log(_currentSection, subSection, $"Failed to fetch answer for question type 'slider'. (choiceRange is null)");
                        ActivityLogger.Log(_currentSection, subSection, $"QuestionData: {questionData?.ToString(Formatting.None)}", true);

                        questionAnswers.Add(new string(' ', prefix.Length) + "\u001b[91mFailed to fetch questions answer.\u001b[97m");
                        break;
                    }

                    try
                    {
                        JToken? sliderCorrectValue = sliderChoiceRange.SelectToken("correct");
                        JToken? sliderTolerance = sliderChoiceRange.SelectToken("tolerance");

                        if (sliderCorrectValue == null || sliderTolerance == null)
                        {
                            throw new Exception("Failed to get the sliders correct value or tolerance.");
                        }

                        questionAnswers.Add(new string(' ', prefix.Length) + $"\u001b[97mAdjust the slider to \u001b[92m'{sliderCorrectValue}'\u001b[97m, with a tolerance of {sliderTolerance}.");
                    }
                    catch (Exception exception)
                    {
                        ActivityLogger.Log(_currentSection, subSection, $"Failed to fetch answer for question type 'slider'.");
                        ActivityLogger.Log(_currentSection, subSection, $"Exception: {exception.Message}", true);
                        ActivityLogger.Log(_currentSection, subSection, $"QuestionData: {questionData?.ToString(Formatting.None)}", true);

                        questionAnswers.Add(new string(' ', prefix.Length) + "\u001b[91mFailed to fetch questions answer.\u001b[97m");
                    }

                    answerAdded = true;
                    break;

                default:
                    break;
            }



            string[] typesWithNoAnswers =
            [
                "nps",
                "scale",
                "content",
                "drop_pin",
                "feedback",
                "word_cloud",
                "brainstorming",
            ];

            string[] multipleChoiceTypes =
            [
                "survey",
                "open_ended",
                "multiple_select_poll",
                "multiple_select_quiz",
            ];



            if (answerAdded == false && typesWithNoAnswers.Contains(questionType.ToString().ToLower()))
            {
                questionAnswers.Add(new string(' ', prefix.Length) + "\u001b[93mNo answer is needed for this question.\u001b[97m");

                answerAdded = true;
            }
            else if (answerAdded == false && multipleChoiceTypes.Contains(questionType.ToString().ToLower()))
            {
#pragma warning disable CS8604 // Possible null reference argument.
                List<string?>? correctAnswers = questionData?["choices"]
                    .Where(choice => (bool)choice["correct"])
                    .Select(correctChoice => (string?)correctChoice["answer"])
                    .ToList();
#pragma warning restore CS8604 // Possible null reference argument.

                if (correctAnswers == null || correctAnswers.Count <= 0)
                {
                    ActivityLogger.Log(_currentSection, subSection, $"Failed to fetch answer for question type '{questionType}'. (correctAnswers array is null or empty)");
                    ActivityLogger.Log(_currentSection, subSection, $"QuestionData: {questionData?.ToString(Formatting.None)}", true);

                    questionAnswers.Add(new string(' ', prefix.Length) + "\u001b[91mFailed to fetch questions answer.\u001b[97m");
                }
                else
                {
                    questionAnswers.Add(new string(' ', prefix.Length) + "\u001b[97mCorrect answers:");

                    foreach (string? correctAnswer in correctAnswers)
                    {
                        questionAnswers.Add(new string(' ', prefix.Length) + $"- \u001b[92m'{correctAnswer}'\u001b[97m");
                    }
                }

                answerAdded = true;
            }

            if (answerAdded == false)
            {
                ActivityLogger.Log(_currentSection, subSection, $"Failed to fetch answer for question type '{questionType}'.");
                ActivityLogger.Log(_currentSection, subSection, $"ATTENTION: This question type is not recognized by the application.", true);
                ActivityLogger.Log(_currentSection, subSection, $"QuestionData: {questionData?.ToString(Formatting.None)}", true);

                successfullyFetchedAnswer = false;
            }



            return (successfullyFetchedAnswer, questionType.ToString(), questionTitle.ToString(), questionAnswers);
        }

        private static string DetermineApproximateWaldoLocation(Point imageDimension, Point waldoDimensionLocation, Point waldoDimensionTolerance)
        {
            Point imageCenter = new(imageDimension.X / 2, imageDimension.Y / 2);
            Point waldoCenter = new(waldoDimensionLocation.X + waldoDimensionTolerance.X / 2, waldoDimensionLocation.Y + waldoDimensionTolerance.Y / 2);

            if (imageCenter == waldoCenter)
            {
                return "Center";
            }

            bool sectionLeft = waldoCenter.X < imageCenter.X;
            bool sectionRight = waldoCenter.X > 2 * imageCenter.X;
            bool sectionTop = waldoCenter.Y < imageCenter.Y;
            bool sectionBottom = waldoCenter.Y > 2 * imageCenter.Y;

            StringBuilder stringBuilder = new();

            if (sectionTop)
            {
                stringBuilder.Append("Upper ");
            }
            else if (sectionBottom)
            {
                stringBuilder.Append("Bottom ");
            }

            if (sectionLeft)
            {
                stringBuilder.Append("Left");
            }
            else if (sectionRight)
            {
                stringBuilder.Append("Right");
            }
            else if (sectionTop || sectionBottom)
            {
                stringBuilder.Append("Center");
            }



            if (stringBuilder.Length == 0)
            {
                return "Unknown";
            }

            return stringBuilder.ToString();
        }

        internal static async Task<(bool successfullySaved, List<JObject> quizQuestionsCache)> SafeQuizQuestions(string quizId, string quizData)
        {
            string subSection = "SafeQuizQuestions";

            List<JObject> quizQuestionsCache = [];

            if (string.IsNullOrWhiteSpace(quizData))
            {
                ActivityLogger.Log(_currentSection, subSection, "QuizData is null or whitespace, check previous logs.");

                return (false, []);
            }

            ActivityLogger.Log(_currentSection, subSection, "Received a valid API response (not empty/not whitespaces).");



            JArray quizQuestions;

            try
            {
                quizQuestions = (JArray?)JObject.Parse(quizData)["questions"] ?? [];

                if (quizQuestions == null)
                {
                    throw new Exception("Quiz questions are null/were not parsed correctly.");
                }
            }
            catch (Exception exception)
            {
                ActivityLogger.Log(_currentSection, subSection, "Failed to parse the quizzes questions from the API response.");
                ActivityLogger.Log(_currentSection, subSection, $"API-Response: {quizData}", true);
                ActivityLogger.Log(_currentSection, subSection, $"Exception: {exception.Message}", true);

                return (false, []);
            }

            ActivityLogger.Log(_currentSection, subSection, "Successfully parsed the quizzes data.");



            ActivityLogger.Log(_currentSection, subSection, "Creating/looking for needed folders within the application folder.");

            string quizzesFolder = _appPaths.quizzesFolder;
            string currentQuizFolder = Path.Combine(quizzesFolder, quizId);

            try
            {
                if (Directory.Exists(quizzesFolder) == false)
                {
                    Directory.CreateDirectory(quizzesFolder);
                }

                if (Directory.Exists(currentQuizFolder) == false)
                {
                    Directory.CreateDirectory(currentQuizFolder);
                }
            }
            catch (Exception exception)
            {
                ActivityLogger.Log(_currentSection, subSection, "Failed to create/find necessary folders.");
                ActivityLogger.Log(_currentSection, subSection, $"Exception: {exception.Message}", true);

                return (false, []);
            }

            ActivityLogger.Log(_currentSection, subSection, "All needed folders exists.");



            ActivityLogger.Log(_currentSection, subSection, "Saving all questions with their data to the local folder.");

            for (int i = 1; i <= quizQuestions.Count; i++)
            {
                JObject questionData;

                try
                {
                    questionData = (JObject)quizQuestions[i - 1];

                    if (questionData == null)
                    {
                        throw new Exception("QuizData is null/was not parsed correctly.");
                    }

                    quizQuestionsCache.Add(questionData);
                }
                catch (Exception exception)
                {
                    ActivityLogger.Log(_currentSection, subSection, $"[ERROR] - Failed to parse current question '{i}'.");
                    ActivityLogger.Log(_currentSection, subSection, $"Exception: {exception.Message}", true);
                    ActivityLogger.Log(_currentSection, subSection, "Please look at the error logs to fix this issue.", true);

                    return (false, []);
                }

                string questionDataPath = Path.Combine(currentQuizFolder, $"question{i}.json");

                try
                {
                    await File.WriteAllTextAsync(questionDataPath, questionData.ToString(Formatting.Indented));
                }
                catch (Exception exception)
                {
                    ActivityLogger.Log(_currentSection, subSection, $"Failed to save current questionData for question '{i}' to disk.");
                    ActivityLogger.Log(_currentSection, subSection, $"Exception: {exception.Message}", true);

                    return (false, []);
                }
            }

            return (true, quizQuestionsCache);
        }

        internal static async Task<bool> AnswerQuestionAutoplay(IWebDriver webDriver, List<JObject> quizQuestionsCache, GameAutoplaySettings gameAutoplaySettings)
        {
            string subSection = "AnswerQuestionAutoplay";

            ActivityLogger.Log(_currentSection, subSection, "Answering a new question via the existing webdriver instance.");
            ActivityLogger.Log(_currentSection, subSection, $"QuizQuestionsCache: {quizQuestionsCache}", true);
            ActivityLogger.Log(_currentSection, subSection, $"QuizId: {gameAutoplaySettings.quizId}", true);



            Autoplay.UpdateWebDriverLog($"\u001b[97mWaiting for the question to appear.");

            try
            {
                WebDriverWait webDriverWait = new(webDriver, TimeSpan.FromSeconds(30));
                webDriverWait.Until(driver => driver.Url == _appUrls.kahootGetReady);
            }
            catch (Exception exception)
            {
                ActivityLogger.Log(_currentSection, subSection, "WebDriver wait timed out! Waiting for the question page to appear was unsuccessfull.");
                ActivityLogger.Log(_currentSection, subSection, $"Exception: {exception.Message}", true);

                return false;
            }

            string questionContent = string.Empty;
            int questionIndex = -1;



            ActivityLogger.Log(_currentSection, subSection, "Awaiting a 1.5 second cooldown, to ensure that the page has fully loaded.");

            await Task.Delay(1500);



            ActivityLogger.Log(_currentSection, subSection, "Trying to fetch question content.");

            Autoplay.UpdateWebDriverLog($"\u001b[92mQuestion appeared!.");
            Autoplay.UpdateWebDriverLog($"\u001b[97mTrying to fetch question.");

            while (webDriver.Url.Equals(_appUrls.kahootGetReady) == true)
            {
                try
                {
                    questionContent = webDriver.FindElement(By.XPath(_appDriverPaths.headerXpathQuestionTitle)).Text;

                    ActivityLogger.Log(_currentSection, subSection, "Found question content!");
                    Autoplay.UpdateWebDriverLog($"\u001b[92mFound question content!");
                    break;
                }
                catch
                {
                    await Task.Delay(50);
                }
            }

            if (questionContent.Equals(string.Empty))
            {
                ActivityLogger.Log(_currentSection, subSection, "Failed to fetch question, searching question index.");
                Autoplay.UpdateWebDriverLog($"\u001b[93mFailed to fetch question, searching question index.");

                try
                {
                    string questionIndexHtml = webDriver.FindElement(By.XPath(_appDriverPaths.divXpathQuestionIndex)).Text;
                    questionIndex = Convert.ToInt32(questionIndexHtml);

                    ActivityLogger.Log(_currentSection, subSection, "Found question index!");
                    Autoplay.UpdateWebDriverLog($"\u001b[92mFound question index!");
                }
                catch (Exception exception)
                {
                    ActivityLogger.Log(_currentSection, subSection, "Failed to fetch question content and index.");
                    ActivityLogger.Log(_currentSection, subSection, "Due to this, the question can not be answered reliably.");
                    ActivityLogger.Log(_currentSection, subSection, $"Exception: {exception.Message}", true);

                    return false;
                }
            }



            ActivityLogger.Log(_currentSection, subSection, "Successfully fetched at least one question variables.");
            ActivityLogger.Log(_currentSection, subSection, $"questionContent: '{questionContent}'", true);
            ActivityLogger.Log(_currentSection, subSection, $"questionIndex: '{questionIndex}'", true);



            JObject questionData;

            if (questionIndex != -1)
            {
                ActivityLogger.Log(_currentSection, subSection, "Fetching question data via questionIndex.");
                questionData = quizQuestionsCache[questionIndex - 1];
            }
            else
            {
                ActivityLogger.Log(_currentSection, subSection, "Fetching question data via questionContent.");
                questionData = FindQuestionByQuestionTitle(questionContent, quizQuestionsCache);
            }



            ActivityLogger.Log(_currentSection, subSection, "Waiting for the answer page to appear.");
            Autoplay.UpdateWebDriverLog($"\u001b[97mWaiting for the answer page to appear.");

            try
            {
                WebDriverWait webDriverWait = new(webDriver, TimeSpan.FromSeconds(30));
                webDriverWait.Until(driver => driver.Url == _appUrls.kahootGameBlock);
            }
            catch (Exception exception)
            {
                ActivityLogger.Log(_currentSection, subSection, "WebDriver wait timed out! Waiting for the answer page to appear was unsuccessfull.");
                ActivityLogger.Log(_currentSection, subSection, $"Exception: {exception.Message}", true);

                return false;
            }



            ActivityLogger.Log(_currentSection, subSection, "Answer page appeared, submitting answer.");
            
            bool submittedAnswer = SubmitAnswer(webDriver, questionData);

            ActivityLogger.Log(_currentSection, subSection, $"Answer was submitted, returning result '{submittedAnswer}'.");



            return submittedAnswer;
        }

        private static JObject FindQuestionByQuestionTitle(string titleToFind, List<JObject> quizQuestionsCache)
        {
            string subSection = "FindQuestionByQuestionTitle";

            foreach (JObject questionData in quizQuestionsCache)
            {
                string questionType = questionData["type"]?.ToString() ?? string.Empty;

                if (questionType.Equals(string.Empty))
                {
                    continue;
                }

                if (questionType.Equals("content") && questionData["title"]?.ToString().Equals(titleToFind) == true)
                {
                    return questionData;
                }

                if (questionType.Equals("content") == false && questionData["question"]?.ToString().Equals(titleToFind) == true)
                {
                    return questionData;
                }
            }

            ActivityLogger.Log(_currentSection, subSection, "Failed to find questionData with the provided information.");
            ActivityLogger.Log(_currentSection, subSection, "Returning an empty list.", true);

            return [];
        }

        private static bool SubmitAnswer(IWebDriver webDriver, JObject questionData)
        {
            return true;
    }
}
}