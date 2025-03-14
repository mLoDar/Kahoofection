namespace Kahoofection.Ressources
{
    internal class ApplicationSettings
    {
        internal class Runtime
        {
            internal readonly string appVersion = "v1.0.0";
            internal readonly string appRelease = "TBA";

            internal readonly string kahootGamePinFormat = "0000000";
        }

        internal class Paths
        {
            internal readonly string appDataFolder;
            internal readonly string clientFolder;
            internal readonly string logsFolder;
            internal readonly string driversFolder;
            internal readonly string quizzesFolder;



            internal Paths()
            {
                appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                clientFolder = Path.Combine(appDataFolder, "Kahoofection");
                logsFolder = Path.Combine(clientFolder, "RuntimeLogs");
                driversFolder = Path.Combine(clientFolder, "Webdrivers");
                quizzesFolder = Path.Combine(clientFolder, "Quizzes");
            }
        }

        internal class Urls
        {
            internal readonly string kahootQuizSearch = "https://create.kahoot.it/rest/kahoots/?";
            internal readonly string kahootCheckQuizId = "https://play.kahoot.it/rest/kahoots/{quizId}";
            internal readonly string kahootImageCdn = "https://images-cdn.kahoot.it/{imageId}";
            internal readonly string kahootWebsocket = "wss://kahoot.it/cometd/{gamePin}/{webSocketToken}";
            internal readonly string kahootSessionReservation = "https://kahoot.it/reserve/session/{gamePin}/?{millisTimestamp}";
            internal readonly string kahootJoinPin = "https://kahoot.it?pin={gamePin}";
            internal readonly string kahootJoinNamerator = "https://kahoot.it/namerator";
            internal readonly string kahootJoinEnterName = "https://kahoot.it/join";
            internal readonly string kahootLobby = "https://kahoot.it/instructions";
            internal readonly string kahootGameStarted = "https://kahoot.it/start";
            internal readonly string kahootGetReady = "https://kahoot.it/getready";
            internal readonly string kahootGameBlock = "https://kahoot.it/gameblock";

            internal readonly string geckoDriverReleases = "https://github.com/mozilla/geckodriver/releases/";
            internal readonly string geckoDriverDownload = "https://github.com/mozilla/geckodriver/releases/download/v{geckoVersion}/geckodriver-v{geckoVersion}-win64.zip";

            internal readonly string chromeDriverDownload = "https://storage.googleapis.com/chrome-for-testing-public/{chromeVersion}/win64/chromedriver-win64.zip";
        }

        internal class DriverPaths
        {
            internal readonly string inputIdNickname = "nickname";

            internal readonly string buttonXpathNameratorSpin = "/html/body/div/div[1]/div/div/div/div[3]/div/div/div[2]/button";
            internal readonly string buttonXpathNameratorConfirm = "/html/body/div/div[1]/div/div/div/div[3]/div/div/div[2]/button[2]";

            internal readonly string divXpathQuestionType = "/html/body/div/div[1]/div/div/div/div[2]/div[2]/div";
            internal readonly string divXpathQuestionIndex = "/html/body/div/div[1]/div/div/div/div[2]/div[1]/div";
            internal readonly string spanXpathQuestionTitle = "/html/body/div/div[1]/div/div/main/div[3]/form/div[1]/div[2]/div/span";
        }
    }
}