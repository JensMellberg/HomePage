using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HomePage
{
    public class RedDayRepository : Repository<RedDay>
    {
        public override string FileName => "RedDay.txt";

        private static string ApiUrl(int year) => $"https://sholiday.faboul.se/dagar/v2.1/{year}";

        public RedDay? InfoForDate(DateTime date)
        {
            var year = date.Year;
            var settingsRepo = new SettingsRepository();
            var settings = settingsRepo.Get();
            if (settings.LastRedDayYearUpdate != year)
            {
                UpdateDays(year).Wait();
                settings.LastRedDayYearUpdate = year;
                settingsRepo.Save(settings);
            }

            return TryGetValue(DateHelper.ToKey(date));
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
                .Select(x => new RedDay { Key = DateHelper.KeyFromKeyWithZeros(x.datum), DayName = x.helgdag, IsRed = x.RedDayText == "Ja"})
                .ToDictionary(x => x.Key, x => x);
            SaveValues(values);
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
