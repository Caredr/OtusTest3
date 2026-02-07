using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;
using OtusTest3.Core.Entities;
using OtusTest3.Core.Infrastructure.DataAccess;
using OtusTest3.Core.Services;
using System.Threading.Tasks;

namespace OtusTest3.Core.TelegramBot
{
    internal class UpdateHandler : IUpdateHandler
    {
        private readonly IUserService _userService;
        private readonly InMemoryToDoRepository _inMemoryToDoRepository;
        private readonly ToDoReportService _toDoReportService;

        public UpdateHandler(IUserService userService, InMemoryToDoRepository inMemoryToDoRepository, ToDoReportService toDoReportService)
        {
            _userService = userService;
            _inMemoryToDoRepository = inMemoryToDoRepository;
            _toDoReportService = toDoReportService;
        }
        private bool commandAccess = false;
        public void HandleUpdateAsync(ITelegramBotClient botClient, Update update)
        {
            string commandEater = update.Message.Text.Trim();
            Guid taskId = default;
            
            ToDoUser? toDoUser = _userService.GetUser(update.Message.From.Id);
            botClient.SendMessage(update.Message.Chat,"Доступные команды /start, " +
               "/help, /info, /addtask, /showtasks, /removetask,/completetask,/showalltasks,/report,/find");
            //bool isRun = true;
            //while (isRun)
            //{
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
                        _inMemoryToDoRepository.Add(new ToDoItem(toDoUser, commandEater));
                        botClient.SendMessage(update.Message.Chat, "таска добавленна");
                        break;
                    case "/showtasks" when commandAccess == true:
                        _inMemoryToDoRepository.GetActiveByUserId(toDoUser.UserId);
                        break;
                    case "/showalltasks" when commandAccess == true:
                        _inMemoryToDoRepository.GetAllByUserId(toDoUser.UserId);
                        break;
                    case string s when s.StartsWith("/removetask") && commandAccess == true:
                        if (Guid.TryParse(commandEater, out taskId))
                        {
                            _inMemoryToDoRepository.Delete(taskId);
                            botClient.SendMessage(update.Message.Chat, "Задача удалена");
                        }
                        else botClient.SendMessage(update.Message.Chat, "Некорректный идентификатор задачи");
                        break;
                   
                case string si when si.StartsWith("/find") && commandAccess == true:
                    if (Guid.TryParse(commandEater, out taskId))
                    {
                        _inMemoryToDoRepository.Find(toDoUser.UserId, task => task.State == ToDoItemState.Active);
                    }
                    break;
                case "/report" when commandAccess == true:
                    _toDoReportService.GetUserStats(toDoUser.UserId);
                        break;

                default:
                            botClient.SendMessage(update.Message.Chat, "Ошибка, введите доступную команду");
                            break;
                }
              }
        public void StartPanel(ITelegramBotClient botClient, Update update)
        {
            if (_userService.GetUser(update.Message.From.Id) is null)
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
            "\n /report - Статистика по задачам" +
            "\n /find - Найти по имени" +
            "\n /removetask - убрать карту" +
            "\n /completetask - поставить статус карте - Completed" );
        }
        public void InfoPanel(ITelegramBotClient botClient, Update update)
        {
            botClient.SendMessage(update.Message.Chat, update.Message.From.Username +
                " версия программы - 0.0.7, дата создания 18.11.2025б " + "редактура от 27.01.2026");
        }
    }
}
