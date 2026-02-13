using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;
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

        public ToDoItem Add(ToDoUser botUser, string name)
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
                _iToDoRepository.Add(newTask);
                return newTask;
            }
            else
            {
                throw new NullReferenceException();
            }
        }
        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            return _iToDoRepository.GetActiveByUserId(userId);
        }
        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            return  _iToDoRepository.GetActiveByUserId(userId); 
        }
        public void MarkCompleted(Guid id)
        {
            var item = _iToDoRepository.Get(id);
            if (item != null)
            {
                item.State = ToDoItemState.Completed;
            }
                else throw new TaskDoesNotExistException("Задача с таким GUID не существует");
        }
        public void Delete(Guid id)
        {
            _iToDoRepository.GetActiveByUserId(id);
        }
        public void CountAdd()
        {
            Console.WriteLine("Введите максимальное количество задач"); //1
            string tasksCountstext = Console.ReadLine() ?? "Ошибка";   //2
            TasksLimit(tasksCountstext);

        }
        public IReadOnlyList<ToDoItem> Find(ToDoUser user, string namePrefix)
        {
            return _iToDoRepository.Find(user.UserId, item =>
       item.Name?.StartsWith(namePrefix, StringComparison.OrdinalIgnoreCase) == true);

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
