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
        private readonly List<ToDoUser> _toDoUserList = [];
        public ToDoUser? GetUser(long telegramUserId)
        {
            var userСurrent = new ToDoUser();
            foreach(var user in _toDoUserList)
            {
                if(user.TelegramUserId == telegramUserId)
                {
                    userСurrent = user;
                }
            }
            return userСurrent;
        }
        public ToDoUser RegisterUser(long telegramUserId, string telegramUserName)
        {
            ArgumentNullException.ThrowIfNull(telegramUserName, nameof(telegramUserName));
            var userСurrent = new ToDoUser();
            {
                userСurrent.TelegramUserId = telegramUserId;
                userСurrent.TelegramUserName = telegramUserName;
            }
            _toDoUserList.Add(userСurrent);
            return userСurrent;
        }
    }
}
