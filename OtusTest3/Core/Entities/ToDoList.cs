using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusTest3.Core.Entities
{
    internal class ToDoList // это сущность (entity) для списка задач в ToDo-приложении. 
                            // Хранит информацию о списке задач, включая его название, владельца и дату создания. 
                            // Список может содержать несколько задач (ToDoItem), которые принадлежат этому списку.
    {
        public ToDoList(Guid id, string name, Guid userId)
        {
            // Используем this. чтобы присваивать полям, а не параметрам
            this.Id = id;
            this.Name = name;
            this.UserId = userId;
            this.CreatedAt = DateTime.UtcNow;
        }

        // Пустой конструктор для десериализации JSON
        public ToDoList() { }
        public Guid Id { get; set; } //Уникальный идентификатор списка
        public string? Name { get; set; } //Название списка (опционально, может быть пустым)
        public ToDoUser? User { get; set; } //Владелец списка (опционально, может быть null, если список не привязан к конкретному пользователю)
        public DateTime CreatedAt { get; set; } //Время создания списка (UTC)
        public Guid UserId { get; set; }  // Идентификатор пользователя-владельца списка (опционально, может быть Guid.Empty, если список не привязан к конкретному пользователю)
    }
}
