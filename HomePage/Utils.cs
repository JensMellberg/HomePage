using System.Globalization;
using HomePage.Model;
using Microsoft.AspNetCore.Mvc;

namespace HomePage
{
    public static class Utils
    {
        public static JsonResult CreateRedirectClientResult(string redirectUrl) => new(new { redirectUrl });

        public static JsonResult CreateErrorClientResult(string? message) => new(new { success = false, message });

        public static JsonResult CreateClientResult(object? data) => new(new { data = data ?? new { success = true }, success = true });
        public static string EncodeForClient(this string _this) => _this.Replace('Ä', '.').Replace('Ö', '*');
        public static void CalculateAverages(IEnumerable<FoodRanking> rankings, out double jensAverage, out double annaAverage, out double totalAverage)
        {
            var jensRankings = rankings.Where(x => x.Person == Person.Jens.Name).ToArray();
            var annaRankings = rankings.Where(x => x.Person == Person.Anna.Name).ToArray();
            jensAverage = (double)jensRankings.Sum(x => x.Ranking) / LengthButNotZero(jensRankings.Length);
            annaAverage = (double)annaRankings.Sum(x => x.Ranking) / LengthButNotZero(annaRankings.Length);
            totalAverage = 0;
            if (annaAverage == 0)
            {
                totalAverage = jensAverage;
            }
            else if (jensAverage == 0)
            {
                totalAverage = annaAverage;
            }
            else
            {
                totalAverage = (jensAverage + annaAverage) / 2;
            }

            jensAverage = Math.Round(jensAverage, 1);
            annaAverage = Math.Round(annaAverage, 1);
            totalAverage = Math.Round(totalAverage, 1);
            int LengthButNotZero(int length) => length == 0 ? 1 : length;
        }

        public static double ToDouble(this string s) => double.Parse(s.Replace(',', '.'), CultureInfo.InvariantCulture);

        public static string GetTextOrNothing(double text) => text == 0 ? "-" : text.ToString();
    }
}
