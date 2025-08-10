using Newtonsoft.Json;

namespace HomePage
{
    public class ThemeDayRepository : Repository<ThemeDay>
    {
        public override string FileName => "ThemeDay.txt";

        public IEnumerable<ThemeDay> InfoForDate(DateTime date)
        {
            var dateKey = DateHelper.ToKey(date);
            return GetValues().Values.Where(x => x.Date == dateKey);
        }

        public void UpdateFromJsonFile()
        {
            var jsonString = File.ReadAllText("DaysJson.txt");
            var json = JsonConvert.DeserializeObject<ApiReponse>(jsonString);
            var days = json?.response ?? [];
            var values = days
                .Select(x => new ThemeDay { Key = x.id, Date = DateHelper.KeyFromKeyWithZeros(x.date), DayName = x.name})
                .ToDictionary(x => x.Key, x => x);
            SaveValues(values);
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
