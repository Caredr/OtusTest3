using OtusTest3.Core.DataAccess;
using OtusTest3.Core.Infrastructure.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusTest3.Core.Services
{
    internal class ToDoReportService 
    {
        private IToDoReportService _iToDoReportService;
        public ToDoReportService(IToDoReportService iToDoReportService) 
        {
            iToDoReportService = _iToDoReportService;
        }
        private InMemoryToDoRepository _inMemoryToDoRepository = new();
        public (int total, int completed, int active, DateTime generatedAt)  GetUserStats(Guid userId)
        {
            int total = 0;
            int completed = 0;
            int active = 0;
            DateTime generatedAt;
            total = _inMemoryToDoRepository.GetActiveByUserId(userId).Count;
            active = _inMemoryToDoRepository.GetActiveByUserId(userId).Count;
            completed = total - active;
            generatedAt = DateTime.UtcNow;
            return (total, completed, active, generatedAt);
        }
    }
}
