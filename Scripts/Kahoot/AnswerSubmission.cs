using Kahoofection.Ressources;

using OpenQA.Selenium;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium.Support.UI;





namespace Kahoofection.Scripts.Kahoot
{
    internal struct PinItAnswer
    {
        internal int xCoordinate;
        internal int yCoordinate;
        internal int width;
        internal int height;
    }



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

                if (buttonToClick == null)
                {
                    throw new Exception("No buttons found where the inner text equals the correct answer.");
                }
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

            ActivityLogger.Log(_currentSection, subSection, "Submitted answer.");

            return true;
        }

        internal static bool PinIt(IWebDriver webDriver, JObject questionData)
        {
            string subSection = "PinIt";



            PinItAnswer pinItRect;
            JArray choiceShapes = questionData["choiceShapes"] as JArray ?? [];
            JObject firstChoiceShape = choiceShapes[0] as JObject ?? [];

            try
            {
                pinItRect = new()
                {
                    width = Convert.ToInt32(firstChoiceShape["width"]),
                    height = Convert.ToInt32(firstChoiceShape["height"]),
                    xCoordinate = Convert.ToInt32(firstChoiceShape["x"]),
                    yCoordinate = Convert.ToInt32(firstChoiceShape["y"])
                };
            }
            catch (Exception exception)
            {
                ActivityLogger.Log(_currentSection, subSection, "Failed to define the answer rect for the question.");
                ActivityLogger.Log(_currentSection, subSection, $"Exception: {exception.Message}", true);

                return false;
            }



            string svgCssSelectorPinItImage = _appDriverPaths.svgCssSelectorPinItImage;

            try
            {
                WebDriverWait webDriverWait = new(webDriver, TimeSpan.FromSeconds(5));
                IJavaScriptExecutor javaScriptExecutor = (IJavaScriptExecutor)webDriver;

                IWebElement pinItViewBox = webDriverWait.Until(driver => driver.FindElement(By.CssSelector(svgCssSelectorPinItImage)));
                IWebElement pinItImage = pinItViewBox.FindElement(By.TagName("image"));

                ActivityLogger.Log(_currentSection, subSection, "Found the needed viewbox with its image element.");



                int imageWidth = Convert.ToInt32(javaScriptExecutor.ExecuteScript("return arguments[0].width.baseVal.value;", pinItImage));
                int imageHeight = Convert.ToInt32(javaScriptExecutor.ExecuteScript("return arguments[0].height.baseVal.value;", pinItImage));

                int centerX = pinItRect.xCoordinate + pinItRect.width / 2;
                int centerY = pinItRect.yCoordinate + pinItRect.height / 2;

                int viewBoxWidth = (int)Math.Max(pinItRect.width * 1.5, imageWidth / 3);
                int viewBoxHeight = (int)Math.Max(pinItRect.height * 1.5, imageHeight / 3);

                viewBoxWidth = Math.Min(viewBoxWidth, imageWidth);
                viewBoxHeight = Math.Min(viewBoxHeight, imageHeight);

                ActivityLogger.Log(_currentSection, subSection, "Assigned values for the target rect and viewbox size.");



                string scriptAdjustment = $"arguments[0].setAttribute('viewBox', '{centerX - viewBoxWidth / 2} {centerY - viewBoxHeight / 2} {viewBoxWidth} {viewBoxHeight}');";
                javaScriptExecutor.ExecuteScript(scriptAdjustment, pinItViewBox);

                ActivityLogger.Log(_currentSection, subSection, "Successfully adjusted the viewbox!");
            }
            catch (Exception exception)
            {
                ActivityLogger.Log(_currentSection, subSection, "Failed to adjust the viewbox.");
                ActivityLogger.Log(_currentSection, subSection, $"Exception: {exception.Message}");

                return false;
            }



            try
            {
                IWebElement submitButton = webDriver.FindElements(By.TagName("button")).FirstOrDefault()
                    ?? throw new Exception("Clicking failed, as no buttons were found.");

                submitButton.Click();
            }
            catch (Exception exception)
            {
                ActivityLogger.Log(_currentSection, subSection, "Failed to submit moved viewbox via button!");
                ActivityLogger.Log(_currentSection, subSection, $"Exception: {exception.Message}");

                return false;
            }

            ActivityLogger.Log(_currentSection, subSection, "Submitted answer.");

            return true;
        }

        internal static bool Slider(IWebDriver webDriver, JObject questionData)
        {
            string subSection = "Slider";



            double sliderStep = -1;
            double sliderCorrect = -1;

            JObject choiceRange = questionData["choiceRange"] as JObject ?? [];

            try
            {
                sliderStep = Convert.ToDouble(choiceRange["step"]);
                sliderCorrect = Convert.ToDouble(choiceRange["correct"]);
            }
            catch (Exception exception)
            {
                ActivityLogger.Log(_currentSection, subSection, "Failed to get the slider step or correct value.");
                ActivityLogger.Log(_currentSection, subSection, $"Values: sliderStep-{sliderStep} | sliderCorrect-{sliderCorrect}", true);
                ActivityLogger.Log(_currentSection, subSection, $"Exception: {exception.Message}", true);

                return false;
            }



            try
            {
                string spanXpathCurrentSliderValue = _appDriverPaths.spanCssCurrentSliderValue;
                IWebElement sliderValue = webDriver.FindElement(By.CssSelector(spanXpathCurrentSliderValue));
                
                IWebElement slider = webDriver.FindElements(By.TagName("input")).FirstOrDefault()
                    ?? throw new Exception("Slider adjustment failed, as no input field/slider was found.");
                
                double startValue = double.Parse(sliderValue.Text);

                double difference = sliderCorrect - startValue;
                int neededKeyPresses = (int)Math.Abs(difference / sliderStep);

                for (int i = 0; i < neededKeyPresses; i++)
                {
                    if (difference > 0)
                    {
                        slider.SendKeys(Keys.ArrowRight);
                        continue;
                    }

                    slider.SendKeys(Keys.ArrowLeft);
                }
            }
            catch (Exception exception)
            {
                ActivityLogger.Log(_currentSection, subSection, "Failed to adjust the slider.");
                ActivityLogger.Log(_currentSection, subSection, $"Exception: {exception.Message}", true);

                return false;
            }

            try
            {
                IWebElement submitButton = webDriver.FindElements(By.TagName("button")).FirstOrDefault()
                    ?? throw new Exception("Clicking failed, as no buttons were found.");

                submitButton.Click();
            }
            catch (Exception exception)
            {
                ActivityLogger.Log(_currentSection, subSection, "Failed to submit the adjusted slider.");
                ActivityLogger.Log(_currentSection, subSection, $"Exception: {exception.Message}", true);

                return false;
            }

            ActivityLogger.Log(_currentSection, subSection, "Submitted answer.");

            return true;
        }
    }
}