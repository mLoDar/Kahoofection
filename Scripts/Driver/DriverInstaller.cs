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

        internal static async Task<bool> DownloadGeckoDriver()
        {
            string subSection = "DownloadGeckoDriver";

            try
            {
                string? firefoxPath = DriverHelper.GetBrowserPath(SupportedBrowser.Firefox);

                if (string.IsNullOrEmpty(firefoxPath) || !File.Exists(firefoxPath))
                {
                    throw new Exception("Firefox's file path was not found, as it might not be installed.");
                }



                string? firefoxVersion = DriverHelper.GetFileVersion(firefoxPath);

                if (string.IsNullOrEmpty(firefoxVersion))
                {
                    throw new Exception("No valid file version of Firefox was found.");
                }



                string? latestGeckoVersion = await DriverHelper.GetLatestGeckoVersion();

                if (string.IsNullOrEmpty(latestGeckoVersion))
                {
                    throw new Exception("Could not determine the latest GeckoDriver version.");
                }



                string baseUrl = _appUrls.geckoDriverDownload;
                string requestUrl = baseUrl.Replace("{geckoVersion}", latestGeckoVersion);

                string driversFolder = _appPaths.driversFolder;
                string localSavePath = Path.Combine(driversFolder, $"geckodriver-{latestGeckoVersion}.zip");



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
                    throw new Exception($"Could not create needed folders. {exception.Message}");
                }



                Exception? downloadError = await WebConnection.DownloadFile(requestUrl, localSavePath);

                if (downloadError != null)
                {
                    throw new Exception($"Failed to download driver. {downloadError.Message}");
                }



                try
                {
                    ZipFile.ExtractToDirectory(localSavePath, driversFolder, true);
                }
                catch (Exception exception)
                {
                    throw new Exception($"Could not extract the downloaded file into the selected folder. {exception.Message}");
                }

                return true;
            }
            catch (Exception exception)
            {
                ActivityLogger.Log(_currentSection, subSection, "Failed to download a geckodriver.");
                ActivityLogger.Log(_currentSection, subSection, $"Exception: {exception.Message}", true);

                return false;
            }
        }
    }
}