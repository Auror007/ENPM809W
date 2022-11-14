CREATE TABLE [dbo].[BlogComment] (
    [Id]    NVARCHAR (50) NOT NULL,
    [Username]  NVARCHAR (50) NOT NULL,
    [CommentTime] DATETIME2 (7)  NOT NULL,
    [Message]   NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_BlogComment] PRIMARY KEY CLUSTERED ([Id] ASC, [Username] ASC, [CommentTime] ASC)
);