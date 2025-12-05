using HomePage.Data;
using HomePage.Model;
using Microsoft.AspNetCore.Mvc;

namespace HomePage.Pages
{
    [IgnoreAntiforgeryToken]
    public class ToDoModel(AppDbContext dbContext, SignInRepository signInRepository, DatabaseLogger logger) : BasePage(signInRepository)
    {
        public List<ToDoItem> ToDos { get; set; }
        public void OnGet()
        {
            ToDos = dbContext.ToDo.OrderBy(x => x.IsCompleted).ThenBy(x => x.Name).ToList();
        }

        public IActionResult OnPost(Guid itemId)
        {
            var redirectResult = GetPotentialRedirectResult(true, true);
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var item = dbContext.ToDo.Single(x => x.Key == itemId);
            item.IsCompleted = true;
            logger.Information($"Did ToDo activity {item.Name}!", LoggedInPerson?.UserName);

            dbContext.SaveChanges();
            return Redirect("ToDo");
        }
    }
}
