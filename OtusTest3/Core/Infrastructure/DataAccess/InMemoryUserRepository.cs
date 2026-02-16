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
        public async Task<ToDoUser?> GetUser(Guid userId, CancellationToken ct)
        {
            foreach (var user in _toDoUserList)
            {
                if (user.UserId == userId)
                {
                    return await Task.FromResult(user);
                }
            }
            return  null;
        }
        public async Task<ToDoUser?> GetUserByTelegramUserId(long telegramUserId, CancellationToken ct)
        {
            foreach (var user in _toDoUserList)
            {
                if (user.TelegramUserId == telegramUserId)
                {
                    return await Task.FromResult(user);
                }
            }
            return null;
        }
        public  Task Add(ToDoUser user, CancellationToken ct)
        {
             _toDoUserList.Add(user);
            return Task.CompletedTask;
        }
    }
}
