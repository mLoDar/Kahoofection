using System.Text;
using System.Net.WebSockets;

using Newtonsoft.Json;





namespace Kahoofection.Scripts
{
    internal class WebSocketHelper
    {
        internal static async Task<Exception?> SendMessageAsync(ClientWebSocket webSocket, object message, CancellationToken cancellationToken = default)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(webSocket);

                if (webSocket.State != WebSocketState.Open)
                {
                    throw new Exception($"WebSocketState is not open, received as '({webSocket.State})'");
                }

                string jsonMessage = JsonConvert.SerializeObject(message, Formatting.None);
                byte[] buffer = Encoding.UTF8.GetBytes(jsonMessage);
                await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, cancellationToken);

                return (null);
            }
            catch (Exception exception)
            {
                return exception;
            }
        }

        internal static async Task<(string messageContent, Exception? occurredError)> ReceiveMessageAsync(ClientWebSocket webSocket, CancellationToken cancellationToken = default)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(webSocket);

                if (webSocket.State != WebSocketState.Open)
                {
                    throw new Exception($"WebSocketState is not open, received as '({webSocket.State})'");
                }

                var buffer = new byte[4096];

                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                string messageContent = Encoding.UTF8.GetString(buffer, 0, result.Count);

                return (messageContent, null);
            }
            catch (Exception exception)
            {
                return (string.Empty, exception);
            }
        }
    }
}