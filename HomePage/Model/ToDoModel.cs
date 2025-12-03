

using System.ComponentModel.DataAnnotations;

namespace HomePage.Model
{
    public class ToDoItem
    {
        [Key]
        public Guid Key { get; set; } = Guid.NewGuid();

        [MaxLength(100)]
        [SaveProperty]
        public string Name { get; set; }

        [SaveProperty]
        public bool IsCompleted { get; set; }
    }
}
