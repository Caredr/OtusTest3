using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusTest3.Core.Entities
{
    internal class ToDoItem // это сущность (entity) для отдельной задачи в ToDo-приложении.
                            // Хранит полную информацию об одной задаче пользователя.
    {
        //Базовая единица данных в системе задач
        //Может существовать самостоятельно или в составе списка (ToDoList)
        //Отслеживает жизненный цикл задачи (создание → выполнение → завершение)
        public ToDoItem( ToDoUser user, string name) //Конструктор - данные по умолчанию, или чтобы не забыть
        {
            Name = name;
            User = user;
            CreatedAt = DateTime.UtcNow;
            State = ToDoItemState.Active;
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; } //Уникальный идентификатор задачи
        public ToDoUser User { get; set; } //Владелец задачи
        public string Name { get; set; } //Название/описание задачи
        public DateTime CreatedAt { get; set; } //Время создания (UTC)
        public ToDoItemState State { get; set; } //Состояние
        public DateTime? StateChangedAt { get; set; } //Когда изменилось состояние
        public DateTime? DeadLine { get; set; } //Срок выполнения (опционально)
        public ToDoList? List { get; set; } //Список, к которому принадлежит (опционально)

    }
}
