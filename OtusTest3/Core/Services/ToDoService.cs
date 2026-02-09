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
    internal class ToDoService 
    {
        private readonly IToDoRepository _toDoRepository;
        public ToDoService(IToDoRepository toDoRepository)
        {
            _toDoRepository = toDoRepository;
        }
        public readonly int TaskCountLimit = 100;
        public readonly int TaskLengthLimitMax = 100;
        public readonly int TaskLengthLimitMin = 3;

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
