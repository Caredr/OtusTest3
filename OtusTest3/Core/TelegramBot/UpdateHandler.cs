using OtusTest3.Core.DataAccess;
using OtusTest3.Core.Entities;

using OtusTest3.Core.Services;
using OtusTest3.Core.TelegramBot.Dto;
using OtusTest3.Core.TelegramBot.Scenaries;

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
        private readonly IEnumerable<IScenario> _scenarios;
        private readonly IScenarioContextRepository _contextRepository;
        private readonly IToDoListService _iToDoListService;
        private readonly int commandDataMaxLenght = 64;

        public UpdateHandler(
            IUserService userService,
            IToDoService iToDoService,
            IToDoReportService iToDoReportService,
            IEnumerable<IScenario> scenarios,
            IScenarioContextRepository contextRepository,
            IToDoListService iToDoListService)
        {
            _userService = userService;
            _iToDoService = iToDoService;
            _iToDoReportService = iToDoReportService;
            _scenarios = scenarios;
            _contextRepository = contextRepository;
            _iToDoListService = iToDoListService;
        }

        private bool commandAccess = true;

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            try
            {
                if (update == null)
                    return;
                if (update.CallbackQuery != null)
                {
                    await HandleCallbackQueryAsync(botClient, update, ct);
                    return;
                }
                if (update.Message?.From == null)
                    return;
                string? commandEater = update.Message.Text?.Trim();
                Guid taskId = default;
                ToDoUser? toDoUser = await _userService.GetUserAsync(update.Message.From.Id, ct);
                ScenarioContext context = await _contextRepository.GetContext(update.Message.From.Id, ct);
                if (commandEater == "/cancel")
                {
                    await _contextRepository.ResetContext(update.Message.From.Id, ct);
                    await SendMainKeyboard(botClient, update.Message.Chat.Id, "Действие отменено.", ct);
                    return;
                }
                if (context != null)
                {
                    // Показываем кнопку /cancel пока идёт сценарий
                    var result = await RunScenarioWithKeyboard(botClient, context, update, ct);
                    return;
                }
                if (toDoUser == null)
                {
                    toDoUser = await _userService.RegisterUser(update.Message.From.Id, update.Message.From.Username, ct);
                }
                var userId = update.Message?.From?.Id ?? update.CallbackQuery?.From?.Id;
                if (userId == null)
                    return;
                if (commandAccess)
                {
                    await SendMainKeyboard(botClient, update.Message.From.Id, update.Message.From.Username, ct);
                }
                switch (commandEater)
                {
                    case "/start":
                        await StartPanel(botClient, update, ct);
                        await SendMainKeyboard(botClient, update.Message.Chat.Id, "Главное меню:", ct);
                        if (commandAccess == true)
                        {
                            await botClient.SendMessage(update.Message.Chat, "старт дан, commandAccess = true");
                            return;
                        }
                        break;
                    case "Menu":
                        await botClient.SendMessage(update.Message.Chat, "Доступные команды /start, "  +
                            " help, / info, / addtask, / showtasks, / removetask,/ completetask,/ showalltasks,/ report,/ find,/ cansel",
                            cancellationToken: ct);
                        break;
                    case "/show":
                        {
                            // Читаем списки из FileToDoListRepository (файлы на диске)
                            var lists = await _iToDoListService.GetUserListsAsync(toDoUser.UserId, ct);
                            var keyboard = BuildShowListsKeyboard(lists);

                            await botClient.SendMessage(
                                chatId: update.Message.Chat.Id,
                                text: "Выберите список:",
                                replyMarkup: keyboard,
                                cancellationToken: ct);
                            break;
                        }
                    case "/report":
                        {
                            var (total, completed, active, generatedAt) = await _iToDoReportService.GetUserStats(toDoUser.UserId, ct);
                            var text = $"Статистика задач:\n" +
                                       $"- Всего: {total}\n" +
                                       $"- Выполнено: {completed}\n" +
                                       $"- Активные: {active}\n" +
                                       $"- Сформировано: {generatedAt:g}";

                            await botClient.SendMessage(chatId: update.Message!.Chat.Id, text: text, cancellationToken: ct);
                            break;
                        }
                    case "/help":
                        await HelpPanel(botClient, update, ct);
                        break;
                    case "/info":
                        await InfoPanel(botClient, update, ct);
                        break;
                    case string s when s.StartsWith("/addtask"):
                        {
                            context = new ScenarioContext(ScenarioType.AddTask);
                            context.Context = toDoUser;
                            await _contextRepository.SetContext(update.Message.From.Id, context, ct);
                            await SendCancelKeyboard(botClient, update.Message.Chat.Id, "Выберите список для задачи:", ct);
                            await ProcessScenario(botClient, context, update, ct);
                            break;
                        }
                    case "addlist":
                        {
                            context = new ScenarioContext(ScenarioType.AddList);
                            context.Context = toDoUser;
                            await _contextRepository.SetContext(update.Message.From.Id, context, ct);
                            await SendCancelKeyboard(botClient, update.Message.Chat.Id, "Введите /cancel для отмены.", ct);
                            var scenario = GetScenario(ScenarioType.AddList);
                            var r = await scenario.HandleMessageAsync(botClient, context, update, ct);
                            if (r == ScenarioResult.Completed)
                            {
                                await _contextRepository.ResetContext(update.Message.From.Id, ct);
                                await SendMainKeyboard(botClient, update.Message.Chat.Id, "Готово!", ct);
                            }
                            else
                                await _contextRepository.SetContext(update.Message.From.Id, context, ct);
                            break;
                        }
                    case "deletelist":
                        {
                            context = new ScenarioContext(ScenarioType.DeleteList);
                            context.Context = toDoUser;
                            await _contextRepository.SetContext(update.Message.From.Id, context, ct);
                            await SendCancelKeyboard(botClient, update.Message.Chat.Id, "Введите /cancel для отмены.", ct);
                            var scenario = GetScenario(ScenarioType.DeleteList);
                            var r = await scenario.HandleMessageAsync(botClient, context, update, ct);
                            if (r == ScenarioResult.Completed)
                            {
                                await _contextRepository.ResetContext(update.Message.From.Id, ct);
                                await SendMainKeyboard(botClient, update.Message.Chat.Id, "Готово!", ct);
                            }
                            else
                                await _contextRepository.SetContext(update.Message.From.Id, context, ct);
                            break;
                        }
                    case string s when s.StartsWith("/removetask") && commandAccess == true:
                        {
                            if (Guid.TryParse(commandEater, out taskId))
                            {
                                await _iToDoService.DeleteAsync(taskId, ct);
                                await botClient.SendMessage(update.Message.Chat, "Задача удалена");
                            }
                            else
                            {
                                await botClient.SendMessage(update.Message.Chat, "Некорректный идентификатор задачи");
                            }
                            break;
                        }
                    case string si when si.StartsWith("/find") && commandAccess == true:
                        {
                            if (Guid.TryParse(commandEater, out taskId))
                            {
                                await _iToDoService.FindAsync(toDoUser, commandEater, ct);
                            }
                            break;
                        }
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

        private async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            if (update.CallbackQuery == null)
                return;
            var callbackQuery = update.CallbackQuery;
            var userId = callbackQuery.From.Id;
            var callbackData = callbackQuery.Data ?? string.Empty;

            // 0) Запуск AddList / DeleteList по инлайн-кнопкам
            if (callbackData == "addlist")
            {
                var toDoUser = await _userService.GetUserAsync(userId, ct)
                    ?? await _userService.RegisterUser(userId, callbackQuery.From.Username, ct);
                var newContext = new ScenarioContext(ScenarioType.AddList);
                newContext.Context = toDoUser;
                await _contextRepository.SetContext(userId, newContext, ct);
                await botClient.SendMessage(callbackQuery.Message!.Chat.Id,
                    "Введите название нового списка:",
                    cancellationToken: ct);
                newContext.CurrentStep = "Name";
                await _contextRepository.SetContext(userId, newContext, ct);
                await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);
                return;
            }
            if (callbackData == "deletelist")
            {
                var toDoUser = await _userService.GetUserAsync(userId, ct)
                    ?? await _userService.RegisterUser(userId, callbackQuery.From.Username, ct);
                var newContext = new ScenarioContext(ScenarioType.DeleteList);
                newContext.Context = toDoUser;
                await _contextRepository.SetContext(userId, newContext, ct);
                var scenario = GetScenario(ScenarioType.DeleteList);
                await scenario.HandleMessageAsync(botClient, newContext, update, ct);
                await _contextRepository.SetContext(userId, newContext, ct);
                await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);
                return;
            }

            // Получаем контекст пользователя (нужен для сценариев с шагами)
            var context = await _contextRepository.GetContext(userId, ct);

            // 1) Выбор конкретного списка для DeleteList
            if (context?.CurrentScenario == ScenarioType.DeleteList &&
                context.CurrentStep == "Approve" &&
                callbackData.StartsWith("deletelist"))
            {
                var dto = ToDoListCallbackDto.FromString(callbackData);
                if (dto.ToDoListId == Guid.Empty)
                {
                    await botClient.AnswerCallbackQuery(callbackQuery.Id, "Некорректный список", cancellationToken: ct);
                    return;
                }
                var todoList = await _iToDoListService.GetAsync(dto.ToDoListId, ct);
                if (todoList == null)
                {
                    await botClient.AnswerCallbackQuery(callbackQuery.Id, "Список не найден", cancellationToken: ct);
                    return;
                }
                context.Data["SelectedList"] = todoList;
                await _contextRepository.SetContext(userId, context, ct);
                var scenario = GetScenario(ScenarioType.DeleteList);
                await scenario.HandleMessageAsync(botClient, context, update, ct);
                await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);
                return;
            }

            // 2) Подтверждение удаления yes/no
            if (context?.CurrentScenario == ScenarioType.DeleteList &&
                context.CurrentStep == "Delete")
            {
                var scenario = GetScenario(ScenarioType.DeleteList) as DeleteListScenario;
                if (scenario != null)
                {
                    await scenario.HandleCallbackQueryAsync(botClient, context, callbackQuery, ct);
                    if (context.CurrentStep == null)
                        await _contextRepository.ResetContext(userId, ct);
                    else
                        await _contextRepository.SetContext(userId, context, ct);
                }
                await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);
                return;
            }

            // 3) Callback для AddTaskScenario (выбор списка)
            if (context?.CurrentScenario == ScenarioType.AddTask &&
                context.CurrentStep == "SelectList" &&
                callbackData.StartsWith("addtask_list"))
            {
                var scenario = GetScenario(ScenarioType.AddTask);
                var result = await scenario.HandleMessageAsync(botClient, context, update, ct);
                if (result == ScenarioResult.Completed)
                    await _contextRepository.ResetContext(userId, ct);
                else
                    await _contextRepository.SetContext(userId, context, ct);
                await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);
                return;
            }

            // 4) Просмотр задач выбранного списка (show|{listId})
            if (callbackData.StartsWith("show"))
            {
                var toDoUser = await _userService.GetUserAsync(userId, ct)
                    ?? await _userService.RegisterUser(userId, callbackQuery.From.Username, ct);

                var dto = ToDoListCallbackDto.FromString(callbackData);

                // Создаём одноразовый контекст для ShowTasksScenario
                var showContext = new ScenarioContext(ScenarioType.ShowTasks);
                showContext.Context = toDoUser;

                // Передаём выбранный список
                if (dto.ToDoListId != Guid.Empty)
                {
                    var list = await _iToDoListService.GetAsync(dto.ToDoListId, ct);
                    showContext.Data["SelectedListId"] = dto.ToDoListId;
                    showContext.Data["SelectedListName"] = list?.Name ?? "(без имени)";
                }
                else
                {
                    showContext.Data["SelectedListId"] = Guid.Empty;
                    showContext.Data["SelectedListName"] = "Без списка";
                }

                var showScenario = GetScenario(ScenarioType.ShowTasks);
                await showScenario.HandleMessageAsync(botClient, showContext, update, ct);

                await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);
                return;
            }

            await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);
        }

        private InlineKeyboardMarkup BuildShowListsKeyboard(IReadOnlyList<ToDoList> lists)
        {
            var rows = new List<IEnumerable<InlineKeyboardButton>>();

            var noListCallbackDto = new ToDoListCallbackDto
            {
                Action = "show",
                ToDoListId = Guid.Empty
            };
            var noListCallback = ToDoListCallbackDto.ToString(noListCallbackDto);

            rows.Add(
            [
                InlineKeyboardButton.WithCallbackData("📌 Без списка", noListCallback)
            ]);

            foreach (var list in lists)
            {
                var dto = new ToDoListCallbackDto
                {
                    Action = "show",
                    ToDoListId = list.Id
                };

                var callbackData = ToDoListCallbackDto.ToString(dto);
                if (callbackData.Length > commandDataMaxLenght)
                    callbackData = callbackData[..commandDataMaxLenght];

                rows.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData(list.Name ?? "(без имени)", callbackData)
                });
            }

            rows.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData("🆕 Добавить", "addlist"),
                InlineKeyboardButton.WithCallbackData("❌ Удалить", "deletelist")
            });

            return new InlineKeyboardMarkup(rows);
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken ct)
        {
            throw new NullReferenceException(null, exception);
        }

        public async Task StartPanel(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            var user = await _userService.GetUserAsync(update.Message.From.Id, ct);
            if (user == null)
            {
                await _userService.RegisterUser(update.Message.From.Id, update.Message.From.Username, ct);
            }
            await botClient.SendMessage(update.Message.Chat, "Добро пожаловать!");
        }

        public static async Task HelpPanel(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            await botClient.SendMessage(update.Message.Chat, " "
                + update.Message.From.Username + " чтобы пользоваться программой" +
                "\n пожалуйста вводите комманды /start, /help, /info, /exit" +
                "\n /start - задает или меняет ваше имя" +
                "\n /help - доска информации" +
                "\n /info - дата создания программы" +
                "\n /addtask - добавить задачу" +
                "\n /show - показать списки (выбери список — увидишь задачи)" +
                "\n /report - Статистика по задачам" +
                "\n /find - Найти по имени" +
                "\n /removetask - убрать задачу" +
                "\n /completetask - поставить статус - Completed" +
                "\n /cancel  - отмена текущего ввода");
        }

        public static async Task InfoPanel(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            await botClient.SendMessage(update.Message.Chat, update.Message.From.Username +
                " версия программы - 0.0.8, дата создания 18.11.2025, редактура от 08.06.2026");
        }

        public IScenario GetScenario(ScenarioType scenarioType)
        {
            var scenario = _scenarios.FirstOrDefault(s => s.CanHandle(scenarioType));
            if (scenario == null)
            {
                var availableScenarios = string.Join(", ", _scenarios.Select(s => s.GetType().Name));
                throw new InvalidOperationException(
                    $"Сценарий для типа '{scenarioType}' не найден. " +
                    $"Доступные сценарии: {availableScenarios}");
            }
            return scenario;
        }

        /// <summary>
        /// Запускает сценарий. При завершении возвращает главную клавиатуру.
        /// </summary>
        public async Task RunScenarioWithKeyboard(ITelegramBotClient botClient, ScenarioContext context, Update update, CancellationToken ct)
        {
            IScenario scenario = GetScenario(context.CurrentScenario);
            ScenarioResult result = await scenario.HandleMessageAsync(botClient, context, update, ct);

            if (result == ScenarioResult.Completed)
            {
                await _contextRepository.ResetContext(update.Message.From.Id, ct);
                await SendMainKeyboard(botClient, update.Message.Chat.Id, "Главное меню:", ct);
            }
            else
            {
                // Сценарий ещё идёт — напоминаем про /cancel
                await SendCancelKeyboard(botClient, update.Message.Chat.Id, "Введите /cancel для отмены.", ct);
                await _contextRepository.SetContext(update.Message.From.Id, context, ct);
            }
        }

        public async Task ProcessScenario(ITelegramBotClient botClient, ScenarioContext context, Update update, CancellationToken ct)
        {
            IScenario scenario = GetScenario(context.CurrentScenario);
            ScenarioResult result = await scenario.HandleMessageAsync(botClient, context, update, ct);

            if (result == ScenarioResult.Completed)
                await _contextRepository.ResetContext(update.Message.From.Id, ct);
            else
                await _contextRepository.SetContext(update.Message.From.Id, context, ct);
        }

        private static async Task SendMainKeyboard(ITelegramBotClient bot, long chatId, string text, CancellationToken ct)
        {
            var keyboard = new ReplyKeyboardMarkup(
                new List<KeyboardButton[]>
                {
                    new KeyboardButton[]
                    {
                        new KeyboardButton("/show"),
                        new KeyboardButton("/addtask"),
                    },
                    new KeyboardButton[]
                    {
                        new KeyboardButton("/report")
                    }
                })
            {
                ResizeKeyboard = true
            };
            await bot.SendMessage(chatId, text, replyMarkup: keyboard, cancellationToken: ct);
        }

        private static async Task SendCancelKeyboard(ITelegramBotClient bot, long chatId, string text, CancellationToken ct)
        {
            var keyboard = new ReplyKeyboardMarkup(new KeyboardButton("/cancel"))
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = true
            };
            await bot.SendMessage(chatId, text, replyMarkup: keyboard, cancellationToken: ct);
        }
    }
}
