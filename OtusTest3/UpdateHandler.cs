using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;

namespace OtusTest3
{
    internal class UpdateHandler : IUpdateHandler
    {
        private IUserService _userService;
        private IToDoService _toDoService;
        private ITelegramBotClient _telegramBotClient;
        public UpdateHandler(IUserService userService, IToDoService toDoService)
        {
            _userService = userService;
            _toDoService = toDoService;
        }

        public UpdateHandler()
        {
        }

        private ToDoService toDoService = new();
        private ToDoUser botUser = new();
        private List<ToDoItem> cardsNamesList = [];
        private string commandEater = "";
        private int taskCounts = 0;
        private int taskCountLimitMin = 3;
        private int taskCountLimitMax = 100;
        private string userName = "Пользователь";
        private bool commandAccess = false;

        public void HandleUpdateAsync(ITelegramBotClient botClient, Update update)
        {
            _telegramBotClient.SendMessage(update.Message.Chat,"Доступные команды /start, " +
               "/help, /info, /addtask, /showtasks, /removetask,/completetask,/showalltasks, /exit");
            bool isRun = true;
            while (isRun)
            {
                commandEater = Console.ReadLine() ?? "";
                switch (commandEater)
                {
                    case "/start":
                        toDoService.StartPanel(_telegramBotClient, update);
                        commandAccess = true;
                        break;
                    case "/help":
                        toDoService.HelpPanel(userName);
                        break;
                    case "/info":
                        toDoService.InfoPanel(userName);
                        break;
                    case "/addtask" when commandAccess == true:
                        toDoService.AddCardPanel(botUser,cardsNamesList,taskCounts,taskCountLimitMax,taskCountLimitMin, commandEater);
                        break;
                    case "/showtasks" when commandAccess == true:
                        toDoService.ShowCardPanel(cardsNamesList);
                        break;
                    case "/showalltasks" when commandAccess == true:
                        toDoService.ShowAllCardsPanel(cardsNamesList);
                        break;
                    case "/removetask" when commandAccess == true:
                        toDoService.RemoveCardPanel(cardsNamesList, commandEater);
                        break;
                    case string si when si.StartsWith("/completetask") && commandAccess == true:
                        toDoService.CompleteTaskPanel(commandEater, cardsNamesList);
                        break;
                    case "/exit":
                        isRun = toDoService.ExitPanel(out isRun, userName);
                        break;
                    default:
                        _telegramBotClient.SendMessage(update.Message.Chat, "Ошибка, введите доступную команду");
                        break;
                }
            }
        }
    }
}
