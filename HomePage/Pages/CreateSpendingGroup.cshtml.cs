using System;
using HomePage.Spending;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomePage.Pages
{
    public class CreateSpendingGroupModel : PageModel
    {
        public SpendingGroup Group { get; set; }

        public string ReturnDate { get; set; }

        public bool IsNew { get; set; }
        public IActionResult OnGet(string groupId, string returnDate)
        {
            this.TryLogIn();
            if (this.ShouldRedirectToLogin())
            {
                return new RedirectResult("/Login");
            }

            IsNew = string.IsNullOrEmpty(groupId);
            if (string.IsNullOrEmpty(groupId))
            {
                Group = new SpendingGroup();
            }
            else
            {
                Group = new SpendingGroupRepository().TryGetValue(groupId) ?? new SpendingGroup();
            }

            ReturnDate = returnDate;
            return Page();
        }

        public IActionResult OnPost(string groupId,
            string delete, 
            string place,
            string ignoreTowardsTotal,
            string patterns, 
            int sortOrder,
            string color,
            string isDateBased,
            string startDate,
            string endDate,
            string returnDate)
        {
            if (!string.IsNullOrEmpty(delete))
            {
                new SpendingGroupRepository().Delete(groupId);
            }
            else if (new SpendingGroupRepository().GetValues().Values.Any(x => x.Key != groupId && x.Name == place))
            {

            }
            else
            {
                if (isDateBased == "on")
                {
                    patterns = null;
                    sortOrder = 0;
                    startDate = DateHelper.KeyFromKeyWithZeros(startDate);
                    endDate = DateHelper.KeyFromKeyWithZeros(endDate);
                } else
                {
                    startDate = null;
                    endDate = null;
                }

                var group = new SpendingGroup { 
                    Key = groupId,
                    IgnoreTowardsTotal = ignoreTowardsTotal == "on",
                    Person = "Both",
                    Name = place, 
                    SortOrder = sortOrder,
                    Color = color,
                    Patterns = string.IsNullOrEmpty(patterns) ? null : patterns.Split('¤').ToList(),
                    StartDate = startDate,
                    EndDate = endDate
                };

                new SpendingGroupRepository().SaveValue(group);
            }

            return Redirect($"/Spending?dateMonth="+returnDate);
        }
    }
}
