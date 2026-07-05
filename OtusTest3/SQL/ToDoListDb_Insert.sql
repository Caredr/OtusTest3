-- Индекс для ToDoList.UserId
CREATE INDEX idx_todolist_user_id
    ON ToDoList (UserId);

-- Индекс для ToDoItem.UserId
CREATE INDEX idx_todoitem_user_id
    ON ToDoItem (UserId);

-- Индекс для ToDoItem.ListId
CREATE INDEX idx_todoitem_list_id
    ON ToDoItem (ListId);

CREATE UNIQUE INDEX idx_todouser_telegram_user_id
    ON ToDoUser (TelegramUserId);

