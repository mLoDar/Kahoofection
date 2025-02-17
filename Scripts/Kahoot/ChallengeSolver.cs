using System.Text;
using System.Text.RegularExpressions;

using Kahoofection.Ressources;

using NCalc;



#pragma warning disable IDE0057 // Use range operator





namespace Kahoofection.Scripts.Kahoot
{
    internal partial class ChallengeSolver
    {
        internal static (bool fetchedWebSocketToken, string webSocketToken, Exception? occurredError) GetWebSocketToken(string decodeChallenge, string encodedSessionToken)
        {
            try
            {
                (int tokenOffset, Exception? offsetError) = GetTokenOffset(decodeChallenge);
                string encodedChallengeToken = GetEncodedChallengeToken(decodeChallenge);

                if (offsetError != null)
                {
                    throw new Exception($"Failed to fetch the token offset from the provided challenge string. {offsetError.Message}");
                }

                if (string.IsNullOrWhiteSpace(encodedChallengeToken))
                {
                    throw new Exception("Failed to fetch the encoded token from the provided challenge string. The 'encodedChallengeToken' is null or whitespace.");
                }



                (string sessionToken, string challengeToken, Exception? decodeError) = DecodeTokens(encodedSessionToken, encodedChallengeToken, tokenOffset);

                if (decodeError != null)
                {
                    throw new Exception($"Failed to decode the session and/or challenge token. {decodeError.Message}");
                }



                (string webSocketToken, Exception? computeTokenError) = ComputeWebSocketToken(sessionToken, challengeToken);

                if (computeTokenError != null)
                {
                    throw new Exception($"Failed to compute the web socket token. {computeTokenError.Message}");
                }



                return (true, webSocketToken, null);
            }
            catch (Exception exception)
            {
                return (false, string.Empty, exception);
            }
        }

        private static string GetEncodedChallengeToken(string challenge)
        {
            Match match = RegexPatterns.KahootChallengeToken().Match(challenge);
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        private static (int tokenOffset, Exception? occurredError) GetTokenOffset(string challenge)
        {
            try
            {
                Match match = RegexPatterns.KahootChallengeOffset().Match(challenge);

                if (match.Success)
                {
                    string expression = match.Groups[1].Value;

                    expression = RegexPatterns.AllWhitespaces().Replace(expression, "");
                    expression = expression.Replace("*", " * ");
                    expression = expression.Replace("+", " + ");



                    var calcExpression = new Expression(expression);
                    object result = calcExpression.Evaluate() ?? throw new Exception("Failed to evaluate offset expression. The 'result' of the expression was null.");
                    string computedExpression = result.ToString() ?? throw new Exception("Failed to evaluate offset expression. The 'computedExpression' of the expression was null.");



                    if (int.TryParse(computedExpression, out int tokenOffset) == false)
                    {
                        throw new Exception($"Computed expression is not a valid int ({computedExpression}).");
                    }

                    return (tokenOffset, null);
                }

                throw new Exception("Failed to find any matches for the offset expression.");
            }
            catch (Exception exception)
            {
                return (-1, exception);
            }
        }

        private static (string sessionToken, string challengeToken, Exception? occurredError) DecodeTokens(string encodedSessionToken, string encodedChallengeToken, int tokenOffset)
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
                


                return (sessionToken, challengeToken, null);
            }
            catch (Exception exception)
            {
                return (string.Empty, string.Empty, exception);
            }
        }

        private static (string webSocketToken, Exception? occurredError) ComputeWebSocketToken(string sessionToken, string challengeToken)
        {
            try
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

                return (webSocketToken, null);
            }
            catch (Exception exception)
            {
                return (string.Empty, exception);
            }
        }
    }
}