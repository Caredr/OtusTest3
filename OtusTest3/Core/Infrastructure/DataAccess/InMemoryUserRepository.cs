using OtusTest3.Core.DataAccess;
using OtusTest3.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusTest3.Core.Infrastructure.DataAccess
{
    internal class InMemoryUserRepository: IUserRepository
    {
        private readonly List<ToDoUser> _toDoUserList = [];
        public ToDoUser? GetUser(Guid userId)
        {
            foreach (var user in _toDoUserList)
            {
                if (user.UserId == userId)
                {
                    return user;
                }
            }
            return null;
        }
        public ToDoUser? GetUserByTelegramUserId(long telegramUserId)
        {
            foreach (var user in _toDoUserList)
            {
                if (user.TelegramUserId == telegramUserId)
                {
                    return user;
                }
            }
            return null;
        }
        public void Add(ToDoUser user)
        {
            _toDoUserList.Add(user);
        }
    }
}
