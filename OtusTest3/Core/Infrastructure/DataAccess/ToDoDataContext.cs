using LinqToDB;
using LinqToDB.Data;
using OtusTest3.Core.DataAccess.Models;

namespace OtusTest3.Core.Infrastructure.DataAccess;

internal class ToDoDataContext : LinqToDB.Data.DataConnection
{
    public ToDoDataContext(string connectionString)
        : base(ProviderName.PostgreSQL, connectionString)
    {
    }
    public ITable<ToDoUserModel> ToDoUsers => this.GetTable<ToDoUserModel>();
    public ITable<ToDoListModel> ToDoLists => this.GetTable<ToDoListModel>();
    public ITable<ToDoItemModel> ToDoItems => this.GetTable<ToDoItemModel>();
}
