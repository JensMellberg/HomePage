using HomePage.Data;
using HomePage.Model;
using Newtonsoft.Json;

namespace HomePage.Repositories
{
    public class RedDayRepository(AppDbContext dbContext, SettingsRepository settingsRepository, DatabaseLogger logger)
    {
        private static string ApiUrl(int year) => $"https://sholiday.faboul.se/dagar/v2.1/{year}";

        public RedDay? InfoForDate(DateTime date)
        {
            var year = date.Year;
            var settings = settingsRepository.Settings;
            if (settings.LastRedDayUpdateDate.Year != year)
            {
                UpdateDays(year).Wait();
                settings.LastRedDayUpdateDate = DateHelper.DateTimeNow;
                dbContext.SaveChanges();
            }

            return dbContext.RedDay.FirstOrDefault(x => x.Date == date);
        }

        private async Task UpdateDays(int year)
        {
            var daysResponse = await new HttpClient().GetAsync(ApiUrl(year));
            if (!daysResponse.IsSuccessStatusCode)
            {
                return;
            }

            var jsonString = await daysResponse.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<ApiReponse>(jsonString);
            var days = json?.dagar ?? [];
            var values = days
                .Where(x => !string.IsNullOrEmpty(x.helgdag) || x.RedDayText == "Ja")
                .Select(x => new RedDay { Date = DateHelper.FromKey(DateHelper.KeyFromKeyWithZeros(x.datum)), DayName = x.helgdag, IsRed = x.RedDayText == "Ja" })
                .ToList();

            if (values?.Any() == true)
            {
                dbContext.RedDay.RemoveRange(dbContext.RedDay.ToList());
                dbContext.RedDay.AddRange(values);
                dbContext.SaveChanges();
            } else
            {
                logger.Error($"Failed to retrieve red days information from api.", null);
            }
        }

        private class ApiReponse
        {
            public IEnumerable<DayResponse> dagar { get; set; }
        }

        private class DayResponse
        {
            public string datum { get; set; }

            [JsonProperty(PropertyName = "röd dag")]
            public string RedDayText { get; set; }

            public string helgdag { get; set; }
        }
    }
}
