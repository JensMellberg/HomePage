using HomePage.Data;
using HomePage.Model;
using Microsoft.AspNetCore.Mvc;

namespace HomePage.Pages
{
    [IgnoreAntiforgeryToken]
    [RequireAdmin]
    public class CreateToDoModel(AppDbContext dbContext, SignInRepository signInRepository) : BasePage(signInRepository)
    {
        public ToDoItem ToDo { get; set; }
        public IActionResult OnGet(Guid id)
        {
            if (id == Guid.Empty)
            {
                ToDo = new ToDoItem();
            }
            else
            {
                ToDo = dbContext.ToDo.Single(x => x.Key == id);
            }

            return Page();
        }

        public IActionResult OnPost(Guid id, string name)
        {
            var toDo = new ToDoItem { Key = id, Name = name };
            dbContext.ToDo.Add(toDo);
            dbContext.SaveChanges();

            return Redirect($"/ToDo");
        }
    }
}
