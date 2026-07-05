CREATE TABLE ToDoUser
(
    UserId uuid NOT NULL PRIMARY KEY,
    TelegramUserName   text        NOT NULL,
    RegisteredAt       timestamptz NOT NULL,
    TelegramUserId     bigint      NOT NULL
);

CREATE TABLE ToDoList
(
    Id             uuid        NOT NULL PRIMARY KEY,

    Name           text        NULL,
    UserId         uuid        NOT NULL,
    CreatedAt      timestamptz NOT NULL
);

CREATE TABLE ToDoItem
(
    Id               uuid        NOT NULL
        PRIMARY KEY,

    Name             text        NOT NULL,
    UserId           uuid        NOT NULL,
    ListId           uuid        NULL,
    CreatedAt        timestamptz NOT NULL,
    State            int         NOT NULL,
    StateChangedAt   timestamptz NULL,
    DeadLine         timestamptz NULL
);

-- ToDoList.UserId -> ToDoUser.UserId
ALTER TABLE ToDoList
    ADD CONSTRAINT fk_todolist_user
        FOREIGN KEY (UserId)
        REFERENCES ToDoUser (UserId)
        ON UPDATE NO ACTION
        ON DELETE NO ACTION;

-- ToDoItem.UserId -> ToDoUser.UserId
ALTER TABLE ToDoItem
    ADD CONSTRAINT fk_todoitem_user
        FOREIGN KEY (UserId)
        REFERENCES ToDoUser (UserId)
        ON UPDATE NO ACTION
        ON DELETE NO ACTION;

-- ToDoItem.ListId -> ToDoList.Id
ALTER TABLE ToDoItem
    ADD CONSTRAINT fk_todoitem_list
        FOREIGN KEY (ListId)
        REFERENCES ToDoList (Id)
        ON UPDATE NO ACTION
        ON DELETE NO ACTION;

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