using System.Text;
using System.Drawing;

using Kahoofection.Ressources;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;





namespace Kahoofection.Scripts.Kahoot
{
    internal class KahootHelper
    {
        private const string _currentSection = "KahootHelper";

        private static readonly ApplicationSettings.Urls _appUrls = new();



        internal static (bool successfullyFetchedAnswer, string fetchedType, string fetchedTitle, List<string> fetchedAnswers) GetQuestionsAnswer(int questionIndex, JObject questionData)
        {
            JToken? questionType = questionData.SelectToken("type");
            JToken? questionTitle = questionData.SelectToken("question");



            if (questionType == null)
            {
                ActivityLogger.Log(_currentSection, $"Failed to fetch the question type.");
                ActivityLogger.Log(_currentSection, $"QuestionData: {questionData.ToString(Formatting.None)}", true);

                return (false, string.Empty, string.Empty, []);
            }

            if (questionTitle == null && questionType.ToString().Equals("content") == false)
            {
                ActivityLogger.Log(_currentSection, $"Failed to fetch the questions title. (Questions type is not 'content')");
                ActivityLogger.Log(_currentSection, $"QuestionData: {questionData.ToString(Formatting.None)}", true);

                return (false, string.Empty, string.Empty, []);
            }

            if (questionType.ToString().Equals("content"))
            {
                questionTitle = questionData.SelectToken("title") ?? "-";
            }

            if (questionTitle == null)
            {
                ActivityLogger.Log(_currentSection, $"The questions title is null although it was already fetched.");
                ActivityLogger.Log(_currentSection, $"QuestionData: {questionData.ToString(Formatting.None)}", true);

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
                        ActivityLogger.Log(_currentSection, $"Failed to find a correct choice for question type 'quiz'. (firstCorrectChoice is null)");
                        ActivityLogger.Log(_currentSection, $"QuestionData: {questionData?.ToString(Formatting.None)}", true);

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
                            ActivityLogger.Log(_currentSection, $"Failed to fetch answer for question type 'quiz'. (answer and imageId is null or whitespace)");
                            ActivityLogger.Log(_currentSection, $"QuestionData: {questionData?.ToString(Formatting.None)}", true);

                            questionAnswers.Add(new string(' ', prefix.Length) + "\u001b[91mFailed to fetch questions answer.\u001b[97m");
                            answerAdded = true;
                            break;
                        }
                    }

#pragma warning restore CS8604 // Possible null reference argument.

                    if (needToUseImageId)
                    {
                        questionAnswers.Add(new string(' ', prefix.Length) + $"\u001b[97mCorrect picture is:");
                        questionAnswers.Add(new string(' ', prefix.Length) + $"\u001b[93m'{_appUrls.kahootImageCdn}{questionAnswer}'\u001b[97m");
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
                        ActivityLogger.Log(_currentSection, $"Failed to fetch 'choiceShapes' or 'imageMetadata' for type 'pin_it'. (one or multiple are null)");
                        ActivityLogger.Log(_currentSection, $"QuestionData: {questionData?.ToString(Formatting.None)}", true);

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
                        ActivityLogger.Log(_currentSection, $"Failed to create waldo points for question type 'pin_it'.");
                        ActivityLogger.Log(_currentSection, $"Exception: {exception.Message}", true);
                        ActivityLogger.Log(_currentSection, $"QuestionData: {questionData?.ToString(Formatting.None)}", true);
                        ActivityLogger.Log(_currentSection, $"ChoiceShapes: {choiceShapes?.ToString(Formatting.None)}", true);
                        ActivityLogger.Log(_currentSection, $"ImageMetadata: {imageMetadata?.ToString(Formatting.None)}", true);

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
                        ActivityLogger.Log(_currentSection, $"Failed to fetch answer for question type 'slider'. (choiceRange is null)");
                        ActivityLogger.Log(_currentSection, $"QuestionData: {questionData?.ToString(Formatting.None)}", true);

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
                        ActivityLogger.Log(_currentSection, $"Failed to fetch answer for question type 'slider'.");
                        ActivityLogger.Log(_currentSection, $"Exception: {exception.Message}", true);
                        ActivityLogger.Log(_currentSection, $"QuestionData: {questionData?.ToString(Formatting.None)}", true);

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
                    ActivityLogger.Log(_currentSection, $"Failed to fetch answer for question type '{questionType}'. (correctAnswers array is null or empty)");
                    ActivityLogger.Log(_currentSection, $"QuestionData: {questionData?.ToString(Formatting.None)}", true);

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
                ActivityLogger.Log(_currentSection, $"Failed to fetch answer for question type '{questionType}'.");
                ActivityLogger.Log(_currentSection, $"ATTENTION: This question type is not recognized by the application.", true);
                ActivityLogger.Log(_currentSection, $"QuestionData: {questionData?.ToString(Formatting.None)}", true);

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
    }
}