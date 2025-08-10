using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomePage.Pages
{
    [IgnoreAntiforgeryToken]
    public class ToDoModel : PageModel
    {
        public List<ToDoItem> ToDos { get; set; }
        public void OnGet()
        {
            this.TryLogIn();
            ToDos = new ToDoRepository().GetValues().Values.OrderBy(x => x.IsCompleted).ThenBy(x => x.Name).ToList();
        }

        public IActionResult OnPost(string itemId)
        {
            if (this.ShouldRedirectToLogin())
            {
                return Redirect("/Login");
            }

            var repo = new ToDoRepository();
            var item = repo.TryGetValue(itemId);
            item.IsCompleted = true;
            repo.SaveValue(item);

            return Redirect("ToDo");
        }
    }
}
