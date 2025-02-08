using System.Text;





namespace Kahoofection.Scripts
{
    internal class WebConnection
    {
        private const string _currentSection = "WebConnection";



        internal static string CreateRequest(string targetUrl, Dictionary<string, string>? requestParameters = null)
        {
            ActivityLogger.Log(_currentSection, $"Initiating a new web request.");
            ActivityLogger.Log(_currentSection, $"Target url: {targetUrl}");

            try
            {
                if (requestParameters != null)
                {
                    StringBuilder sb = new(targetUrl);

                    foreach (var currentParameter in requestParameters)
                    {
                        sb.Append($"&{currentParameter.Key}={currentParameter.Value}");
                    }

                    targetUrl = sb.ToString();
                    ActivityLogger.Log(_currentSection, targetUrl);

                }

                HttpClient httpClient = new();

                HttpResponseMessage responseMessage = httpClient.GetAsync(targetUrl).Result;
                string response = new StreamReader(responseMessage.Content.ReadAsStreamAsync().Result).ReadToEnd();

                ActivityLogger.Log(_currentSection, $"Web request was a success, returning response.");

                return response;
            }
            catch (Exception exception)
            {
                ActivityLogger.Log(_currentSection, $"An error occured while performing the web request.");

                if (requestParameters != null)
                {
                    ActivityLogger.Log(_currentSection, $"Provided parameters:", true);

                    foreach (var currentParameter in requestParameters)
                    {
                        ActivityLogger.Log(_currentSection, $"{currentParameter.Key} -> {currentParameter.Value}", true);
                    }
                }
                else
                {
                    ActivityLogger.Log(_currentSection, $"No parameters were provided for the request or they are null.", true);
                }

                ActivityLogger.Log(_currentSection, $"Target url: {exception.Message}", true);
                ActivityLogger.Log(_currentSection, $"Exception thrown: {exception.Message}", true);

                return string.Empty;
            }
        }
    }
}