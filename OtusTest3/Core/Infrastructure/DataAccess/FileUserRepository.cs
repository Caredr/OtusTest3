using OtusTest3.Core.DataAccess;
using OtusTest3.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OtusTest3.Core.Infrastructure.DataAccess
{
    internal class FileUserRepository : IUserRepository
    {
        private readonly string _basePath;
        private readonly JsonSerializerOptions _options = new() { WriteIndented = true };
        public FileUserRepository(string basePath)
        {
            _basePath = basePath;  // Базовая папка из конструктора
            Directory.CreateDirectory(_basePath);
        }
        private string GetFilePath(Guid userId) => Path.Combine(_basePath, $"{userId}.json");
        private async Task<ToDoUser?> LoadUserAsync(Guid userId, CancellationToken ct)
        {
            string path = GetFilePath(userId);
            if (!File.Exists(path)) return null;
            await using var stream = File.OpenRead(path);
            return await JsonSerializer.DeserializeAsync<ToDoUser>(stream, _options, ct);
        }
        private async Task SaveUserAsync(ToDoUser user, CancellationToken ct)
        {
            string path = GetFilePath(user.UserId);
            await using var stream = File.Create(path);
            await JsonSerializer.SerializeAsync(stream, user, _options, ct);
        }
        public async Task<ToDoUser?> GetUser(Guid userId, CancellationToken ct)
        {
            return await LoadUserAsync(userId, ct);
        }

        public async Task<ToDoUser?> GetUserByTelegramUserId(long telegramUserId, CancellationToken ct)
        {
            // Сканируем все файлы пользователей (мало файлов, приемлемо)
            var files = Directory.GetFiles(_basePath, "*.json");
            foreach (string file in files)
            {
                ct.ThrowIfCancellationRequested();
                if (Guid.TryParse(Path.GetFileNameWithoutExtension(file), out Guid id))
                {
                    var user = await LoadUserAsync(id, ct);
                    if (user?.TelegramUserId == telegramUserId)
                        return user;
                }
            }
            return null;
        }

        public async Task Add(ToDoUser user, CancellationToken ct)
        {
            if (user.UserId == Guid.Empty) 
                user.UserId = Guid.NewGuid();
            await SaveUserAsync(user, ct);
        }
    }
}

