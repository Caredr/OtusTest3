using Otus.ToDoList.ConsoleBot.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusTest3
{
    internal class UserService: IUserService
    {
        public ToDoUser? GetUser(long telegramUserId)
        {
            Update update = new();
            telegramUserId = update.Message.From.Id;
            var userСurrent = new ToDoUser();
            {
                userСurrent.TelegramUserId = telegramUserId;
            }
            return userСurrent;
        }
        public ToDoUser RegisterUser(long telegramUserId, string telegramUserName)
        {
            ArgumentNullException.ThrowIfNull(telegramUserName, nameof(telegramUserName));

            Update update = new();
            telegramUserId = update.Message.From.Id;
            telegramUserName = update.Message.From.Username;
            var userСurrent = new ToDoUser();
            {
                userСurrent.TelegramUserId = telegramUserId;
                userСurrent.TelegramUserName = telegramUserName;
            }
            return userСurrent;
        }
    }
}
