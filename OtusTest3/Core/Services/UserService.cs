using Otus.ToDoList.ConsoleBot.Types;
using OtusTest3.Core.DataAccess;
using OtusTest3.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusTest3.Core.Services
{
    internal class UserService
    {
        private IUserRepository _iUserRepository;
        public UserService(IUserRepository iUserRepository)
        {
            _iUserRepository = iUserRepository;
        }
        
        
    }
}
