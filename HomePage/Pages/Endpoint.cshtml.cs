using HomePage.Chores;
using HomePage.Data;
using HomePage.Model;
using HomePage.Repositories;
using HomePage.Spending;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace HomePage.Pages
{
    [IgnoreAntiforgeryToken]
    public class EndpointModel(AppDbContext dbContext,
        SignInRepository signInRepository,
        ChoreRepository choreRepository, 
        DatabaseLogger logger,
        SettingsRepository settingsRepository) : BasePage(signInRepository)
    {
        public void OnGet()
        {
        }

        public ActionResult OnPost(string action, string transactions, Guid itemKey, string groupId, string movieSearch, string username, string password, string ingredients)
        {
            if (action == "MovieSearch")
            {
                var movieResponse = new HttpClient().GetAsync("https://v2.sg.media-imdb.com/suggestion/h/" + movieSearch + ".json").Result;
                if (!movieResponse.IsSuccessStatusCode)
                {
                    return new JsonResult(new { success = false });
                }

                var jsonString = movieResponse.Content.ReadAsStringAsync().Result;
                return new JsonResult(jsonString);
            }

            if (!string.IsNullOrEmpty(transactions) && signInRepository.VerifyUserCredentials(Request, username, password)?.IsAdmin == true)
            {
                var parsed = JsonConvert.DeserializeObject<List<Transaction>>(transactions);
                var person = action;
                if (parsed == null || person != Person.Jens.Name && person != Person.Anna.Name && person != "Both")
                {
                    return new JsonResult(new { success = false });
                }

                var existingTransactions = dbContext.SpendingItem.ToList();
                var valuesToSave = new List<SpendingItem>();
                foreach (var tran in parsed)
                {
                    var spending = new SpendingItem { 
                        Amount = tran.amount,
                        TransactionDate = DateHelper.FromKey(DateHelper.KeyFromKeyWithZeros(tran.date)),
                        Person = person,
                        Place = tran.place
                    };

                    if (!existingTransactions.Any(x => x.CollidesWith(spending)))
                    {
                        while (valuesToSave.Any(x => x.CollidesWith(spending)))
                        {
                            spending.Amount += 1;
                        }

                        valuesToSave.Add(spending);
                    }
                }

                dbContext.AddRange(valuesToSave);
                dbContext.SaveChanges();
                return new JsonResult(new { success = true, saved = valuesToSave.Count });
            }

            if (action == "MoveGroup")
            {
                if (!IsAdmin)
                {
                    return Redirect("/Login");
                }

                var item = dbContext.SpendingItem.Find(itemKey);
                if (item == null)
                {
                    return new JsonResult(new { success = false });
                }

                item.SetGroupId = groupId;
                dbContext.SaveChanges();

                return new JsonResult(new { success = true });
            }

            if (action == "SaveIngredients")
            {
                if (!IsAdmin)
                {
                    return Redirect("/Login");
                }

                var food = dbContext.Food
                    .Include(x => x.FoodConnections)
                    .Include(x => x.Categories)
                    .Include(x => x.FoodIngredients)
                    .FirstOrDefault(x => x.Id == itemKey);
                if (food == null)
                {
                    return new JsonResult(new { success = false });
                }

                food.FoodIngredients = ingredients?.Split('¤')?.Select(FoodIngredient.Parse)?.ToList() ?? [];
                dbContext.SaveChanges();

                return new JsonResult(new { success = true });
            }

            if (action == "ImageUrl")
            {
                var pair = ImageRepository.Instance.GetImageUrl();
                var lastUpdated = dbContext.Settings.Single().LastHomePageChange;
                return new JsonResult(new { lastUpdated, url = pair.Item1, taken = pair.Item2 });
            }

            var redirectResult = GetPotentialClientRedirectResult(true, true, "/Index");
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var chore = choreRepository.GetChore(action);
            if (chore != null)
            {
                var chorePerson = chore.ChorePerson();
                if (chorePerson == null)
                {
                    return Utils.CreateErrorClientResult(null);
                }

                var streak = chore.Update();
                logger.Information($"{chorePerson} performed chore {chore.Id} with a new streak of {streak}.", null);
                dbContext.SaveChanges();
                settingsRepository.UpdateHomePageChange();
                return Utils.CreateClientResult(new { streak });
            }

            return Utils.CreateErrorClientResult(null);
        }

        public class Transaction
        {
            public string date { get; set; }

            public string place { get; set; }

            public int amount { get; set; }
        }
    }
}
