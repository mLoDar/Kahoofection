using System.Net;
using System.Text;
using System.Drawing;

using Kahoofection.Ressources;





namespace Kahoofection.Scripts
{
    internal class WebConnection
    {
        private const string _currentSection = "WebConnection";



        internal static async Task<string> CreateRequest(string requestUrl, Dictionary<string, string>? requestParameters = null)
        {
            string subSection = "CreateRequest";

            ActivityLogger.Log(_currentSection, subSection, $"Initiating a new web request.");
            ActivityLogger.Log(_currentSection, subSection, $"Request url: {requestUrl}", true);

            try
            {
                if (requestParameters != null)
                {
                    StringBuilder sb = new(requestUrl);

                    foreach (var currentParameter in requestParameters)
                    {
                        sb.Append($"&{currentParameter.Key}={currentParameter.Value}");
                    }

                    requestUrl = sb.ToString();
                }

                HttpClient httpClient = new();

                HttpResponseMessage responseMessage = await httpClient.GetAsync(requestUrl);
                string response = new StreamReader(await responseMessage.Content.ReadAsStreamAsync()).ReadToEnd();

                response = RegexPatterns.HtmlTags().Replace(response, string.Empty);
                response = WebUtility.HtmlDecode(response);

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

                ActivityLogger.Log(_currentSection, subSection, $"Exception thrown: {exception.Message}", true);

                return string.Empty;
            }
        }

        internal static async Task<Exception?> DownloadFile(string requestUrl, string localSavePath, string filePurpose, bool suppressProgressMessage = false)
        {
            string subSection = "DownloadFile";

            ActivityLogger.Log(_currentSection, subSection, $"Initiating a new file download.");
            ActivityLogger.Log(_currentSection, subSection, $"Request url: {requestUrl}", true);
            ActivityLogger.Log(_currentSection, subSection, $"Local save path: {localSavePath}", true);



            Console.CursorVisible = false;

            try
            {
                HttpClient httpClient = new();

                using HttpResponseMessage httpResponse = await httpClient.GetAsync(requestUrl, HttpCompletionOption.ResponseHeadersRead);
                httpResponse.EnsureSuccessStatusCode();

                ActivityLogger.Log(_currentSection, subSection, $"Received a success status code from the request url.");

                ConsoleHelper.ResetConsole();

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"             Downloading '{filePurpose}'");



                long? totalBytesToDownload = httpResponse.Content.Headers.ContentLength;
                
                if (totalBytesToDownload == null)
                {
                    Console.WriteLine("             This process may take a moment.");

                    suppressProgressMessage = true;
                }

                

                using FileStream fileStream = new(localSavePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
                
                ActivityLogger.Log(_currentSection, subSection, $"Opened a new file stream for the local save path which was provided.");



                byte[] readBuffer = new byte[8192];
                long totalReadProgress = 0;
                int currentReadProgress;


                await using Stream contentStream = await httpResponse.Content.ReadAsStreamAsync();

                while ((currentReadProgress = await contentStream.ReadAsync(readBuffer)) > 0)
                {
                    await fileStream.WriteAsync(readBuffer.AsMemory(0, currentReadProgress));

                    if (suppressProgressMessage == true)
                    {
                        continue;
                    }

                    totalReadProgress += currentReadProgress;

                    if (totalBytesToDownload.HasValue)
                    {
                        double currentDownloadMb = totalReadProgress / 1024.0 / 1024.0;
                        double totalDownloadMb = totalBytesToDownload.Value / 1024.0 / 1024.0;
                        int progressInPercent = (int)(totalReadProgress * 100 / totalBytesToDownload.Value);

                        Console.Write($"\r             Progress: {currentDownloadMb:F2}MB / {totalDownloadMb:F2}MB ({progressInPercent}%)   ");
                    }
                }

                ActivityLogger.Log(_currentSection, subSection, $"File was fully written to disk, disposing any streams.");



                await fileStream.DisposeAsync();
                await contentStream.DisposeAsync();

                ActivityLogger.Log(_currentSection, subSection, $"All streams were disposed, successfully finished the download.");



                Console.CursorVisible = true;

                return null;
            }
            catch (Exception exception)
            {
                ActivityLogger.Log(_currentSection, subSection, $"Failed to download file.");
                ActivityLogger.Log(_currentSection, subSection, exception.Message, true);

                Console.CursorVisible = true;

                return exception;
            }
        }

        internal static async Task<(Size imageDimensions, bool successfullyFetched)> GetImageSizeFromUrl(string imageUrl)
        {
            string subSection = "GetImageSizeFromUrl";

            ActivityLogger.Log(_currentSection, subSection, "Initiating a new web request.");
            ActivityLogger.Log(_currentSection, subSection, $"Request url: {imageUrl}", true);

            try
            {
                using HttpClient httpClient = new();
                httpClient.DefaultRequestHeaders.Range = new System.Net.Http.Headers.RangeHeaderValue(0, 4096);

                using HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(imageUrl, HttpCompletionOption.ResponseHeadersRead);
                httpResponseMessage.EnsureSuccessStatusCode();

                using Stream stream = await httpResponseMessage.Content.ReadAsStreamAsync();
                using MemoryStream memoryStream = new();

                await stream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;



#pragma warning disable CA1416 // Validate platform compatibility

                Image image = Image.FromStream(memoryStream, false, false);
                Size imageDimensions = new(image.Width, image.Height);

#pragma warning restore CA1416 // Validate platform compatibility



                ActivityLogger.Log(_currentSection, subSection, "Successfully fetched image dimensions.");
                ActivityLogger.Log(_currentSection, subSection, $"Returning: {imageDimensions.Width}px width | {imageDimensions.Height}px height", true);

                return (imageDimensions, true);
            }
            catch (Exception exception)
            {
                ActivityLogger.Log(_currentSection, subSection, "An error occurred while performing the web request.");
                ActivityLogger.Log(_currentSection, subSection, $"Exception thrown: {exception.Message}", true);
                
                return (new Size(-1, -1), false);
            }
        }
    }
}