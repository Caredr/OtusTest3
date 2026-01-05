namespace OtusTest3
{
    #region CustomExeptions
    public class TaskCountLimitException(int count) : Exception($"Превышенно максимальное количество карт{count}")
    {
    }
    public class TaskLengthLimitException(int taskLength, int taskLengthLimit) : Exception($"Превышенно максимальная длина карты {taskLength}, лимит {taskLengthLimit}")
    {
    }
    public class DuplicateTaskException(string task) : Exception($"Такая {task} уже существует")
    {
    }
    #endregion
    public enum ToDoItemState
    {
        Active,
        Completed
    }
    internal class ToDoItem
    {
        public ToDoItem(string name, ToDoUser user)
        {
            Name = name;
            User = user; 
            CreatedAt = DateTime.UtcNow;
            State = ToDoItemState.Active;
            Id = Guid.NewGuid();
        }
        public Guid Id { get; private set; }
        public ToDoUser User { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public ToDoItemState State { get; set; }
        public DateTime? StateChangedAt { get; set; }
    }
    internal class ToDoUser
    {
        public ToDoUser()
        {
            UserId = Guid.NewGuid();
            TelegramUserName = "TgShablon";
            RegisteredAt = DateTime.UtcNow;
        }
        public Guid UserId { get; private set; }
        public string TelegramUserName { get; set; }
        public DateTime RegisteredAt { get; private set; }
    }
    internal class MyApp 
    {
        ToDoUser botUser = new();
        private List<ToDoItem> cardsNames = [];
        private string commandEater = "";
        private int taskCounts = 0;
        private int taskCountLimitMin = 3;
        private int taskCountLimitMax = 100;
        private string userName = "Пользователь";
        private bool echoAccess = false;  
        public void CountAdd()
        {
            Console.WriteLine("Введите максимальное количество задач"); //1
            string tasksCountstext = Console.ReadLine() ?? "Ошибка";   //2
            taskCounts = TasksLimit(tasksCountstext);
            MenuCaller();
        }
        public void MenuCaller()
        {
            Console.WriteLine("Доступные команды /start, " +
                "/help, /info, /addtask, /showtasks, /removetask,/completetask,/showalltasks, /exit");
            bool isRun = true;
            while (isRun)
            {
                commandEater = Console.ReadLine() ?? "";
                switch (commandEater)
                {
                    case "/start":
                        StartPanel();
                        break;
                    case "/help":
                        HelpPanel();
                        break;
                    case "/info":
                        InfoPanel();
                        break;
                    case "/addtask":
                        AddCardPanel();
                        break;
                    case "/showtasks":
                        ShowCardPanel();
                        break;
                    case "/showalltasks":
                        ShowAllCardsPanel();
                        break;
                    case "/removetask":
                        RemoveCardPanel();
                        break;
                    case string s when s.StartsWith("/echo"):
                        EchoPanel(commandEater);
                        break;
                    case string si when si.StartsWith("/completetask"):
                        CompleteTaskPanel(commandEater);
                        break;
                    case "/exit":
                        isRun = ExitPanel(out isRun);
                        break;
                    default:
                        Console.WriteLine("Ошибка, введите доступную команду");
                        break;
                }
            }
        }
        private string StartPanel()
        {
            Console.WriteLine("Ввелось /start");
            Console.WriteLine("Введите Имя");
            botUser.TelegramUserName = Console.ReadLine() ?? "Пользователь";
            echoAccess = true;
            userName = botUser.TelegramUserName;
            Console.WriteLine("Доступна секретная команда /echo");
            return userName;
        }
        private void HelpPanel()
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
        private void InfoPanel()
        {
            Console.WriteLine(userName + "версия программы - 0.0.4, дата создания 18.11.2025б " +
                "редактура от 16.12.2025");
        }
        private void EchoPanel(string commandEater)
        {
            if (echoAccess)
            {
                int spaceChecker = commandEater.IndexOf(' ');
                //Ищем первый пробел
                //Если пробела нет возвращение -1
                if (spaceChecker > -1 && spaceChecker < this.commandEater.Length - 1)
                //Если есть пробел и пробел не послений символ
                {
                    string echo = commandEater[spaceChecker++..];
                    //Берется подстрока от символа послепробела и до конца строки.
                    Console.WriteLine(echo); //вывод на консоль
                }
                else
                    Console.WriteLine("Ошибка");
            }
            else
                Console.WriteLine("Ошибка, введите доступную команду");
        }
        private void CompleteTaskPanel(string commandEater)
        {
            int spaceChecker = commandEater.IndexOf(' ');
            if (spaceChecker > -1 && spaceChecker < this.commandEater.Length - 1)
            {
                string cardId = commandEater[spaceChecker++..];
                Guid findedCardId = new Guid(cardId);
                if (cardsNames.Count > 0)
                {
                    foreach (var card in cardsNames)
                    {
                        if (card.Id == findedCardId)
                            card.State = ToDoItemState.Completed;
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
        private void AddCardPanel()
        {
            string cardName = "";
            bool check = true;
            while (check)
            {
                Console.WriteLine("Введите название карты:");
                cardName = Console.ReadLine() ?? "Ошибка1111";
                if (cardName == "Ошибка1111")
                {
                    Console.WriteLine($"Ошибка, введите название карты");
                    continue;
                }
                check = false;
                ToDoItem newCard = new(cardName, botUser);
                Console.WriteLine($"Карта {cardName} добавленна");
                foreach (var card in cardsNames)
                {
                    if(card.Name == cardName)
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
            if(cardName.Length > taskCountLimitMax)
            {
                throw new TaskLengthLimitException(cardName.Length, taskCountLimitMax);
            }
            ParseAndValidateInt(cardName, taskCountLimitMin, taskCountLimitMax);
        }
        private void ShowCardPanel()
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
        private void ShowAllCardsPanel()
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
        private void RemoveCardPanel()
        {
            bool check = true;
            if(cardsNames.Count > 0)
            {
                while (check)
                {
                    Console.WriteLine("Введите номер карты для удаления или введите /back," +
                        " чтобы выйти из меню удаления");
                    ShowCardPanel();
                    string cardNameToDelete = Console.ReadLine() ?? "Ошибка";

                    if(cardNameToDelete == "/back")
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
        private bool ExitPanel(out bool appState)
        {
            Console.WriteLine(userName + " Нажмите любую кнопку, чтобы выйти");
            appState = false;
            return appState;
        }
        private static int Translator(string stringToTest)
        {
            int taskTextLenght = stringToTest.Length;
            return taskTextLenght;
        }
        #region CustomThrows
        private static int TasksLimit(string limit)
        {
            int taskCount = int.TryParse(limit, out int result) ? result : 0;
            if (taskCount <= 0 || taskCount > 100)
            {
                throw new ArgumentException("число должно быть больше 0-я и  меньше 100");
            }
            return taskCount;
        }
        private static void ParseAndValidateInt(string? str, int min, int max)
        {
            str = ValidateString(str);
            int taskTextLenght = Translator(str);
            Validate(taskTextLenght, min, max);
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
        #endregion

    }
    internal class Program
    {
        private static void Main(string[] args)
        {
            ArgumentNullException.ThrowIfNull(args);
            MyApp myapp = new();
            //Задаю макс кол-во задач, считываю, идет преобразование.
            try
            {
                myapp.CountAdd();
            }
            // Когда статический конструктор класса падает с ошибкой
            catch (TypeInitializationException ex)
            {
                Console.WriteLine($"Произошла непредвиденная ошибка {ex.Message}");
                Console.WriteLine($"StackTrace:\n{ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner exception: {0}", ex.InnerException);
                }
            }
            //Когда экземпляр не инициализирован, когда не написал new() в MyApp myapp = new();
            catch (ArgumentNullException ex)
            {
                Console.WriteLine($"Произошла непредвиденная ошибка {ex.Message}");
                Console.WriteLine($"StackTrace:\n{ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner exception: {0}", ex.InnerException);
                }
            }
            //Когда экземпляр написан не верно(?)
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Произошла непредвиденная ошибка {ex.Message}");
            }
            catch (TaskCountLimitException ex)
            {
                Console.WriteLine($"Превышен лимит карт  {ex.Message}");
            }
            catch (TaskLengthLimitException ex)
            {
                Console.WriteLine($"Превышен лимит длины названия карты {ex.Message}");
            }
            catch (DuplicateTaskException ex)
            {
                Console.WriteLine($"Такое название карты уже есть {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла непредвиденная ошибка {ex.Message}");
                Console.WriteLine($"Произошла непредвиденная ошибка:{ex.GetType().Name}");
                Console.WriteLine($"StackTrace:\n{ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner exception: {0}", ex.InnerException);
                }
            }
        }
       
    }   
}
