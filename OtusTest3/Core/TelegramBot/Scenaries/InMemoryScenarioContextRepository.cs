using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusTest3.Core.TelegramBot.Scenaries
{
    internal class InMemoryScenarioContextRepository : IScenarioContextRepository 
    {
        private readonly ConcurrentDictionary<long, ScenarioContext> _storage = new();

        public Task<ScenarioContext?> GetContext(long userId, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            _storage.TryGetValue(userId, out var context);
            return Task.FromResult<ScenarioContext?>(context);
        }
        public Task SetContext(long userId, ScenarioContext context, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            _storage[userId] = context;
            return Task.CompletedTask;
        }
        public Task ResetContext(long userId, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            _storage.Remove(userId, out var context);
            return Task.CompletedTask;
        }
    }
}
