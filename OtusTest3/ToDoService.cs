using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusTest3
{
    internal class ToDoService: IToDoService
    {
        private IUserService _userService;
        public ToDoService(IUserService userService, IToDoService toDoService)
        {
            _userService = userService;
        }
        public ToDoService()
        {
        }
        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            var tasks = new List<ToDoItem>();
            foreach(var task in tasks)
            {
                if(task.Id == userId)
                {
                    tasks.Add(task);
                }
            }
            return tasks;
        }
        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            var tasks = new List<ToDoItem>();
            foreach (var task in tasks)
            {
                if (task.Id == userId && task.State == ToDoItemState.Active)
                {
                    tasks.Add(task);
                }
            }
            return tasks;
        }
        public ToDoItem Add(ToDoUser user, string name)
        {
            ToDoItem task = new(user, name);
            return task;
        }
        public void MarkCompleted(Guid id)
        {
            var tasks = new List<ToDoItem>();
            foreach (var task in tasks)
            {
                if (task.Id == id)
                {
                    task.State = ToDoItemState.Completed;
                }
            }
        }
        public void Delete(Guid id)
        {
            var tasks = new List<ToDoItem>();
            foreach (var task in tasks)
            {
                if (task.Id == id)
                {
                    tasks.Remove(task);
                }
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
        public void HelpPanel(string userName)
        {
            Console.WriteLine(" " + userName + " чтобы пользоваться программой" +
                "\n пожалуйста вводите комманды /start, /help, /info, /exit" +
                "\n /start - задает или меняет ваше имя" +
                "\n /help - доска информации" +
                "\n /info - дата создания программы" +
                "\n /addtask - добавить карту" +
                "\n /showtasks - показать список карт со статусом Active" +
                "\n /showalltasks - показать список всех карт" +
                "\n /removetask - убрать карту" +
                "\n /completetask - поставить статус карте - Completed" +
                "\n /exit - выход из программы");
        }
        public void InfoPanel(string userName)
        {
            Console.WriteLine(userName + "версия программы - 0.0.4, дата создания 18.11.2025б " +
                "редактура от 19.01.2026");
        }
        public void CompleteTaskPanel(string commandEater, List<ToDoItem> cardsNames)
        {
            int spaceChecker = commandEater.IndexOf(' ');
            if (spaceChecker > -1 && spaceChecker < commandEater.Length - 1)
            {
                string cardId = commandEater[spaceChecker++..];
                Guid findedCardId = new Guid(cardId);
                if (cardsNames.Count > 0)
                {
                    foreach (var card in cardsNames)
                    {
                        if (card.Id == findedCardId)
                            MarkCompleted(findedCardId);
                        else
                            Console.WriteLine("Нет такой карты c таким Id");
                    }
                }
                else
                    Console.WriteLine("Список пуст");
            }
            else
                Console.WriteLine("Ошибка");
        }
        public void AddCardPanel(ToDoUser botUser, List<ToDoItem> cardsNames,
            int taskCounts, int taskCountLimitMax, int taskCountLimitMin, string commandEater)
        {
            int spaceChecker = commandEater.IndexOf(' ');
            if (spaceChecker > -1 && spaceChecker < commandEater.Length - 1)
            //Если есть пробел и пробел не послений символ
            {
                string cardName = commandEater[spaceChecker++..];
                    //Берется подстрока от символа послепробела и до конца строки.
                bool check = true;
                while (check)
                {
                    if (cardName == "Ошибка1111")
                    {
                        Console.WriteLine($"Ошибка, введите название карты");
                        continue;
                    }
                    check = false;
                    ToDoItem newCard = new(botUser, cardName);
                    Console.WriteLine($"Карта {cardName} добавленна");
                    foreach (var card in cardsNames)
                    {
                        if (card.Name == cardName)
                        {
                            Console.WriteLine("такое название уже существует");
                            throw new DuplicateTaskException(cardName);
                        }
                    }
                    cardsNames.Add(newCard);

                }
                if (cardsNames.Count > taskCounts)
                {
                    throw new TaskCountLimitException(taskCounts);
                }
                if (cardName.Length > taskCountLimitMax)
                {
                    throw new TaskLengthLimitException(cardName.Length, taskCountLimitMax);
                }
                ParseAndValidateInt(cardName, taskCountLimitMin, taskCountLimitMax);
            }
            else
                Console.WriteLine("Ошибка");
        }
        public void ShowCardPanel(List<ToDoItem> cardsNames)
        {
            if (cardsNames.Count > 0)
            {
                int cardIndex = 1;
                foreach (ToDoItem card in cardsNames)
                {
                    if (card.State == ToDoItemState.Active)
                    {
                        Console.WriteLine($"{cardIndex} {card.Name} {card.CreatedAt} {card.Id}");
                        cardIndex++;
                    }
                }
            }
            else Console.WriteLine("Список пуст");
        }
        public void ShowAllCardsPanel(List<ToDoItem> cardsNames)
        {
            if (cardsNames.Count > 0)
            {
                int cardIndex = 1;
                foreach (ToDoItem card in cardsNames)
                {
                    Console.WriteLine($"{cardIndex}{card.State}{card.Name} {card.CreatedAt} {card.Id}");
                    cardIndex++;
                }
            }
            else Console.WriteLine("Список пуст");
        }
        public void RemoveCardPanel(List<ToDoItem> cardsNames, string commandEater)
        {
            int spaceChecker = commandEater.IndexOf(' ');
            if (spaceChecker > -1 && spaceChecker < commandEater.Length - 1)
            {
                string cardNameToDelete = commandEater[spaceChecker++..];
                bool check = true;
                if (cardsNames.Count > 0)
                {
                    while (check)
                    {
                        if (cardNameToDelete == "/back")
                        {
                            Console.WriteLine("вы вышли из меню удаления");
                            check = false;
                        }
                        else
                        {
                            TestTo(cardNameToDelete);
                            if (int.TryParse(cardNameToDelete, out int deletedCardNumber))
                            {
                                Console.WriteLine($"карта {cardsNames[deletedCardNumber - 1]} Удалена");
                                cardsNames.RemoveAt(deletedCardNumber - 1);
                                check = false;
                            }
                            else
                            {
                                Console.WriteLine("Введите номер из списка");
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Список пуст");
                }
            }
            else
              Console.WriteLine("Ошибка");
        }
        public bool ExitPanel(out bool appState, string userName)
        {
            Console.WriteLine(userName + " Нажмите любую кнопку, чтобы выйти");
            appState = false;
            return appState;
        }
        public void CountAdd()
        {
            Console.WriteLine("Введите максимальное количество задач"); //1
            string tasksCountstext = Console.ReadLine() ?? "Ошибка";   //2
           // taskCounts = toDoService.TasksLimit(tasksCountstext);

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
        private static void ParseAndValidateInt(string? str, int min, int max)
        {
            str = ValidateString(str);
            int taskTextLenght = Translator(str);
            Validate(taskTextLenght, min, max);
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
        void IToDoService.Delete(Guid id)
        {
            Delete(id);
        }
        #endregion
    }
}
