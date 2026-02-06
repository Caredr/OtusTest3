using Otus.ToDoList.ConsoleBot.Types;
using OtusTest3.Core.DataAccess;
using OtusTest3.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusTest3.Core.Services
{
    internal class UserService: IUserService
    {
        private IUserRepository _iUserRepository;
        public UserService(IUserRepository iUserRepository)
        {
            _iUserRepository = iUserRepository;
        }

        private readonly List<ToDoUser> _toDoUserList = [];
        public ToDoUser? GetUser(long telegramUserId)
        {
            foreach(var user in _toDoUserList)
            {
                if(user.TelegramUserId == telegramUserId)
                {
                    return user;
                }
            }
            return null;
        }
        public ToDoUser RegisterUser(long telegramUserId, string telegramUserName)
        {
            ArgumentNullException.ThrowIfNull(telegramUserName, nameof(telegramUserName));
            var userСurrent = new ToDoUser
            {
                TelegramUserId = telegramUserId,
                TelegramUserName = telegramUserName
            };
            _toDoUserList.Add(userСurrent);
            return userСurrent;
        }
    }
}
