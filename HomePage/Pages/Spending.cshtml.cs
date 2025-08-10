using HomePage.Spending;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace HomePage.Pages
{
    public class SpendingModel : PageModel
    {
        public string Person { get; set; }

        public string ChosenMonth { get; set; }

        public List<(string key, string value)> MonthsToChoose { get; set; }

        public List<MonthSpendingGroup> SpendingGroups { get; set; }

        public int Total { get; set; }

        public string GroupOptionsJson { get; set; }

        public void OnGet(string person, string dateMonth)
        {
            Person = person ?? "Both";
            var allSpendings = new SpendingItemRepository().GetValues().Values.Where(x => x.Person == Person).ToList();
            allSpendings.ForEach(x => x.ConvertedDate = DateHelper.FromKey(x.Date));
            
            var allDates = allSpendings.Select(x => new DateTime(x.ConvertedDate.Year, x.ConvertedDate.Month, 1)).Distinct().OrderBy(x => x).ToList();
            MonthsToChoose = allDates.Select(x => (DateHelper.ToKey(x), DateHelper.MonthNumberToString[x.Month] + " " + x.Year)).ToList();
            ChosenMonth = dateMonth ?? DateHelper.ToKey(allDates.Count > 0 ? allDates.Last() : DateTime.Now);
            var convertedSelectedDate = DateHelper.FromKey(ChosenMonth);

            var groups = new SpendingGroupRepository().GetAllSpendingGroups(Person, convertedSelectedDate, convertedSelectedDate.AddMonths(1).AddDays(-1));
            GroupOptionsJson = JsonConvert.SerializeObject(groups.Where(x => !string.IsNullOrEmpty(x.Id)).Select(x => new { id = x.Id, label = x.Name }));
            var spendingsForMonth = allSpendings.Where(x => x.ConvertedDate.Month == convertedSelectedDate.Month && x.ConvertedDate.Year == convertedSelectedDate.Year).ToList();
            var monthGroups = SortIntoGroups(spendingsForMonth, groups);

            SpendingGroups = monthGroups.Values.OrderBy(x => x.Entries.Count > 0 ? 0 : 1).ThenBy(x => x.SortOrder).ToList();
            Total = SpendingGroups.Sum(x => x.TotalWithIgnore);
        }

        public static IDictionary<string, MonthSpendingGroup> SortIntoGroups(IEnumerable<SpendingItem> items, IEnumerable<ISpendingGroup> groups)
        {
            var monthGroups = groups.ToDictionary(x => x.Name, x => new MonthSpendingGroup(x));
            foreach (var spending in items)
            {
                if (!string.IsNullOrEmpty(spending.SetGroupId))
                {
                    monthGroups.Values.Where(x => x.Id == spending.SetGroupId).First().Entries.Add(spending);
                    continue;
                }

                var matchedGroup = groups.First(x => x.IsMatch(spending));
                monthGroups[matchedGroup.Name].Entries.Add(spending);
            }

            return monthGroups;
        }
    }
}
