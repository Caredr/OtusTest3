using OtusTest3.Core.DataAccess;
using OtusTest3.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusTest3.Core.Services
{
    internal class ToDoListService : IToDoListService
    {
        private readonly Dictionary<Guid, ToDoList> _lists = new();
        private const int maxNameLength = 10;

        private readonly IToDoService _todoService;

        public ToDoListService(IToDoService todoService)
        {
            _todoService = todoService ?? throw new ArgumentNullException(nameof(todoService));
        }

        public async Task<ToDoList> AddAsync(ToDoUser user, string name, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Имя не должно отсутствовать", nameof(name));

            if (name.Length > maxNameLength)
                throw new ArgumentException($"Имя не должно быть больше {maxNameLength} букв.", nameof(name));

            var existing = _lists.Values
                .FirstOrDefault(l => l.UserId == user.UserId && string.Equals(l.Name, name, StringComparison.OrdinalIgnoreCase));

            if (existing is not null)
                throw new ArgumentException($"Лист с таким именем '{name}' уже существует для пользователя '{user.UserId}'.", nameof(name));

            var list = new ToDoList(Guid.NewGuid(), name, user.UserId);
            await Task.Run(() => _lists[list.Id] = list, ct);
            return list;
        }

        public async Task<ToDoList?> GetAsync(Guid id, CancellationToken ct)
        {
            return await Task.Run(() =>
            {
                _lists.TryGetValue(id, out var list);
                return list;
            }, ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct)
        {
            await Task.Run(() => _lists.Remove(id), ct);
        }

        public async Task<IReadOnlyList<ToDoList>> GetUserListsAsync(Guid userId, CancellationToken ct)
        {
            return await Task.Run(() =>
            {
                var lists = _lists.Values
                    .Where(l => l.UserId == userId)
                    .ToList();

                return (IReadOnlyList<ToDoList>)lists;
            }, ct);
        }

        /// <summary>
        /// Возвращает задачи пользователя из указанного списка (или "без списка", если listId == null).
        /// Делегирует в IToDoService.GetByUserIdAndList.
        /// </summary>
        public async Task<IReadOnlyList<ToDoItem>> GetByUserIdAndListAsync(Guid userId, Guid? listId, CancellationToken ct)
        {
            return await _todoService.GetByUserIdAndList(userId, listId, ct);
        }
    }
}
