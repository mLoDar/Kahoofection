using System.Text;





namespace Kahoofection.Scripts
{
    internal class WebConnection
    {
        private const string _currentSection = "WebConnection";



        internal static async Task<string> CreateRequest(string targetUrl, Dictionary<string, string>? requestParameters = null)
        {
            string subSection = "CreateRequest";

            ActivityLogger.Log(_currentSection, subSection, $"Initiating a new web request.");
            ActivityLogger.Log(_currentSection, subSection, $"Target url: {targetUrl}");

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

                }

                HttpClient httpClient = new();

                HttpResponseMessage responseMessage = await httpClient.GetAsync(targetUrl);
                string response = new StreamReader(await responseMessage.Content.ReadAsStreamAsync()).ReadToEnd();

                ActivityLogger.Log(_currentSection, subSection, $"Web request was a success, returning response.");

                return response;
            }
            catch (Exception exception)
            {
                ActivityLogger.Log(_currentSection, subSection, $"An error occurred while performing the web request.");

                if (requestParameters != null)
                {
                    ActivityLogger.Log(_currentSection, subSection, $"Provided parameters:", true);

                    foreach (var currentParameter in requestParameters)
                    {
                        ActivityLogger.Log(_currentSection, subSection, $"{currentParameter.Key} -> {currentParameter.Value}", true);
                    }
                }
                else
                {
                    ActivityLogger.Log(_currentSection, subSection, $"No parameters were provided for the request or they are null.", true);
                }

                ActivityLogger.Log(_currentSection, subSection, $"Target url: {exception.Message}", true);
                ActivityLogger.Log(_currentSection, subSection, $"Exception thrown: {exception.Message}", true);

                return string.Empty;
            }
        }
    }
}