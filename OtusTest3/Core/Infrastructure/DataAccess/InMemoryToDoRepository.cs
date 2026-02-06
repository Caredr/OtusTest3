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
        public List<ToDoItem> IToDoItemList
        {
            get { return _toDoItemList; }
        }
        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            var readTasks = new List<ToDoItem>();
            foreach (var user in _toDoItemList)
            {
                if (user.Id == userId)
                {
                    readTasks.Add(user);
                }
            }
            return readTasks;
        }
        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            var readTasks = new List<ToDoItem>();
            foreach (var user in _toDoItemList)
            {
                if (user.Id == userId && user.State == ToDoItemState.Active)
                {
                    readTasks.Add(user);
                }
            }
            return readTasks;
        }
        public ToDoItem? Get(Guid id)
        {
            foreach (var item in _toDoItemList)
            {
                if (item.Id == id)
                {
                    return item;
                }
            }
            return null;
        }
        public void Add(ToDoItem item)
        {
            _toDoItemList.Add(item);
        }
        public void Update(ToDoItem item)
        {
            foreach (var itemUpdated in _toDoItemList)
            {
                if(itemUpdated == item)
                {
                    
                }
            }
        }
        public void Delete(Guid id)
        {
            foreach (var item in _toDoItemList)
            {
                if (item.Id == id)
                {
                    _toDoItemList.Remove(item);
                    break;
                }
            }
        }
        public bool ExistsByName(Guid userId, string name)
        {
            foreach (var item in _toDoItemList)
            {
                if (item.User.UserId == userId && item.Name == name)
                {
                    return true;
                }
            }
            return false;
        }
        public int CountActive(Guid userId)
        {
            int count = 0;
            foreach (var item in _toDoItemList)
            {
                if (item.User.UserId == userId && item.State == ToDoItemState.Active)
                {
                    count++;
                }
            }
            return count;
        }
        public IReadOnlyList<ToDoItem> Find(Guid userId, Func<ToDoItem, bool> predicate)
        {
            var result = new List<ToDoItem>();
            foreach (var item in _toDoItemList)
            {
                if (item.Id == userId && predicate(item))
                {
                    result.Add(item);
                }
            }
            return result;
        }
        
    }
               
           
}
