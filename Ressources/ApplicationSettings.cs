namespace Kahoofection.Ressources
{
    internal class ApplicationSettings
    {
        internal class Runtime
        {
            internal readonly string appVersion = "v1.0.0";
            internal readonly string appRelease = "TBA";

            internal readonly string gamePinFormat = "0000000";
        }

        internal class Paths
        {
            internal readonly string appDataFolder;
            internal readonly string clientFolder;
            internal readonly string logsFolder;



            internal Paths()
            {
                appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                clientFolder = Path.Combine(appDataFolder, "Kahoofection");
                logsFolder = Path.Combine(clientFolder, "RuntimeLogs");
            }
        }

        internal class Urls
        {
            internal readonly string kahootQuizSearch = "https://create.kahoot.it/rest/kahoots/?";
            internal readonly string kahootCheckQuizId = "https://play.kahoot.it/rest/kahoots/{quizId}";
            internal readonly string kahootImageCdn = "https://images-cdn.kahoot.it/{imageId}";
            internal readonly string kahootWebsocket = "wss://kahoot.it/cometd/{gamePin}/{webSocketToken}";
            internal readonly string kahootSessionReservation = "https://kahoot.it/reserve/session/{gamePin}/?{millisTimestamp}";
        }
    }
}