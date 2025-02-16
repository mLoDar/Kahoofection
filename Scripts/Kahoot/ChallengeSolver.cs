using System.Data;
using System.Text;
using System.Text.RegularExpressions;

using Kahoofection.Ressources;



#pragma warning disable IDE0057 // Use range operator





namespace Kahoofection.Scripts.Kahoot
{
    internal partial class ChallengeSolver
    {
        internal static string GetWebSocketToken(string decodeChallenge, string encodedSessionToken)
        {
            string encodedChallengeToken = GetEncodedChallengeToken(decodeChallenge);
            int tokenOffset = GetTokenOffset(decodeChallenge);

            (string sessionToken, string challengeToken) = DecodeTokens(encodedSessionToken, encodedChallengeToken, tokenOffset);

            string webSocketToken = ComputeWebSocketToken(sessionToken, challengeToken);
            return webSocketToken;
        }

        private static string GetEncodedChallengeToken(string challenge)
        {
            Match match = RegexPatterns.KahootChallengeToken().Match(challenge);
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        private static int GetTokenOffset(string challenge)
        {
            Match match = RegexPatterns.KahootChallengeOffset().Match(challenge);

            if (match.Success)
            {
                string expression = match.Groups[1].Value;

                expression = RegexPatterns.AllWhitespaces().Replace(expression, "");
                expression = expression.Replace("*", " * ");
                expression = expression.Replace("+", " + ");

                DataTable table = new();
                return Convert.ToInt32(table.Compute(expression, ""));
            }

            return -1;
        }

        private static (string sessionToken, string challengeToken) DecodeTokens(string encodedSessionToken, string encodedChallengeToken, int tokenOffset)
        {
            try
            {
                var encodedSessionTokenBytes = Convert.FromBase64String(encodedSessionToken);
                string sessionToken = Encoding.UTF8.GetString(encodedSessionTokenBytes);



                char[] challengeTokenChars = new char[encodedChallengeToken.Length];

                for (int i = 0; i < encodedChallengeToken.Length; i++)
                {
                    challengeTokenChars[i] = (char)((((encodedChallengeToken[i] * i) + tokenOffset) % 77) + 48);
                }

                string challengeToken = new(challengeTokenChars);
                


                return (sessionToken, challengeToken);
            }
            catch
            {
                return (string.Empty, string.Empty);
            }
        }

        private static string ComputeWebSocketToken(string sessionToken, string challengeToken)
        {
            int maxTokenLength = Math.Max(sessionToken.Length, challengeToken.Length);
            char[] webSocketTokenChars = new char[maxTokenLength];

            for (int i = 0; i < maxTokenLength; i++)
            {
                char sessionChar = i < sessionToken.Length ? sessionToken[i] : (char)0;
                char challengeChar = i < challengeToken.Length ? challengeToken[i] : (char)0;

                webSocketTokenChars[i] = (char)(sessionChar ^ challengeChar);
            }
            
            string webSocketToken = new(webSocketTokenChars);
            webSocketToken = webSocketToken.Substring(0, webSocketToken.Length - 4);

            return webSocketToken;
        }
    }
}