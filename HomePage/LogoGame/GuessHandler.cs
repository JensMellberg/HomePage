using System.Globalization;
using System.Text;

namespace HomePage.LogoGame
{
    public static class GuessHandler
    {
        public static bool IsCorrect(string guess, string[] answers) => answers.Any(x => IsCorrect(guess, x));

        public static bool IsCorrect(string guess, string answer)
        {
            guess = Normalize(guess);
            answer = Normalize(answer);

            if (guess.Equals(answer, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            int maxLength = Math.Max(guess.Length, answer.Length);

            if (maxLength <= 3)
            {
                return false;
            }

            if (answer.StartsWith(guess, StringComparison.InvariantCultureIgnoreCase) && answer.Length - guess.Length == 1)
            {
                return answer.Length >= 7;
            }

            int maxDistance = maxLength switch
            {
                <= 7 => 1,
                <= 12 => 2,
                _ => 3
            };

            if (Math.Abs(guess.Length - answer.Length) > maxDistance)
            {
                return false;
            }


            return DamerauLevenshtein(guess, answer, maxDistance) <= maxDistance;
        }

        static string Normalize(string value)
        {
            var normalized = value.Normalize(NormalizationForm.FormD);

            var builder = new StringBuilder(normalized.Length);

            foreach (char c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                if (char.IsLetterOrDigit(c))
                {
                    builder.Append(char.ToLowerInvariant(c));
                }
            }

            return builder.ToString();
        }

        static int DamerauLevenshtein(string a, string b, int maxDistance)
        {
            int n = a.Length;
            int m = b.Length;

            if (Math.Abs(n - m) > maxDistance)
            {
                return maxDistance + 1;
            }


            var matrix = new int[n + 1, m + 1];

            for (int i = 0; i <= n; i++)
            {
                matrix[i, 0] = i;
            }


            for (int j = 0; j <= m; j++)
            {
                matrix[0, j] = j;
            }

            for (int i = 1; i <= n; i++)
            {
                int rowMin = int.MaxValue;

                for (int j = 1; j <= m; j++)
                {
                    int cost = a[i - 1] == b[j - 1] ? 0 : 1;

                    int value = Math.Min(
                        matrix[i - 1, j] + 1,
                        Math.Min(
                            matrix[i, j - 1] + 1,
                            matrix[i - 1, j - 1] + cost
                        )
                    );

                    if (i > 1 &&
                        j > 1 &&
                        a[i - 1] == b[j - 2] &&
                        a[i - 2] == b[j - 1])
                    {
                        value = Math.Min(value, matrix[i - 2, j - 2] + cost
                        );
                    }

                    matrix[i, j] = value;
                    rowMin = Math.Min(rowMin, value);
                }

                if (rowMin > maxDistance)
                {
                    return maxDistance + 1;
                }

            }

            return matrix[n, m];
        }
    }
}
