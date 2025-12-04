using HomePage.Data;
using HomePage.Model;
using Microsoft.AspNetCore.Mvc;

namespace HomePage.Pages
{
    [IgnoreAntiforgeryToken]
    public class ToDoModel(AppDbContext dbContext, SignInRepository signInRepository) : BasePage(signInRepository)
    {
        public List<ToDoItem> ToDos { get; set; }
        public void OnGet()
        {
            //BigMigrator.Migrate7(dbContext);
            ToDos = dbContext.ToDo.OrderBy(x => x.IsCompleted).ThenBy(x => x.Name).ToList();
        }

        public IActionResult OnPost(Guid itemId)
        {
            if (!IsAdmin)
            {
                return Redirect("/Login");
            }

            var item = dbContext.ToDo.Single(x => x.Key == itemId);
            item.IsCompleted = true;
            dbContext.SaveChanges();
            return Redirect("ToDo");
        }
    }
}
