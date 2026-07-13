using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Remote;
using OtusTest3.Core.DataAccess;
using OtusTest3.Core.DataAccess.Models;
using OtusTest3.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusTest3.Core.Infrastructure.DataAccess
{
    public class SqlToDoRepository : IToDoRepository
    {
        private readonly LinqToDB.Remote.IDataContextFactory<ToDoDataContext> _factory;
        private readonly ModelMapper _mapper;

        public SqlToDoRepository(
            IDataContextFactory<ToDoDataContext> factory,
            ModelMapper mapper)
        {
            _factory = factory;
            _mapper = mapper;
        }

        private static IQueryable<ToDoItem> IncludeAll(ToDoDataContext dbContext)
        {
            return dbContext.ToDoItems
                .LoadWith(i => i.User)
                .LoadWith(i => i.List)
                .LoadWith(i => i.List!.User);
        }

        private static IQueryable<ToDoList> IncludeLists(ToDoDataContext dbContext)
        {
            return dbContext.ToDoLists
                .LoadWith(l => l.User);
        }
    }
}
