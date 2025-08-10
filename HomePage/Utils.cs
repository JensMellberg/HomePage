using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomePage
{
    public static class Utils
    {
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

        public static bool ShouldRedirectToLogin(this PageModel model)
        {
            if (!SignInRepository.IsLoggedIn(model.HttpContext.Session))
            {
                model.HttpContext.Session.SetString("ReturnUrl", model.Request.GetDisplayUrl());
                return true;
            }

            return false;
        }

        public static void TryLogIn(this PageModel model)
        {
            SignInRepository.TryLogIn(model.HttpContext.Session, model.Request, model.Response);
        }
    }
}
