using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusTest3.Core.Entities
{
    public class ToDoUser // это сущность (entity) для пользователя в ToDo-приложении. 
                          // Хранит информацию о пользователе, включая его уникальный идентификатор, имя пользователя в Telegram, дату регистрации и идентификатор пользователя в Telegram. 
                          // Эта информация может использоваться для управления доступом к задачам и спискам задач, а также для персонализации опыта пользователя.
    {
        public ToDoUser() // Конструктор - данные по умолчанию, или чтобы не забыть
        {
                UserId = Guid.NewGuid(); // Генерируем уникальный идентификатор для пользователя
            TelegramUserName = "TgShablon"; //  Устанавливаем имя пользователя по умолчанию
            RegisteredAt = DateTime.UtcNow; // Устанавливаем дату регистрации на текущее время
            TelegramUserId = 111; // Устанавливаем идентификатор пользователя в Telegram по умолчанию
        }
        public Guid UserId { get;  set; } // Уникальный идентификатор пользователя (GUID)
        public string TelegramUserName { get; set; } // Имя пользователя в Telegram (опционально, может быть пустым)
        public DateTime RegisteredAt { get; private set; }
        public long TelegramUserId { get; set; } // Идентификатор пользователя в Telegram (опционально, может быть 0 или отрицательным, если не привязан к Telegram)
    }
}
