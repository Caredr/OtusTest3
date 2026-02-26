using OtusTest3.Core.DataAccess;
using OtusTest3.Core.Entities;
using OtusTest3.Core.Infrastructure.DataAccess;
using OtusTest3.Core.Services;
using System;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace OtusTest3.Core.TelegramBot
{
    internal class UpdateHandler : IUpdateHandler
    {
        private readonly IUserService _userService;
        private readonly IToDoService _iToDoService;
        private readonly IToDoReportService _iToDoReportService;



        ReplyKeyboardMarkup replyKeyboard = new ReplyKeyboardMarkup(
            new List<KeyboardButton[]>()
                 {
                      new KeyboardButton[]
                      {
                           new KeyboardButton("/showalltasks"),
                           new KeyboardButton("/showtasks"),
                      },
                      new KeyboardButton[]
                      {
                           new KeyboardButton("/report")
                      }
           })
        {
            // автоматическое изменение размера клавиатуры, если не стоит true,
            ResizeKeyboard = true,
        };


        public UpdateHandler(IUserService userService, IToDoService iToDoService, IToDoReportService iToDoReportService)
        {
            _userService = userService;
            _iToDoService = iToDoService;
            _iToDoReportService = iToDoReportService;
        }
        private bool commandAccess = false;
        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            try
            {
                string commandEater = update.Message.Text.Trim();
                Guid taskId = default;
                ToDoUser? toDoUser = await _userService.GetUser(update.Message.From.Id, ct);
                if (toDoUser == null)
                {
                    toDoUser = await _userService.RegisterUser(update.Message.From.Id, update.Message.From.Username, ct);
                }
                if (commandAccess)
                {
                    await botClient.SendMessage(update.Message.Chat, "Меню тасок", replyMarkup: replyKeyboard);
                }
                switch (commandEater)
                {
                    case "/start":
                        await StartPanel(botClient, update, ct);
                        commandAccess = true;
                        if (commandAccess == true)
                        {
                            await botClient.SendMessage(update.Message.Chat, "старт дан, commandAccess = true");
                            return;
                        }
                        break;
                    case "/showalltasks":
                        var items = await _iToDoService.GetActiveByUserId(toDoUser.UserId, ct);
                        if (items.Count == 0)
                        {
                            await botClient.SendMessage(chatId: update.Message!.Chat.Id,
                                text: "У вас нет активных задач.",
                                cancellationToken: ct);
                            break;
                        }
                        var sb = new StringBuilder();
                        sb.AppendLine("Ваши активные задачи:");
                        int i = 1;
                        foreach (var item in items)
                        {
                            sb.AppendLine($"{i}. {item.Name} (создана: {item.CreatedAt:g})");
                            i++;
                        }
                        await botClient.SendMessage(chatId: update.Message!.Chat.Id,text: sb.ToString(),
                        cancellationToken: ct);
                        break;
                    case "Menu":
                        await botClient.SendMessage(update.Message.Chat, "\"Доступные команды /start, \" +\r\n" +
                        "\"/help, /info, /addtask, /showtasks, /removetask,/completetask,/showalltasks,/report,/find\"", cancellationToken: ct);
                        break;
                    case "/showtasks":
                        await _iToDoService.GetActiveByUserId(toDoUser.UserId, ct);
                        break;
                    case "/report":
                        await _iToDoService.GetActiveByUserId(toDoUser.UserId, ct);
                        break;
                    case "/help":
                        await HelpPanel(botClient, update, ct);
                        break;
                    case "/info":
                        await InfoPanel(botClient, update, ct);
                        break;
                    case string s when s.StartsWith("/addtask") && commandAccess == true:
                        await _iToDoService.Add(toDoUser, commandEater, ct);
                        await botClient.SendMessage(update.Message.Chat, "таска добавленна");
                        break;
                    case string s when s.StartsWith("/removetask") && commandAccess == true:
                        if (Guid.TryParse(commandEater, out taskId))
                        {
                            await _iToDoService.Delete(taskId, ct);
                            await botClient.SendMessage(update.Message.Chat, "Задача удалена");
                        }
                        else await botClient.SendMessage(update.Message.Chat, "Некорректный идентификатор задачи");
                        break;
                    case string si when si.StartsWith("/find") && commandAccess == true:
                        if (Guid.TryParse(commandEater, out taskId))
                        {
                            await _iToDoService.Find(toDoUser, commandEater, ct);
                        }
                        break;
                    default:
                        await botClient.SendMessage(update.Message.Chat, "Ошибка, введите доступную команду");
                        break;
                    }
                }
                 catch (Exception ex)
                 {
                Console.WriteLine(ex.ToString());
                  }
            }
        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken ct)
        {
            throw new NullReferenceException(null, exception);
        }
        public async Task StartPanel(ITelegramBotClient botClient, Update update,CancellationToken ct)
        {
            if (_userService.GetUser(update.Message.From.Id, ct) is null)
            {
                _userService.RegisterUser(update.Message.From.Id, update.Message.From.Username, ct);

            }
            await botClient.SendMessage(update.Message.Chat, "Добро пожаловать!");
        }
        public async Task HelpPanel(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
             await botClient.SendMessage(update.Message.Chat, " "
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
            "\n /completetask - поставить статус карте - Completed");
            
        }
        public async Task InfoPanel(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            await  botClient.SendMessage(update.Message.Chat, update.Message.From.Username +
                " версия программы - 0.0.7, дата создания 18.11.2025б " + "редактура от 27.01.2026");
        }


    }
}
