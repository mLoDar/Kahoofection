using System.IO.Compression;

using Kahoofection.Ressources;





namespace Kahoofection.Scripts.Driver
{
    internal class DriverInstaller
    {
        private const string _currentSection = "DriverInstaller";

        private static readonly ApplicationSettings.Urls _appUrls = new();
        private static readonly ApplicationSettings.Paths _appPaths = new();



        internal static async Task<bool> DownloadChromeDriver()
        {
            string subSection = "DownloadChromeDriver";

            try
            {
                string? chromePath = DriverHelper.GetBrowserPath(SupportedBrowser.Chrome);

                if (string.IsNullOrEmpty(chromePath) || !File.Exists(chromePath))
                {
                    throw new Exception("Chrome's file path was not found, as it might not be installed.");
                }



                string? chromeVersion = DriverHelper.GetFileVersion(chromePath);

                if (string.IsNullOrEmpty(chromeVersion))
                {
                    throw new Exception("No valid file version of Chrome was found.");
                }



                string baseUrl = _appUrls.chromeDriverDownload;;
                string requestUrl = baseUrl.Replace("{chromeVersion}", chromeVersion);

                string driversFolder = _appPaths.driversFolder;
                string localSavePath = Path.Combine(driversFolder, $"chromedriver-{chromeVersion}.zip");



                try
                {
                    if (File.Exists(localSavePath))
                    {
                        File.Delete(localSavePath);
                    }

                    if (!Directory.Exists(driversFolder))
                    {
                        Directory.CreateDirectory(driversFolder);
                    }
                }
                catch (Exception exception)
                {
                    throw new Exception($"Could not create needed folders or delete existing files. {exception.Message}");
                }
                


                Exception? downloadError = await WebConnection.DownloadFile(requestUrl, localSavePath);

                if (downloadError != null)
                {
                    throw downloadError;
                }



                try
                {
                    using ZipArchive driverArchive = ZipFile.OpenRead(localSavePath);

                    foreach (ZipArchiveEntry zipEntry in driverArchive.Entries)
                    {
                        if (zipEntry.FullName.EndsWith('/') == false && zipEntry.FullName.StartsWith("chromedriver-win64/") == true)
                        {
                            string extractedFilePath = Path.Combine(driversFolder, Path.GetFileName(zipEntry.FullName));

                            zipEntry.ExtractToFile(extractedFilePath, true);
                        }
                    }
                }
                catch (Exception exception)
                {
                    throw new Exception($"Could not extract the downloaded file into the selected folder. {exception.Message}");
                }

                return true;
            }
            catch (Exception exception)
            {
                ActivityLogger.Log(_currentSection, subSection, "Failed to download a chromedriver.");
                ActivityLogger.Log(_currentSection, subSection, $"Exception: {exception.Message}", true);

                return false;
            }
        }
    }
}