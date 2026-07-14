using LinqToDB;
using LinqToDB.Remote;
using OtusTest3.Core.DataAccess;
using OtusTest3.Core.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OtusTest3.Core.Infrastructure.DataAccess
{
    internal class SqlUserRepository : IUserRepository
    {
        private readonly IDataContextFactory<ToDoDataContext> _factory;

        public SqlUserRepository(IDataContextFactory<ToDoDataContext> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public async Task<ToDoUser?> GetUser(Guid userId, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            var model = await dbContext.ToDoUsers
                .FirstOrDefaultAsync(u => u.UserId == userId, ct);

            return model is null
                ? null
                : ModelMapper.MapFromModel(model);
        }

        public async Task<ToDoUser?> GetUserByTelegramUserId(long telegramUserId, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            var model = await dbContext.ToDoUsers
                .FirstOrDefaultAsync(u => u.TelegramUserId == telegramUserId, ct);

            return model is null
                ? null
                : ModelMapper.MapFromModel(model);
        }

        public async Task Add(ToDoUser user, CancellationToken ct)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            using var dbContext = _factory.CreateDataContext();

            var model = ModelMapper.MapToModel(user);

            await dbContext.InsertAsync(model, token: ct);
        }
    }
}