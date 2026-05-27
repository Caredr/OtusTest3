using OtusTest3.Core.DataAccess;
using OtusTest3.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OtusTest3.Core.Infrastructure.DataAccess
{
    internal class FileToDoListRepository: IToDoListRepository // Класс для работы с Репозиторием
    {
        private readonly string _basePath; // папка для хранения файлов (создается автоматически)
        private readonly JsonSerializerOptions _options = new() { WriteIndented = true }; //настройки сериализации (отступы для читаемости)
        public FileToDoListRepository(string basePath)
        {
            _basePath = basePath;  // Базовая папка из конструктора
            Directory.CreateDirectory(_basePath); //Создание директории с фалами (Где они создаются?)
        }
        public Task<string> GetFilePath(Guid Id) => Task.FromResult(Path.Combine(_basePath, $"{Id}.json")); //Возвращает путь./data/{ Id}.json
        public async Task<ToDoList?> LoadListAsync(Guid Id, CancellationToken ct) // Загружает лист со списком фалов репозитория
        {
            string path = await GetFilePath(Id);
            if (!File.Exists(path)) return null;
            await using var stream = File.OpenRead(path);
            return await JsonSerializer.DeserializeAsync<ToDoList>(stream, _options, ct);
        }
        public async Task SaveListAsync(ToDoList list, CancellationToken ct) // Записывает ToDoList → ./data/{list.Id}.json
        {
            string path = await GetFilePath(list.Id);
            await using var stream = File.Create(path);
            await JsonSerializer.SerializeAsync(stream, list, _options, ct);
        }
        public async Task<ToDoList?> Get(Guid id, CancellationToken ct) // Проверяет File.Exists вызывает LoadListAsync
        {
            if(!File.Exists(await GetFilePath(id)))
                return null;
            return await LoadListAsync(id, ct);
        }
        public async Task<IReadOnlyList<ToDoList>?> GetByUserId(Guid userId, CancellationToken ct) //O(N) — сканирует все файлы (плохо для масштаба)
        {
            var result = new List<ToDoList>();
            var files = Directory.GetFiles(_basePath, "*.json"); // Directory.GetFiles("*.json") — находит ВСЕ файлы
            foreach (string file in files) 
            {
                ct.ThrowIfCancellationRequested(); // ct.ThrowIfCancellationRequested() — проверка отмены
                if (Guid.TryParse(Path.GetFileNameWithoutExtension(file), out Guid id)) // Парсит Guid из имени файла
                {
                    ToDoList? list = await LoadListAsync(id, ct); //Загружает ToDoList
                    if (list?.User?.UserId == userId) //Проверяет list.User.UserId == userId
                        result.Add(list); //Добавляет в результат
                }
            }
            return result;
        }
        public async Task Add(ToDoList list, CancellationToken ct)  //
        {
            ArgumentNullException.ThrowIfNull(list); //Валидация 
            if (list?.User != null && list.User.UserId == Guid.Empty) //Автогенерация UserId если Guid.Empty
                list.User.UserId = Guid.NewGuid();
            await SaveListAsync(list, ct); //Сохраняет через SaveListAsync
        }
        public async Task Delete(Guid id, CancellationToken ct)
        {
            string path = await GetFilePath(id); //File.Delete — удаляет ./data/{id}.json
            if (File.Exists(path)) // Безопасно: если файла нет — игнорирует
                File.Delete(path);
        }
        public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(name);  
            var lists = await GetByUserId(userId, ct); // GetByUserId(userId) — загружает ВСЕ списки пользователя
            foreach (var list in lists) // foreach список: проверяет Name (без учета регистра)
            {
                if (string.Equals(list.Name, name, StringComparison.OrdinalIgnoreCase))
                    return true; //  Возвращает true если найден
            }
            return false;
        }
    }
}
