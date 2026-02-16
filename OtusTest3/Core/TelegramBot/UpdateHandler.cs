using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;
using OtusTest3.Core.DataAccess;
using OtusTest3.Core.Entities;
using OtusTest3.Core.Infrastructure.DataAccess;
using OtusTest3.Core.Services;
using System.Threading.Tasks;

namespace OtusTest3.Core.TelegramBot
{
    internal class UpdateHandler : IUpdateHandler
    {
        private readonly IUserService _userService;
        private readonly IToDoService _iToDoService;
        private readonly IToDoReportService _iToDoReportService;
        public UpdateHandler(IUserService userService, IToDoService iToDoService, IToDoReportService iToDoReportService)
        {
            _userService = userService;
            _iToDoService = iToDoService;
            _iToDoReportService = iToDoReportService;
        }

        private bool commandAccess = false;
        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {

            string commandEater = update.Message.Text.Trim();
            Guid taskId = default;
            ToDoUser? toDoUser = await _userService.GetUser(update.Message.From.Id, ct);
            await botClient.SendMessage(update.Message.Chat,"Доступные команды /start, " +
               "/help, /info, /addtask, /showtasks, /removetask,/completetask,/showalltasks,/report,/find",ct);
                switch (commandEater)
                {
                    case "/start":  
                        await StartPanel(botClient, update, ct);
                        commandAccess = true;
                        if(commandAccess == true)
                        {
                            await botClient.SendMessage(update.Message.Chat, "старт дан, commandAccess = true", ct);
                        }
                        break;
                    case "/help":
                        await HelpPanel(botClient, update, ct);
                        break;
                    case "/info":
                         await InfoPanel(botClient, update, ct);
                        break;
                    case string s when s.StartsWith("/addtask") && commandAccess == true:
                        await _iToDoService.Add(toDoUser, commandEater, ct);
                        await botClient.SendMessage(update.Message.Chat, "таска добавленна", ct);
                        break;
                    case "/showtasks" when commandAccess == true:
                        await _iToDoService.GetActiveByUserId(toDoUser.UserId, ct);
                        break;
                    case "/showalltasks" when commandAccess == true:
                        await _iToDoService.GetAllByUserId(toDoUser.UserId, ct);
                        break;
                    case string s when s.StartsWith("/removetask") && commandAccess == true:
                        if (Guid.TryParse(commandEater, out taskId))
                        {
                            await _iToDoService.Delete(taskId, ct);
                            await botClient.SendMessage(update.Message.Chat, "Задача удалена", ct);
                        }
                        else await botClient.SendMessage(update.Message.Chat, "Некорректный идентификатор задачи", ct);
                        break;
                   
                case string si when si.StartsWith("/find") && commandAccess == true:
                    if (Guid.TryParse(commandEater, out taskId))
                    {
                        await _iToDoService.Find(toDoUser, commandEater, ct);
                    }
                    break;
                case "/report" when commandAccess == true:
                    await _iToDoReportService.GetUserStats(toDoUser.UserId, ct);
                        break;

                default:
                            await botClient.SendMessage(update.Message.Chat, "Ошибка, введите доступную команду", ct);
                            break;
                }
              }
        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken ct)
        {
            throw new NullReferenceException(null, exception);
        }
        public Task StartPanel(ITelegramBotClient botClient, Update update,CancellationToken ct)
        {
            if (_userService.GetUser(update.Message.From.Id, ct) is null)
            {
                _userService.RegisterUser(update.Message.From.Id, update.Message.From.Username, ct);

            }
            return Task.CompletedTask;
        }
        public Task HelpPanel(ITelegramBotClient botClient, Update update, CancellationToken ct)
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
            "\n /completetask - поставить статус карте - Completed", ct);
            return Task.CompletedTask;
        }
        public Task InfoPanel(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            botClient.SendMessage(update.Message.Chat, update.Message.From.Username +
                " версия программы - 0.0.7, дата создания 18.11.2025б " + "редактура от 27.01.2026", ct);
            return Task.CompletedTask;
        }


    }
}
