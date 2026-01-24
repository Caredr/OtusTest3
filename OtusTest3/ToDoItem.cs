using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusTest3
{
    internal class ToDoItem
    {
        public ToDoItem( ToDoUser user, string name)
        {
            Name = name;
            User = user;
            CreatedAt = DateTime.UtcNow;
            State = ToDoItemState.Active;
            Id = Guid.NewGuid();
        }
        public Guid Id { get; private set; }
        public ToDoUser User { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public ToDoItemState State { get; set; }
        public DateTime? StateChangedAt { get; set; }
    }
}
