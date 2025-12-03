using System.Net.Http.Headers;
using System.Text;
using HomePage.Data;
using HomePage.Repositories;
using HomePage.Spending;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace HomePage.Pages
{
    [IgnoreAntiforgeryToken]
    public class EndpointModel(AppDbContext dbContext, SignInRepository signInRepository) : BasePage(signInRepository)
    {
        public void OnGet()
        {
        }

        public ActionResult OnPost(string action, string transactions, Guid itemKey, string groupId, string movieSearch)
        {
            /*if (action == "shoppingList")
            {
                var today = DateTime.Now.Date;
                var foodRepo = new FoodRepository();
                var relevantDays = new DayFoodRepository().GetValues().Values.Where(x => x.Day >= today).ToList();
                var allFoods = foodRepo.GetValues();
                var relevantFoods = relevantDays.Select(x => allFoods[x.FoodId]).ToList();
                var allIngredients = new Dictionary<string, string>();
                foreach (var food in relevantFoods)
                {
                    if ((food.Ingredients == null || food.Ingredients.Count == 0) && !string.IsNullOrEmpty(food.RecipeUrl))
                    {
                        var httpClient = new HttpClient();
                        var url = "https://api.openai.com/v1/responses";

                        var requestBody = new
                        {
                            model = "gpt-4.1",
                            input = "Give me an ingredient list based on the recipe at \"" + food.RecipeUrl + "\". Make sure to include all ingredients and measurements. Answer ONLY with a json object with ingredients in swedish as keys and the amount in metric units as values. The response must be a parsable json without line breaks."
                        };

                        var json = JsonConvert.SerializeObject(requestBody);

                        var request = new HttpRequestMessage(HttpMethod.Post, url);
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "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");
                        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                        var response = httpClient.SendAsync(request).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            var responseContent = response.Content.ReadAsStringAsync().Result;
                            var result = JsonConvert.DeserializeObject<Root>(responseContent);

                            var text = result?.Output?[0]?.Content?[0]?.Text?.Replace("```json", "")?.Replace("\n", "");
                            if (text != null)
                            {
                                var parsed = JsonConvert.DeserializeObject<Dictionary<string, string>>(text);
                                food.Ingredients = parsed.SelectMany(x => (List<string>)[x.Key, x.Value]).ToList();
                                foodRepo.SaveValue(food);
                            }
                        }
                    }

                    if (food.Ingredients?.Any() == true)
                    {
                        for (var i = 0; i < food.Ingredients.Count; i+=2)
                        {
                            var ingredient = food.Ingredients[i].Trim();
                            var amount = food.Ingredients[i + 1].Trim();
                            if (allIngredients.ContainsKey(ingredient))
                            {
                                allIngredients[ingredient] = allIngredients[ingredient] + " + " + amount;
                            } else
                            {
                                allIngredients[ingredient] = amount;
                            }
                        }
                    }
                }

                return new JsonResult(new { list = string.Join('\n', allIngredients.Select(x => x.Key + ": " + x.Value)) });
            }*/

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

            if (!string.IsNullOrEmpty(transactions))
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
                        valuesToSave.Add(spending);
                    }
                }

                foreach (var val in valuesToSave)
                {
                    while (existingTransactions.Any(x => x.CollidesWith(val)))
                    {
                        val.Amount += 1;
                    }
                }

                dbContext.AddRange(valuesToSave);
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

            if (action == "ImageUrl")
            {
                var pair = ImageRepository.Instance.GetImageUrl();
                return new JsonResult(new { url = pair.Item1, taken = pair.Item2 });
            }

            var redirectResult = GetPotentialClientRedirectResult(true, true, "/Index");
            if (redirectResult != null)
            {
                return redirectResult;
            }

            if (action == "Flower")
            {
                return Utils.CreateClientResult(new { streak = SettingsRepositoryTemp.FlowerChore.Update() });
            }
            else if (action == "Floss")
            {
                return Utils.CreateClientResult(new { streak = SettingsRepositoryTemp.FlossChore.Update() });
            }
            else if (action == "FlossJens")
            {
                return Utils.CreateClientResult(new { streak = SettingsRepositoryTemp.FlossChoreJens.Update() });
            }
            else if (action == "Bed")
            {
                return Utils.CreateClientResult(new { streak = SettingsRepositoryTemp.BedSheetChore.Update() });
            }
            else if (action == "Eye")
            {
                return Utils.CreateClientResult(new { streak = SettingsRepositoryTemp.EyeChore.Update() });
            }
            else if (action == "Sink")
            {
                return Utils.CreateClientResult(new { streak = SettingsRepositoryTemp.SinkChore.Update() });
            }
            else if (action == "Workout")
            {
                return Utils.CreateClientResult(new { streak = SettingsRepositoryTemp.WorkoutChore.Update() });
            }

            return Utils.CreateErrorClientResult(null);
        }

        public class Transaction
        {
            public string date { get; set; }

            public string place { get; set; }

            public int amount { get; set; }
        }

        public class Root
        {
            public List<OutputItem> Output { get; set; }
        }

        public class OutputItem
        {
            public List<ContentItem> Content { get; set; }
        }

        public class ContentItem
        {
            public string Text { get; set; }
        }
    }
}
