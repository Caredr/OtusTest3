using OtusTest3.Core.DataAccess;
using OtusTest3.Core.Entities;
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

        public async Task<ToDoUser?> GetUser(long telegramUserId, CancellationToken ct)
        {
            return await _iUserRepository.GetUserByTelegramUserId(telegramUserId, ct);
        }
        public Task<ToDoUser> RegisterUser(long telegramUserId, string telegramUserName, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(telegramUserName, nameof(telegramUserName));
            var userСurrent = new ToDoUser
            {
                TelegramUserId = telegramUserId,
                TelegramUserName = telegramUserName
            };
             _iUserRepository.Add(userСurrent, ct);
            return Task.FromResult(userСurrent);
        }
    }
}
