using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;
using System.Threading.Tasks;

namespace OtusTest3
{
    internal class UpdateHandler : IUpdateHandler
    {
        private readonly IUserService _userService;
        private readonly ToDoService _toDoService;

        public UpdateHandler(IUserService userService, ToDoService toDoService)
        {
            _userService = userService;
            _toDoService = toDoService;
        }
        private bool commandAccess = false;
        public void HandleUpdateAsync(ITelegramBotClient botClient, Update update)
        {
            Guid taskId;
            ToDoUser? toDoUser = _userService.GetUser(update.Message.From.Id);
            botClient.SendMessage(update.Message.Chat,"Доступные команды /start, " +
               "/help, /info, /addtask, /showtasks, /removetask,/completetask,/showalltasks, /exit");
            bool isRun = true;
            while (isRun)
            {
                string commandEater = Console.ReadLine() ?? "";
                switch (commandEater)
                {
                    case "/start":  
                        StartPanel(botClient, update);
                        commandAccess = true;
                        if(commandAccess == true)
                        {
                            botClient.SendMessage(update.Message.Chat, "старт дан, commandAccess = true");
                        }
                        break;
                    case "/help":
                        HelpPanel(botClient, update);
                        break;
                    case "/info":
                        InfoPanel(botClient, update);
                        break;
                    case string s when s.StartsWith("/addtask") && commandAccess == true:
                        _toDoService.Add(toDoUser, commandEater);
                        botClient.SendMessage(update.Message.Chat, "таска добавленна");
                        break;
                    case "/showtasks" when commandAccess == true:
                        _toDoService.GetAllByUserId(toDoUser.UserId);
                        break;
                    case "/showalltasks" when commandAccess == true:
                        _toDoService.GetActiveByUserId(toDoUser.UserId);
                        break;
                    case string s when s.StartsWith("/removetask") && commandAccess == true:
                        if (Guid.TryParse(commandEater, out taskId))
                        {
                            _toDoService.Delete(taskId);
                            botClient.SendMessage(update.Message.Chat, "Задача удалена");
                        }
                        else botClient.SendMessage(update.Message.Chat, "Некорректный идентификатор задачи");
                        break;
                    case string si when si.StartsWith("/completetask") && commandAccess == true:
                        if (Guid.TryParse(commandEater, out taskId))
                        {
                            _toDoService.MarkCompleted(taskId);
                            botClient.SendMessage(update.Message.Chat, "Задача завершена");
                        }
                        break;
                    case "/exit":
                        isRun = ExitPanel(out isRun, botClient, update);
                        break;
                    default:
                        botClient.SendMessage(update.Message.Chat, "Ошибка, введите доступную команду");
                        break;
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
        public void HelpPanel(ITelegramBotClient botClient, Update update)
        {
            botClient.SendMessage(update.Message.Chat, " "
                + update.Message.From.Username + " чтобы пользоваться программой" +
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
        public void InfoPanel(ITelegramBotClient botClient, Update update)
        {
            botClient.SendMessage(update.Message.Chat, update.Message.From.Username +
                " версия программы - 0.0.7, дата создания 18.11.2025б " + "редактура от 27.01.2026");
        }
        public bool ExitPanel(out bool appState, ITelegramBotClient botClient, Update update)
        {
            botClient.SendMessage(update.Message.Chat, update.Message.From.Username + " Нажмите любую кнопку, чтобы выйти");
            appState = false;
            return appState;
        }
    }
}
