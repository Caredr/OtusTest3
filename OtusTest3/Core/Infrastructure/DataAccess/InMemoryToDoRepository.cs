using OtusTest3.Core.DataAccess;
using OtusTest3.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusTest3.Core.Infrastructure.DataAccess
{
    internal class InMemoryToDoRepository : IToDoRepository
    {
        private readonly List<ToDoItem> _toDoItemList = [];
        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken ct)
        {
            var readTasks = new List<ToDoItem>();
            foreach (var user in _toDoItemList)
            {
                if (user.Id == userId)
                {
                    readTasks.Add(user);
                }
            }
            return await Task.FromResult(readTasks);
        }
        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken ct)
        {
            var readTasks = new List<ToDoItem>();
            foreach (var user in _toDoItemList)
            {
                if (user.Id == userId && user.State == ToDoItemState.Active)
                {
                    readTasks.Add(user);
                }
            }
            return await Task.FromResult(readTasks);
        }
        public async Task<ToDoItem?> Get(Guid id, CancellationToken ct)
        {
            foreach (var item in _toDoItemList)
            {
                if (item.Id == id)
                {
                    return await Task.FromResult(item);
                }
            }
            return null;
        }
        public Task Add(ToDoItem item, CancellationToken ct)
        {
            _toDoItemList.Add(item);
            return Task.CompletedTask;
        }
        public async Task Update(ToDoItem item, CancellationToken ct)
        {
            for(int i = 0; i < _toDoItemList.Count; i++)
            {
                if (_toDoItemList[i].Id == item.Id)
                {
                    await Task.FromResult(_toDoItemList[i] = item);
                }
            }
        }
        public async Task Delete(Guid id, CancellationToken ct)
        {
            foreach (var item in _toDoItemList)
            {
                if (item.Id == id)
                {
                    await Task.FromResult(_toDoItemList.Remove(item));
                    break;
                }
            }
        }
        public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
        {
            foreach (var item in _toDoItemList)
            {
                if (item.User.UserId == userId && item.Name == name)
                {
                    return await Task.FromResult(true);
                }
            }
            return await Task.FromResult(false);
        }
        public async Task<int> CountActive(Guid userId, CancellationToken ct)
        {
            int count = 0;
            foreach (var item in _toDoItemList)
            {
                if (item.User.UserId == userId && item.State == ToDoItemState.Active)
                {
                    count++;
                }
            }
            return await Task.FromResult(count);
        }
        public async Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
        {
            var result = new List<ToDoItem>();
            foreach (var item in _toDoItemList)
            {
                if (item.Id == userId && predicate(item))
                {
                    result.Add(item);
                }
            }
            return await Task.FromResult(result);
        }
        
    }
               
           
}
