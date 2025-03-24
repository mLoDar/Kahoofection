using Kahoofection.Ressources;

using OpenQA.Selenium;
using Newtonsoft.Json.Linq;





namespace Kahoofection.Scripts.Kahoot
{
    class AnswerSubmission
    {
        private const string _currentSection = "AnswerSubmission";

        private static readonly ApplicationSettings.DriverPaths _appDriverPaths = new();



        internal static bool Quiz(IWebDriver webDriver, JObject questionData)
        {
            string subSection = "Quiz";



            JArray questionChoices = questionData["choices"] as JArray ?? [];

            int foundAnswerIndex = -1;
            string foundAnswerContent = "";

            for (int index = 0; index < questionChoices.Count; index++)
            {
                JObject choice = questionChoices[index] as JObject ?? [];

                if (Convert.ToBoolean(choice["correct"]) == true)
                {
                    foundAnswerIndex = index;
                    foundAnswerContent = choice["answer"]?.ToString().ToLower() ?? string.Empty;
                    break;
                }
            }

            if (Enumerable.Range(0, 3).Contains(foundAnswerIndex) == false && string.IsNullOrWhiteSpace(foundAnswerContent) == true)
            {
                ActivityLogger.Log(_currentSection, subSection, "Received invalid answer parameters.");
                ActivityLogger.Log(_currentSection, subSection, $"foundAnswerIndex: {foundAnswerIndex}", true);
                ActivityLogger.Log(_currentSection, subSection, $"foundAnswerContent: {foundAnswerContent}", true);

                return false;
            }

            IWebElement buttonToClick;

            try
            {
                IJavaScriptExecutor js = (IJavaScriptExecutor)webDriver;
                buttonToClick = (IWebElement)js.ExecuteScript($@"
                    return [...document.querySelectorAll('button')]
                    .find(button => button.innerText.toLowerCase()
                    .trim() === '{foundAnswerContent}');
                ");
            }
            catch (Exception exceptionButtonSearch)
            {
                ActivityLogger.Log(_currentSection, subSection, "Failed to find button via answer content.");
                ActivityLogger.Log(_currentSection, subSection, $"Exception: {exceptionButtonSearch.Message}", true);

                ActivityLogger.Log(_currentSection, subSection, "Searching button via answer index.");

                string buttonXpathQuizChoice = _appDriverPaths.buttonXpathQuizChoiceAnswerNotDisplayed;

                string row = ((foundAnswerIndex / 2) + 1).ToString();
                string column = ((foundAnswerIndex % 2) + 1).ToString();

                string finalPath = buttonXpathQuizChoice.Replace("{row}", row).Replace("{column}", column);

                try
                {
                    buttonToClick = webDriver.FindElement(By.XPath(finalPath));
                }
                catch (Exception exception)
                {
                    ActivityLogger.Log(_currentSection, subSection, "Failed to find buttons by XPATH!");
                    ActivityLogger.Log(_currentSection, subSection, $"Exception: {exception.Message}", true);

                    return false;
                }
            }

            try
            {
                buttonToClick.Click();
            }
            catch (Exception exception)
            {
                ActivityLogger.Log(_currentSection, subSection, "Failed to click selected button!");
                ActivityLogger.Log(_currentSection, subSection, $"Exception: {exception.Message}");

                return false;
            }

            return true;
        }
    }
}