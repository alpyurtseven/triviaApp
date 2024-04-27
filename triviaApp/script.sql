IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [Password] nvarchar(max) NOT NULL,
    [Status] tinyint NOT NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240417213338_initial', N'7.0.18');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'Password');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [AspNetUsers] DROP COLUMN [Password];
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240418190501_removedOpenPass', N'7.0.18');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Competitions] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [JoinLink] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Competitions] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Categories] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [CompetitionId] int NULL,
    CONSTRAINT [PK_Categories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Categories_Competitions_CompetitionId] FOREIGN KEY ([CompetitionId]) REFERENCES [Competitions] ([Id])
);
GO

CREATE TABLE [Questions] (
    [Id] int NOT NULL IDENTITY,
    [Body] nvarchar(max) NOT NULL,
    [RightAnswer] nvarchar(max) NOT NULL,
    [WrongAnswer1] nvarchar(max) NOT NULL,
    [WrongAnswer2] nvarchar(max) NOT NULL,
    [WrongAnswer3] nvarchar(max) NOT NULL,
    [Difficulty] int NOT NULL,
    [CategoryId] int NOT NULL,
    CONSTRAINT [PK_Questions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Questions_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Categories_CompetitionId] ON [Categories] ([CompetitionId]);
GO

CREATE INDEX [IX_Questions_CategoryId] ON [Questions] ([CategoryId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240418191853_competionmodels', N'7.0.18');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Categories] DROP CONSTRAINT [FK_Categories_Competitions_CompetitionId];
GO

DROP INDEX [IX_Categories_CompetitionId] ON [Categories];
GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Categories]') AND [c].[name] = N'CompetitionId');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Categories] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Categories] DROP COLUMN [CompetitionId];
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240418200740_removedRelation', N'7.0.18');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Questions] ADD [CompetitionId] int NULL;
GO

ALTER TABLE [Categories] ADD [CompetitionId] int NULL;
GO

CREATE INDEX [IX_Questions_CompetitionId] ON [Questions] ([CompetitionId]);
GO

CREATE INDEX [IX_Categories_CompetitionId] ON [Categories] ([CompetitionId]);
GO

ALTER TABLE [Categories] ADD CONSTRAINT [FK_Categories_Competitions_CompetitionId] FOREIGN KEY ([CompetitionId]) REFERENCES [Competitions] ([Id]);
GO

ALTER TABLE [Questions] ADD CONSTRAINT [FK_Questions_Competitions_CompetitionId] FOREIGN KEY ([CompetitionId]) REFERENCES [Competitions] ([Id]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240421093034_competitionRelation', N'7.0.18');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Categories] DROP CONSTRAINT [FK_Categories_Competitions_CompetitionId];
GO

ALTER TABLE [Questions] DROP CONSTRAINT [FK_Questions_Competitions_CompetitionId];
GO

DROP INDEX [IX_Questions_CompetitionId] ON [Questions];
GO

DROP INDEX [IX_Categories_CompetitionId] ON [Categories];
GO

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Questions]') AND [c].[name] = N'CompetitionId');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Questions] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [Questions] DROP COLUMN [CompetitionId];
GO

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Categories]') AND [c].[name] = N'CompetitionId');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Categories] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [Categories] DROP COLUMN [CompetitionId];
GO

CREATE TABLE [CompetitionCategory] (
    [CompetitionId] int NOT NULL,
    [CategoryId] int NOT NULL,
    CONSTRAINT [PK_CompetitionCategory] PRIMARY KEY ([CompetitionId], [CategoryId]),
    CONSTRAINT [FK_CompetitionCategory_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_CompetitionCategory_Competitions_CompetitionId] FOREIGN KEY ([CompetitionId]) REFERENCES [Competitions] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [CompetitionQuestion] (
    [CompetitionId] int NOT NULL,
    [QuestionId] int NOT NULL,
    CONSTRAINT [PK_CompetitionQuestion] PRIMARY KEY ([CompetitionId], [QuestionId]),
    CONSTRAINT [FK_CompetitionQuestion_Competitions_CompetitionId] FOREIGN KEY ([CompetitionId]) REFERENCES [Competitions] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_CompetitionQuestion_Questions_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [Questions] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_CompetitionCategory_CategoryId] ON [CompetitionCategory] ([CategoryId]);
GO

CREATE INDEX [IX_CompetitionQuestion_QuestionId] ON [CompetitionQuestion] ([QuestionId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240421093619_competitionRelationUpdated', N'7.0.18');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Participants] (
    [Id] int NOT NULL IDENTITY,
    [Username] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Participants] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Scores] (
    [Id] int NOT NULL IDENTITY,
    [Points] int NOT NULL,
    [ParticipantId] int NOT NULL,
    [CompetitionId] int NOT NULL,
    CONSTRAINT [PK_Scores] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Scores_Competitions_CompetitionId] FOREIGN KEY ([CompetitionId]) REFERENCES [Competitions] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Scores_Participants_ParticipantId] FOREIGN KEY ([ParticipantId]) REFERENCES [Participants] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Scores_CompetitionId] ON [Scores] ([CompetitionId]);
GO

CREATE INDEX [IX_Scores_ParticipantId] ON [Scores] ([ParticipantId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240424084724_participants', N'7.0.18');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Scores] DROP CONSTRAINT [FK_Scores_Competitions_CompetitionId];
GO

ALTER TABLE [Scores] DROP CONSTRAINT [FK_Scores_Participants_ParticipantId];
GO

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Participants]') AND [c].[name] = N'Username');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Participants] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [Participants] ALTER COLUMN [Username] nvarchar(450) NOT NULL;
GO

ALTER TABLE [Participants] ADD [CompetitionId] int NOT NULL DEFAULT 0;
GO

CREATE INDEX [IX_Participants_CompetitionId] ON [Participants] ([CompetitionId]);
GO

CREATE UNIQUE INDEX [IX_Participants_Username_CompetitionId] ON [Participants] ([Username], [CompetitionId]);
GO

ALTER TABLE [Participants] ADD CONSTRAINT [FK_Participants_Competitions_CompetitionId] FOREIGN KEY ([CompetitionId]) REFERENCES [Competitions] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [Scores] ADD CONSTRAINT [FK_Scores_Competitions_CompetitionId] FOREIGN KEY ([CompetitionId]) REFERENCES [Competitions] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [Scores] ADD CONSTRAINT [FK_Scores_Participants_ParticipantId] FOREIGN KEY ([ParticipantId]) REFERENCES [Participants] ([Id]) ON DELETE NO ACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240424093333_UpdateDeleteBehavior', N'7.0.18');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Questions] ADD [Points] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [Questions] ADD [Time] int NOT NULL DEFAULT 0;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240424151207_updateQuestion', N'7.0.18');
GO

COMMIT;
GO

