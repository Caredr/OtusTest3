using Otus.ToDoList.ConsoleBot.Types;
using OtusTest3.Core.DataAccess;
using OtusTest3.Core.Entities;
using OtusTest3.Core.Infrastructure.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusTest3.Core.Services
{
    internal class UserService : IUserService
    {
        private IUserRepository _iUserRepository;
        public UserService(IUserRepository iUserRepository)
        {
            _iUserRepository = iUserRepository;
        }

        public ToDoUser? GetUser(long telegramUserId)
        {
            return _iUserRepository.GetUserByTelegramUserId(telegramUserId);
        }
        public ToDoUser RegisterUser(long telegramUserId, string telegramUserName)
        {
            ArgumentNullException.ThrowIfNull(telegramUserName, nameof(telegramUserName));
            var userСurrent = new ToDoUser
            {
                TelegramUserId = telegramUserId,
                TelegramUserName = telegramUserName
            };
            _iUserRepository.Add(userСurrent);
            return userСurrent;
        }
    }
}
