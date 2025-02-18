using System.Net.WebSockets;

using Kahoofection.Ressources;

using Newtonsoft.Json.Linq;





namespace Kahoofection.Scripts.Kahoot
{
    internal class KahootClient
    {
        private readonly ClientWebSocket _kahootWebSocket = new();

        private readonly ApplicationSettings.Urls _appUrls = new();



        internal async Task JoinGame(int gamePin, string gameNickname)
        {
            long millisTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            string sessionUrl = _appUrls.kahootSessionReservation;
            sessionUrl = sessionUrl.Replace("{gamePin}", gamePin.ToString());
            sessionUrl = sessionUrl.Replace("{millisTimestamp}", millisTimestamp.ToString());

            // TODO: Actually join the game
        }

        internal static async Task<(string webSocketToken, Exception? occurredError)> GetWebSocketToken(string url)
        {
            try
            {
                (string decodeChallenge, string encodedSessionToken, Exception? sessionReservationError) = await ReserveSession(url);

                if (sessionReservationError != null)
                {
                    throw sessionReservationError;
                }

                if (string.IsNullOrWhiteSpace(decodeChallenge))
                {
                    throw new Exception("Failed to reserve session, 'decodeChallenge' is null or whitespace.");
                }

                if (string.IsNullOrWhiteSpace(encodedSessionToken) == true)
                {
                    throw new Exception("Failed to reserve session, 'encodedSessionToken' is null or whitespace.");
                }

                (bool fetchedWebSocketToken, string webSocketToken, Exception? challengeSolverError) = ChallengeSolver.GetWebSocketToken(decodeChallenge, encodedSessionToken);

                if (fetchedWebSocketToken == false && challengeSolverError != null)
                {
                    throw challengeSolverError;
                }

                if (fetchedWebSocketToken)
                {
                    throw new Exception($"Failed to fetch webSocketToken, due to unknown reasons.");
                }

                return (webSocketToken, null);
            }
            catch (Exception exception)
            {
                return (string.Empty, exception);
            }
        }

        internal static async Task<(string decodeChallenge, string encodedSessionToken, Exception? occurredError)> ReserveSession(string url)
        {
            try
            {
                using HttpClient client = new();

                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                JObject? apiResponse = JObject.Parse(responseBody);

                string decodeChallenge = apiResponse?["challenge"]?.ToString() ?? "-";

                if (decodeChallenge.Equals("-"))
                {
                    throw new Exception("Failed to reserve session. No valid 'decodeChallenge' was found.");
                }

                foreach (var header in response.Headers)
                {
                    if (header.Key.Equals("x-kahoot-session-token"))
                    {
                        return (decodeChallenge, string.Join("", header.Value), null);
                    }
                }

                throw new Exception("Failed to reserve session. No valid 'x-kahoot-session-token' was found.");
            }
            catch (Exception exception)
            {
                return (string.Empty, string.Empty, exception);
            }
        }
    }
}