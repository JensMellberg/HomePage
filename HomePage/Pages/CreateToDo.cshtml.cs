using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomePage.Pages
{
    [IgnoreAntiforgeryToken]
    public class CreateToDoModel : PageModel
    {
        public ToDoItem ToDo { get; set; }
        public IActionResult OnGet(string id)
        {
            this.TryLogIn();
            if (this.ShouldRedirectToLogin())
            {
                return new RedirectResult("/Login");
            }

            if (string.IsNullOrEmpty(id))
            {
                ToDo = new ToDoItem();
            }
            else
            {
                ToDo = new ToDoRepository().TryGetValue(id) ?? new ToDoItem();
            }

            return Page();
        }

        public IActionResult OnPost(string id, string name)
        {
            var toDo = new ToDoItem { Key = id, Name = name };
            new ToDoRepository().SaveValue(toDo);

            return Redirect($"/ToDo");
        }
    }
}
