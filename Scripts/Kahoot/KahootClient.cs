using System.Net.WebSockets;





namespace Kahoofection.Scripts.Kahoot
{
    internal class KahootClient
    {
        private readonly ClientWebSocket _kahootWebSocket = new();



        internal async Task JoinGame(int gamePin, string gameNickname, string websocketToken)
        {

        }
    }
}