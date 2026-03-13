using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusTest3.Core.Entities
{
    public class ToDoUser
    {
            public ToDoUser()
            {
                UserId = Guid.NewGuid();
                TelegramUserName = "TgShablon";
                RegisteredAt = DateTime.UtcNow;
                TelegramUserId = 111;
            }
            public Guid UserId { get;  set; }
            public string TelegramUserName { get; set; }
            public DateTime RegisteredAt { get; private set; }
            public long TelegramUserId { get; set; }
    }
}
