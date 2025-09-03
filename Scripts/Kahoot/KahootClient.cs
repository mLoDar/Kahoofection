using System.Net.WebSockets;

using Kahoofection.Ressources;

using Newtonsoft.Json.Linq;



#pragma warning disable CA1822 // Mark members as static





namespace Kahoofection.Scripts.Kahoot
{
    internal class KahootClient
    {
        private const string _currentSection = "KahootClient";
        private static bool _clientTerminated = false;

        private static readonly ApplicationSettings.Urls _appUrls = new();



        internal void Terminate()
        {
            _clientTerminated = true;
        }

        internal async Task JoinGame(int gamePin, string gameNickname)
        {
            string subSection = "JoinGame";

            ClientWebSocket kahootWebSocket = new();

            ActivityLogger.Log(_currentSection, subSection, $"Received a new request to join a game with the pin '{gamePin}' as '{gameNickname}'.");

            long millisTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            string sessionUrl = _appUrls.kahootSessionReservation;
            sessionUrl = sessionUrl.Replace("{gamePin}", gamePin.ToString());
            sessionUrl = sessionUrl.Replace("{millisTimestamp}", millisTimestamp.ToString());



            (string webSocketToken, Exception? webSocketError) = await GetWebSocketToken(sessionUrl);

            if (webSocketError != null)
            {
                ActivityLogger.Log(_currentSection, subSection, webSocketError.Message);

                return;
            }



            int requestId = 0;

            string webSocketUrl = _appUrls.kahootWebsocket;
            webSocketUrl = webSocketUrl.Replace("{gamePin}", gamePin.ToString());
            webSocketUrl = webSocketUrl.Replace("{webSocketToken}", webSocketToken);

            ActivityLogger.Log(_currentSection, subSection, "Attempting to connect to the WebSocket.");

            try
            {
                await kahootWebSocket.ConnectAsync(new Uri(webSocketUrl), CancellationToken.None);
            }
            catch (Exception exception)
            {
                ActivityLogger.Log(_currentSection, subSection, "Failed to connect to the WebSocket.");
                ActivityLogger.Log(_currentSection, subSection, $"Token: {webSocketToken}", true);
                ActivityLogger.Log(_currentSection, subSection, $"Exception: {exception.Message}", true);

                return;
            }

            ActivityLogger.Log(_currentSection, subSection, "Successfully connected!");



            requestId++;

            (bool successfullyFetched, string clientId) = await GetWebSocketClientId(kahootWebSocket, requestId);

            if (successfullyFetched == false)
            {
                ActivityLogger.Log(_currentSection, subSection, "Failed to get a clientId!");

                return;
            }



            requestId++;

            var connectionData = new[]
            {
                new
                {
                    id = requestId.ToString(),
                    channel = "/meta/connect",
                    connectionType = "websocket",
                    advice = new
                    {
                        timeout = 0
                    },
                    clientId,
                    ext = new
                    {
                        ack = 0,
                        timesync =new
                        {
                            tc =
                            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                            l = 0,
                            o = 0
                        }
                    }
                }
            };

            bool successfulMessage = await SendWebSocketMessage(kahootWebSocket, requestId, connectionData);

            if (successfulMessage == false)
            {
                ActivityLogger.Log(_currentSection, subSection, "Failed to establish a game connection!");

                return;
            }



            requestId++;

            var loginData = new[]
            {
                new
                {
                    id = requestId.ToString(),
                    channel = "/service/controller",
                    data = new
                    {
                        type = "login",
                        gameid = gamePin,
                        host = "kahoot.it",
                        name = gameNickname,
                        content = "{}"
                    },
                    clientId,
                    ext = new { }
                }
            };

            successfulMessage = await SendWebSocketMessage(kahootWebSocket, requestId, loginData);

            if (successfulMessage == false)
            {
                ActivityLogger.Log(_currentSection, subSection, "Failed to establish a game connection!");

                return;
            }



            requestId++;

            var controllerData = new[]
            {
                new
                {
                    id = requestId.ToString(),
                    channel = "/service/controller",
                    data = new
                    {
                        gameid = gamePin,
                        type = "message",
                        host = "kahoot.it",
                        id = 16,
                        content = "{\"usingNamerator\":false}"
                    },
                    clientId,
                    ext = new { }
                }
            };

            successfulMessage = await SendWebSocketMessage(kahootWebSocket, requestId, controllerData);

            if (successfulMessage == false)
            {
                ActivityLogger.Log(_currentSection, subSection, "Failed to send initial controller data!");

                return;
            }



            requestId++;

            var gameData = new[]
            {
                new
                {
                    id = requestId.ToString(),
                    channel = "/service/controller",
                    data = new
                    {
                        gameid = gamePin,
                        type = "message",
                        host = "kahoot.it",
                        id = 61,
                        content = "{\"points\":0}"
                    },
                    clientId,
                    ext = new { }
                }
            };

            successfulMessage = await SendWebSocketMessage(kahootWebSocket, requestId, gameData);

            if (successfulMessage == false)
            {
                ActivityLogger.Log(_currentSection, subSection, "Failed to send initial controller data!");

                return;
            }



            ActivityLogger.Log(_currentSection, subSection, "Game connection established! Sending a continuous heartbeat to keep the connection alive.");

            while (_clientTerminated == false)
            {
                await Task.Delay(10000);

                if (_clientTerminated == true)
                {
                    break;
                }

                requestId++;

                var heartbeatData = new[]
                {
                        new
                        {
                            id = requestId.ToString(),
                            channel = "/meta/connect",
                            connectionType = "websocket",
                            clientId,
                            ext = new
                            {
                                ack = requestId,
                                timesync = new
                                {
                                    tc = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                                    l = 0,
                                    o = 0
                                }
                            }
                        }
                    };

                bool heartbeatSent = await SendWebSocketMessage(kahootWebSocket, requestId, heartbeatData);
                
                if (heartbeatSent == false)
                {
                    ActivityLogger.Log(_currentSection, subSection, "Failed to maintain the connection! Last heartbeat failed.");

                    return;
                }
            }

            kahootWebSocket.Abort();
            await kahootWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            
            ActivityLogger.Log(_currentSection, subSection, $"The client '{gameNickname}' with clientId '{clientId}' connected to game '{gamePin}' was terminated, see you soon!");
        }

