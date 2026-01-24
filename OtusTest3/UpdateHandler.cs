using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;
using System.Threading.Tasks;

namespace OtusTest3
{
    internal class UpdateHandler : IUpdateHandler
    {
        private IUserService _userService;

        public UpdateHandler(IUserService userService)
        {
            _userService = userService;
        }

        public UpdateHandler()
        {
        }

        private ITelegramBotClient _telegramBotClient;
        private ToDoService _toDoService;
        private ToDoUser botUser = new();
        private bool commandAccess = false;
        public void HandleUpdateAsync(ITelegramBotClient botClient, Update update)
        {
            Guid taskId;
            string commandEater = "";
            _telegramBotClient.SendMessage(update.Message.Chat,"Доступные команды /start, " +
               "/help, /info, /addtask, /showtasks, /removetask,/completetask,/showalltasks, /exit");
            bool isRun = true;
            while (isRun)
            {
                commandEater = Console.ReadLine() ?? "";
                switch (commandEater)
                {
                    case "/start":
                        _toDoService.StartPanel(_telegramBotClient, update);
                        commandAccess = true;
                        break;
                    case "/help":
                        _toDoService.HelpPanel(_telegramBotClient, update);
                        break;
                    case "/info":
                        _toDoService.InfoPanel(update);
                        break;
                    case string s when s.StartsWith("/addtask") && commandAccess == true:
                        _toDoService.Add(botUser, commandEater);
                        break;
                    case "/showtasks" when commandAccess == true:
                        _toDoService.GetAllByUserId(botUser.UserId);
                        break;
                    case "/showalltasks" when commandAccess == true:
                        _toDoService.GetActiveByUserId(botUser.UserId);
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
                        isRun = _toDoService.ExitPanel(out isRun, update);
                        break;
                    default:
                        _telegramBotClient.SendMessage(update.Message.Chat, "Ошибка, введите доступную команду");
                        break;
                }
            }
        }
    }
}
