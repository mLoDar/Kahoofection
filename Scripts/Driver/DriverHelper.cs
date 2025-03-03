using System.Diagnostics;
using System.Text.RegularExpressions;

using Kahoofection.Ressources;

using Microsoft.Win32;



#pragma warning disable CA1416 // Validate platform compatibility





namespace Kahoofection.Scripts.Driver
{
    enum SupportedBrowser
    {
        Firefox,
        Chrome,
    }



    internal class DriverHelper
    {
        private const string _currentSection = "DriverHelper";

        private static readonly ApplicationSettings.Urls _appUrls = new();



        internal static string? GetBrowserPath(SupportedBrowser browser)
        {
            string subSection = "GetBrowserPath";

            string registryKey;

            switch (browser)
            {
                case SupportedBrowser.Firefox:
                    registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\firefox.exe";
                    break;

                case SupportedBrowser.Chrome:
                    registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe";
                    break;

                default:
                    ActivityLogger.Log(_currentSection, subSection, "Received an invalid browser as input, returning an empty string.");
                    ActivityLogger.Log(_currentSection, subSection, $"Input: {browser}");
                    return string.Empty;
            }

            using RegistryKey? key = Registry.LocalMachine.OpenSubKey(registryKey);

            return Convert.ToString(key?.GetValue(null));
        }

        internal static string? GetFileVersion(string filePath)
        {
            string subSection = "GetFileVersion";

            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(filePath);

            if (versionInfo == null)
            {
                ActivityLogger.Log(_currentSection, subSection, "Failed to get a file version for the provided input, returning an empty string.");
                ActivityLogger.Log(_currentSection, subSection, $"Input: {filePath}");

                return string.Empty;
            }

            return versionInfo.FileVersion;
        }

        internal static async Task<string> GetLatestGeckoVersion()
        {
            string subSection = "GetLatestGeckoVersion";

            ActivityLogger.Log(_currentSection, subSection, "Fetching latest gecko version.");

            try
            {
                string requestUrl = _appUrls.geckoDriverReleases;
                string apiResponse = await WebConnection.CreateRequest(requestUrl);

                ActivityLogger.Log(_currentSection, subSection, "Received a response from the API.");



                MatchCollection matches = RegexPatterns.GeckoVersion().Matches(apiResponse);

                if (matches.Count <= 0)
                {
                    throw new Exception("Could not match any text for gecko versions.");
                }

                ActivityLogger.Log(_currentSection, subSection, $"Found a total of '{matches.Count}' matches for a version.");



                string latestGeckoVersion = matches.Cast<Match>().Select(match => match.Groups[1].Value).Distinct().OrderByDescending(version => version).First();

                if (string.IsNullOrEmpty(latestGeckoVersion))
                {
                    throw new Exception("Could not find the latest gecko version while searching all matches.");
                }

                ActivityLogger.Log(_currentSection, subSection, $"Returning '{latestGeckoVersion}' as the latest gecko version.");

                return (latestGeckoVersion);
            }
            catch (Exception exception)
            {
                ActivityLogger.Log(_currentSection, subSection, $"Failed to fetch the latest gecko version.");
                ActivityLogger.Log(_currentSection, subSection, $"Exception: {exception.Message}", true);

                return (string.Empty);
            }
        }
    }
}