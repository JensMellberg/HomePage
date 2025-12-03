using HomePage.Data;
using HomePage.Model;
using Newtonsoft.Json;

namespace HomePage.Repositories
{
    public class ThemeDayRepository(AppDbContext dbContext)
    {
        public IEnumerable<ThemeDay> InfoForDate(DateTime date)
        {
            return dbContext.ThemeDay.Where(x => x.ThemeDate == date).ToList();
        }

        public void UpdateFromJsonFile()
        {
            var jsonString = File.ReadAllText("DaysJson.txt");
            var json = JsonConvert.DeserializeObject<ApiReponse>(jsonString);
            var days = json?.response ?? [];
            var values = days
                .Select(x => new ThemeDay { Key = x.id, ThemeDate = DateHelper.FromKey(DateHelper.KeyFromKeyWithZeros(x.date)), DayName = x.name });
            dbContext.ThemeDay.RemoveRange(dbContext.ThemeDay);
            dbContext.ThemeDay.AddRange(values);
        }

        private class ApiReponse
        {
            public IEnumerable<DayResponse> response { get; set; }
        }

        private class DayResponse
        {
            public string id { get; set; }
            public string name { get; set; }

            public string date { get; set; }
        }
    }
}
