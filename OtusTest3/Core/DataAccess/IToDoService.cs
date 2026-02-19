using OtusTest3.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusTest3.Core.DataAccess
{
    internal interface IToDoService
    {
        Task<IReadOnlyList<ToDoItem>> Find(ToDoUser user, string namePrefix, CancellationToken ct);
        Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken ct);
        //Возвращает ToDoItem для UserId со статусом Active
        Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken ct);
        Task<ToDoItem> Add(ToDoUser user, string name, CancellationToken ct);
        Task MarkCompleted(Guid id, CancellationToken ct);
        Task Delete(Guid id, CancellationToken ct);
    }
}
