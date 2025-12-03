using System;
using HomePage.Data;
using HomePage.Spending;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomePage.Pages
{
    [RequireAdmin]
    public class CreateSpendingGroupModel(AppDbContext dbContext, SignInRepository signInRepository) : BasePage(signInRepository)
    {
        public SpendingGroup Group { get; set; }

        public string ReturnDate { get; set; }

        public bool IsNew { get; set; }
        public IActionResult OnGet(Guid groupId, string returnDate)
        {
            IsNew = groupId == Guid.Empty;
            if (IsNew)
            {
                Group = new SpendingGroup();
            }
            else
            {
                Group = dbContext.SpendingGroup.Find(groupId) ?? new SpendingGroup();
            }

            ReturnDate = returnDate;
            return Page();
        }

        public IActionResult OnPost(Guid groupId,
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
            var existing = dbContext.SpendingGroup.Find(groupId);
            if (!string.IsNullOrEmpty(delete) && existing != null)
            {
                dbContext.SpendingGroup.Remove(existing);
            }
            else if (dbContext.SpendingGroup.Any(x => x.Id != groupId && x.Name == place))
            {

            }
            else
            {
                List<string> patternList = new();
                var convertedStartDate = isDateBased == "on" ? DateHelper.FromKey(DateHelper.KeyFromKeyWithZeros(startDate)) : (DateTime?)null;
                var convertedEndDate = isDateBased == "on" ? DateHelper.FromKey(DateHelper.KeyFromKeyWithZeros(endDate)) : (DateTime?)null;
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
                    patternList = patterns.Split('¤').ToList();
                }

                if (existing != null)
                {
                    existing.IgnoreTowardsTotal = ignoreTowardsTotal == "on";
                    existing.Name = place;
                    existing.SortOrder = sortOrder;
                    existing.StartDate = convertedStartDate;
                    existing.EndDate = convertedEndDate;
                    existing.Patterns = patternList;
                } else
                {
                    var group = new SpendingGroup
                    {
                        Id = groupId,
                        IgnoreTowardsTotal = ignoreTowardsTotal == "on",
                        Person = "Both",
                        Name = place,
                        SortOrder = sortOrder,
                        Color = color,
                        Patterns = patternList,
                        StartDate = convertedStartDate,
                        EndDate = convertedEndDate
                    };

                    dbContext.SpendingGroup.Add(group);
                }
            }

            dbContext.SaveChanges();
            return Redirect($"/Spending?dateMonth="+returnDate);
        }
    }
}
