using OtusTest3.Core.DataAccess;
using OtusTest3.Core.Entities;
using OtusTest3.Core.Services;
using OtusTest3.Core.TelegramBot.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace OtusTest3.Core.TelegramBot.Scenaries
{
    internal class DeleteListScenario : IScenario
    {
        private readonly IUserService _userService;
        private readonly IToDoListService _todoListService;
        private readonly IToDoService _todoService;

        public DeleteListScenario(IUserService userService, IToDoListService todoListService, IToDoService todoService)
        {
            _userService = userService;
            _todoListService = todoListService;
            _todoService = todoService;
        }
        public bool CanHandle(ScenarioType scenario)
        {
            return scenario == ScenarioType.AddTask;
        }
        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot,
        ScenarioContext context, Update update, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var user = context.Context!;
            var inputText = update.Message?.Text?.Trim();
            switch (context.CurrentStep)
            {
                case null:
                    var todoUser = await _userService.GetUserAsync(update.Message.From.Id, ct);
                    context.Context = todoUser;
                    // Отправляем сообщение
                    await bot.SendMessage(update.Message.Chat.Id, "Выберите список для удаления:",
                        cancellationToken: ct);
                    var lists = await _todoListService.GetUserListsAsync(todoUser.UserId, ct);
                    var buttons = new List<InlineKeyboardButton>();
                    foreach (var list in lists)
                    {
                        var dto = new ToDoListCallbackDto
                        {
                            Action = "deletelist",
                            ToDoListId = list.Id
                        };

                        var callbackData = ToDoListCallbackDto.ToString(dto);
                        buttons.Add(InlineKeyboardButton.WithCallbackData(list.Name, callbackData));
                    }
                    var markup = new InlineKeyboardMarkup(buttons.ToArray());
                    await bot.SendMessage(update.Message.Chat.Id, "Выберете список для удаления:",
                        replyMarkup: markup,
                        cancellationToken: ct);
                    context.CurrentStep = "Approve";
                    return ScenarioResult.Transition;

                    case "Approve":
                    {
                        // Ожидаем, что в Data лежит выбранный ToDoList (его мы сохраняем при обработке CallbackQuery)
                        if (!context.Data.TryGetValue("SelectedList", out var obj) || obj is not ToDoList todoList)
                            return ScenarioResult.Completed;
                        todoUser = context.Context;
                        if (todoUser == null)
                            return ScenarioResult.Completed;
                        // Отправляем сообщение с подтверждением
                        var keyboard = new InlineKeyboardMarkup(new[]
                        {
                            InlineKeyboardButton.WithCallbackData("✅Да", "yes"),
                            InlineKeyboardButton.WithCallbackData("❌Нет", "no")
                        });

                        await bot.SendMessage(update.Message.Chat.Id,
                            $"Подтверждаете удаление списка {todoList.Name} и всех его задач?",
                            replyMarkup: keyboard,
                            cancellationToken: ct);

                        context.CurrentStep = "Delete";
                        return ScenarioResult.Transition;
                    }

                case "Delete":
                    {
                        if (update.CallbackQuery is not { Data: var data })
                            return ScenarioResult.Completed;
                        todoUser = context.Context;
                        if (!context.Data.TryGetValue("SelectedList", out var obj) || obj is not ToDoList todoList)
                            return ScenarioResult.Completed;
                        if (todoUser == null)
                            return ScenarioResult.Completed;
                        if (data == "yes")
                        {
                            // Удалить все задачи по ToDoUser и ToDoList
                            await _todoService.DeleteAsync(todoUser.UserId,  ct);
                            // УдалитьToDoList
                            await _todoListService.DeleteAsync(todoUser.UserId,  ct);
                            await bot.SendMessage(update.Message.Chat.Id,
                                "Список и все его задачи удалены.",
                                cancellationToken: ct);
                        }
                        else if (data == "no")
                        {
                            await bot.SendMessage(update.Message.Chat.Id,
                                "Удаление отменено.",
                                cancellationToken: ct);
                        }
                        // Очистим выбранный список
                        context.Data.Remove("SelectedList");
                        context.CurrentStep = null;
                        return ScenarioResult.Completed;
                    }
                       default:
                context.CurrentStep = null;
                return ScenarioResult.Completed;
            }
        }

        public async Task<ScenarioResult> HandleCallbackQueryAsync(ITelegramBotClient bot, ScenarioContext context,
       CallbackQuery callbackQuery, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            if (context.CurrentStep != "Delete")
                return ScenarioResult.Completed;
            var data = callbackQuery.Data;
            if (!context.Data.TryGetValue("User", out var userObj) || userObj is not ToDoUser todoUser)
                return ScenarioResult.Completed;
            if (!context.Data.TryGetValue("SelectedList", out var obj) || obj is not ToDoList todoList)
                return ScenarioResult.Completed;
            if (data == "yes")
            {
                await _todoService.DeleteAsync(todoUser.UserId,  ct);
                await _todoListService.DeleteAsync(todoUser.UserId, ct);
                await bot.SendMessage(callbackQuery.Message!.Chat.Id,
                    "Список и все его задачи удалены.",
                    cancellationToken: ct);
            }
            else if (data == "no")
            {
                await bot.SendMessage(
                    callbackQuery.Message!.Chat.Id,
                    "Удаление отменено.",
                    cancellationToken: ct);
            }

            context.Data.Remove("SelectedList");
            context.CurrentStep = null;
            return ScenarioResult.Completed;
        }
    }
}
