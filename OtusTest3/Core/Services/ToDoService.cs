using OtusTest3.Core.DataAccess;
using OtusTest3.Core.Entities;
using OtusTest3.Core.Exeptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusTest3.Core.Services
{
    internal class ToDoService : IToDoService
    {
        private readonly IToDoRepository _iToDoRepository;
        public ToDoService(IToDoRepository toDoRepository)
        {
            _iToDoRepository = toDoRepository;
        }
        public readonly int TaskCountLimit = 100;
        public readonly int TaskLengthLimitMax = 100;
        public readonly int TaskLengthLimitMin = 3;

        public async Task<ToDoItem> Add(ToDoUser botUser, string name, CancellationToken ct)
        {
            int spaceChecker = name.IndexOf(' ');
            string taskName;
            if (spaceChecker > -1 && spaceChecker < name.Length - 1)
            //Если есть пробел и пробел не послений символ
            {
                taskName = name[spaceChecker++..];
                if (taskName.Length > TaskLengthLimitMax) throw new TaskLengthLimitException(taskName.Length, TaskLengthLimitMax);
                //Берется подстрока от символа послепробела и до конца строки.
                ParseAndValidateInt(taskName, TaskLengthLimitMin, TaskLengthLimitMax);
                ToDoItem newTask = new(botUser, taskName);
                await _iToDoRepository.Add(newTask, ct);
                return await Task.FromResult(newTask);
            }
            else
            {
                throw new NullReferenceException();
            }
        }
        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken ct)
        {
            return await _iToDoRepository.GetActiveByUserId(userId, ct);
        }
        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken ct)
        {
            return await _iToDoRepository.GetActiveByUserId(userId, ct); 
        }
        public async Task MarkCompleted(Guid id, CancellationToken ct)
        {
            var item = await _iToDoRepository.Get(id, ct);
            if (item != null)
            {
                item.State = ToDoItemState.Completed;
            }
                else throw new TaskDoesNotExistException("Задача с таким GUID не существует");
        }
        public async Task Delete(Guid id, CancellationToken ct)
        {
            await _iToDoRepository.GetActiveByUserId(id, ct);
        }

        public async Task<IReadOnlyList<ToDoItem>> Find(ToDoUser user, string namePrefix, CancellationToken ct)
        {
            return await _iToDoRepository.Find(user.UserId, item =>
       item.Name?.StartsWith(namePrefix, StringComparison.OrdinalIgnoreCase) == true, ct);

        }

        public void CountAdd()
        {
            Console.WriteLine("Введите максимальное количество задач"); //1
            string tasksCountstext = Console.ReadLine() ?? "Ошибка";   //2
            TasksLimit(tasksCountstext);

        }
        private static void ParseAndValidateInt(string? str, int min, int max)
        {
            str = ValidateString(str);
            int taskTextLenght = Translator(str);
            Validate(taskTextLenght, min, max);
        }
        #region CustomThrows
        public int TasksLimit(string limit)
        {
            int taskCount = int.TryParse(limit, out int result) ? result : 0;
            if (taskCount <= 0 || taskCount > 100)
            {
                throw new ArgumentException("число должно быть больше 0-я и  меньше 100");
            }
            return taskCount;
        }
        private int TestTo(string stringToTest)
        {
            if (!int.TryParse(stringToTest, out int taskTextLenght))
            {
                throw new ArgumentException("Нельзя превратить текст в число");
            }
            return taskTextLenght;
        }
        private static string ValidateString(string stringToTest)
        {
            if (string.IsNullOrWhiteSpace(stringToTest))
            {
                throw new ArgumentException("Строка не может быть пустой");
            }
            return stringToTest;
        }
        private static void Validate(int lenghtToTest, int minLenght, int maxLenght)
        {
            if (lenghtToTest < minLenght || lenghtToTest > maxLenght)
            {
                throw new ArgumentException("слишком короткое название или слишком длинное");
            }
        }
        public static int Translator(string stringToTest)
        {
            int taskTextLenght = stringToTest.Length;
            return taskTextLenght;
        }
        #endregion
    }
}
