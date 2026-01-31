using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusTest3
{
    internal class ToDoService : IToDoService
    {
        private IUserService _userService;
        public readonly int TaskCountLimit = 100;
        public readonly int TaskLengthLimitMax = 100;
        public readonly int TaskLengthLimitMin = 3;
        private readonly List<ToDoItem> _tasks = [];
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
                foreach (var task in _tasks)
                {
                    if (task.Name == taskName)
                    {
                        throw new DuplicateTaskException(taskName);
                    }
                }
                _tasks.Add(newTask);
                if (_tasks.Count > TaskCountLimit) throw new TaskCountLimitException(TaskCountLimit);
                return newTask;
            }
            else
            {
                throw new NullReferenceException();
            }
        }
        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            foreach(var task in _tasks)
            {
                if(task.Id == userId)
                {
                    _tasks.Add(task);
                }
            }
            return _tasks;
        }
        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            foreach (var task in _tasks)
            {
                if (task.Id == userId && task.State == ToDoItemState.Active)
                {
                    _tasks.Add(task);
                }
            }
            return _tasks;
        }
        public void MarkCompleted(Guid id)
        {
            foreach (var task in _tasks)
            {
                if (task.Id == id)
                {
                    task.State = ToDoItemState.Completed;
                }
                else throw new TaskDoesNotExistException("Задача с таким GUID не существует");
            }
        }
        public void Delete(Guid id)
        {
            foreach (var task in _tasks)
            {
                if (task.Id == id)
                {
                    _tasks.Remove(task);
                }
                else throw new TaskDoesNotExistException("Задача с таким GUID не существует");
            }
        }
        public void StartPanel(ITelegramBotClient botClient, Update update)
        {
            var user = _userService.GetUser(update.Message.From.Id);
            if (user == null)
            {
                _userService.RegisterUser(update.Message.From.Id, update.Message.From.Username);
            }
        }


        public bool ExitPanel(out bool appState, Update update)
        {
            Console.WriteLine(update.Message.From.Username + " Нажмите любую кнопку, чтобы выйти");
            appState = false;
            return appState;
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
