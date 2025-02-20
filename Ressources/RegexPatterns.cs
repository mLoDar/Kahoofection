using System.Text.RegularExpressions;





namespace Kahoofection.Ressources
{
    partial class RegexPatterns
    {
        [GeneratedRegex(@"\x1B\[[0-9;]*m")]
        internal static partial Regex AnsiSequence();

        [GeneratedRegex(@"\s+")]
        internal static partial Regex AllWhitespaces();

        [GeneratedRegex("[^0-9]")]
        internal static partial Regex NoNumbers();



        [GeneratedRegex(@"decode\.call\(this, '([^']+)'\)")]
        internal static partial Regex KahootChallengeToken();

        [GeneratedRegex(@"var offset = (.+?);")]
        internal static partial Regex KahootChallengeOffset();
    }
}