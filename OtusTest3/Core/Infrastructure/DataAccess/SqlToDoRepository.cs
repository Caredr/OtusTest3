using LinqToDB;
using LinqToDB.Remote;
using OtusTest3.Core.DataAccess;
using OtusTest3.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OtusTest3.Core.Infrastructure.DataAccess
{
    public class SqlToDoRepository : IToDoRepository
    {
        private readonly IDataContextFactory<ToDoDataContext> _factory;
        private readonly ModelMapper _mapper;

        public SqlToDoRepository(IDataContextFactory<ToDoDataContext> factory)
        {
            _factory = factory;
            _mapper = new ModelMapper();
        }

        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            var items = await dbContext.ToDoItems
                .LoadWith(i => i.User)
                .LoadWith(i => i.List)
                .LoadWith(i => i.List!.User)
                .Where(i => i.UserId == userId)
                .ToListAsync(ct);

            return items
                .Select(_mapper.Map)
                .ToList();
        }

        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            var items = await dbContext.ToDoItems
                .LoadWith(i => i.User)
                .LoadWith(i => i.List)
                .LoadWith(i => i.List!.User)
                .Where(i => i.UserId == userId && !i.IsCompleted)
                .ToListAsync(ct);

            return items
                .Select(_mapper.Map)
                .ToList();
        }

        public async Task<ToDoItem?> Get(Guid id, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            var item = await dbContext.ToDoItems
                .LoadWith(i => i.User)
                .LoadWith(i => i.List)
                .LoadWith(i => i.List!.User)
                .FirstOrDefaultAsync(i => i.Id == id, ct);

            return item is null ? null : _mapper.Map(item);
        }

        public async Task Add(ToDoItem item, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            var dbItem = _mapper.Map(item);

            await dbContext.InsertAsync(dbItem, token: ct);
        }

        public async Task Update(ToDoItem item, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            var existing = await dbContext.ToDoItems
                .LoadWith(i => i.User)
                .LoadWith(i => i.List)
                .LoadWith(i => i.List!.User)
                .FirstOrDefaultAsync(i => i.Id == item.Id, ct);

            if (existing is null)
                throw new InvalidOperationException($"ToDoItem with id {item.Id} was not found.");

            _mapper.Map(item, existing);

            await dbContext.UpdateAsync(existing, token: ct);
        }

        public async Task Delete(Guid id, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            var rowsAffected = await dbContext.ToDoItems
                .Where(i => i.Id == id)
                .DeleteAsync(ct);

            if (rowsAffected == 0)
                throw new InvalidOperationException($"ToDoItem with id {id} was not found.");
        }

        public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            return await dbContext.ToDoItems
                .LoadWith(i => i.User)
                .LoadWith(i => i.List)
                .LoadWith(i => i.List!.User)
                .AnyAsync(i => i.UserId == userId && i.Name == name, ct);
        }

        public async Task<int> CountActive(Guid userId, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            return await dbContext.ToDoItems
                .LoadWith(i => i.User)
                .LoadWith(i => i.List)
                .LoadWith(i => i.List!.User)
                .CountAsync(i => i.UserId == userId && !i.IsCompleted, ct);
        }

        public async Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            var items = await dbContext.ToDoItems
                .LoadWith(i => i.User)
                .LoadWith(i => i.List)
                .LoadWith(i => i.List!.User)
                .Where(i => i.UserId == userId)
                .ToListAsync(ct);

            return items
                .Select(_mapper.Map)
                .Where(predicate)
                .ToList();
        }
    }
}