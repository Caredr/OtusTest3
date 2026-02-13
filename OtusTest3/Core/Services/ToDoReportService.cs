using OtusTest3.Core.DataAccess;
using OtusTest3.Core.Infrastructure.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusTest3.Core.Services
{
    internal class ToDoReportService : IToDoReportService
    {
        private readonly IToDoRepository _iToDoRepository;
        public ToDoReportService(IToDoRepository inMemoryToDoRepository)
        {
            _iToDoRepository = inMemoryToDoRepository;
        }
        public (int total, int completed, int active, DateTime generatedAt)  GetUserStats(Guid userId)
        {
            int total = 0;
            int completed = 0;
            int active = 0;
            DateTime generatedAt;
            total = _iToDoRepository.GetActiveByUserId(userId).Count;
            active = _iToDoRepository.GetActiveByUserId(userId).Count;
            completed = total - active;
            generatedAt = DateTime.UtcNow;
            return (total, completed, active, generatedAt);
        }
    }
}