        private static async Task<(string webSocketToken, Exception? occurredError)> GetWebSocketToken(string url)
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

                if (fetchedWebSocketToken == false)
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

        private static async Task<(string decodeChallenge, string encodedSessionToken, Exception? occurredError)> ReserveSession(string url)
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

        private static async Task<(bool successfullyFetched, string clientId)> GetWebSocketClientId(ClientWebSocket kahootWebSocket, int requestId)
        {
            string subSection = "GetWebSocketClientId";

            var handShakeData = new[]
            {
                new
                {
                    id = requestId.ToString(),
                    version = "1.0",
                    minimumVersion = "1.0",
                    channel = "/meta/handshake",
                    supportedConnectionTypes = new[] { "websocket" },
                    advice = new
                    {
                        timeout = 60000,
                        interval = 0
                    },
                    ext = new
                    {
                        ack = true,
                        timesync = new
                        {
                            tc = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                            l = 0,
                            o = 0
                        }
                    }
                }
            };



            try
            {
                await WebSocketHelper.SendMessageAsync(kahootWebSocket, handShakeData);
                (string messageContent, Exception? occurredError) = await WebSocketHelper.ReceiveMessageAsync(kahootWebSocket);
                
                if (occurredError != null)
                {
                    throw occurredError;
                }

                JArray jsonArray = JArray.Parse(messageContent);

                if (jsonArray == null || jsonArray.Count <= 0)
                {
                    throw new Exception("The received 'jsonArray' is either null or contains no entries.");
                }

                string clientId = jsonArray?[0]?["clientId"]?.ToString() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(clientId) || clientId.Equals(string.Empty))
                {
                    throw new Exception("The received 'clientId' is either null, a whitespace or empty.");
                }

                return (true, clientId);
            }
            catch (Exception exception)
            {
                ActivityLogger.Log(_currentSection, subSection, "Failed to fetch the WebSocket's clientId with the initial handshake.");
                ActivityLogger.Log(_currentSection, subSection, exception.Message, true);

                return (false, string.Empty);
            }
        }

        private static async Task<bool> SendWebSocketMessage(ClientWebSocket kahootWebSocket, int requestId, object message)
        {
            string subSection = "SendWebSocketMessage";

            try
            {
                await WebSocketHelper.SendMessageAsync(kahootWebSocket, message);
                (string messageContent, Exception? occurredError) = await WebSocketHelper.ReceiveMessageAsync(kahootWebSocket);
                
                if (occurredError != null)
                {
                    throw occurredError;
                }

                return true;
            }
            catch (Exception exception)
            {
                ActivityLogger.Log(_currentSection, subSection, $"Failed to send message data for request '{requestId}'.");
                ActivityLogger.Log(_currentSection, subSection, $"Exception: {exception.Message}", true);

                return false;
            }
        }
    }
}