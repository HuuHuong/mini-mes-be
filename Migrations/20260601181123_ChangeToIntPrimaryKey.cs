using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mini_mes_be.Migrations
{
    /// <inheritdoc />
    public partial class ChangeToIntPrimaryKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Cannot ALTER a PK column type or FK column directly in SQL Server.
            // Strategy: drop FK + indexes, drop tables, recreate with int IDENTITY PK.
            // Note: existing data (if any) will be lost — acceptable for a dev schema change.

            // 1. Drop FK constraint and indexes on RefreshTokens
            migrationBuilder.Sql("ALTER TABLE [RefreshTokens] DROP CONSTRAINT [FK_RefreshTokens_Users_UserId]");
            migrationBuilder.Sql("DROP INDEX [IX_RefreshTokens_Token] ON [RefreshTokens]");
            migrationBuilder.Sql("DROP INDEX [IX_RefreshTokens_UserId] ON [RefreshTokens]");

            // 2. Drop indexes on Users
            migrationBuilder.Sql("DROP INDEX [IX_Users_Email] ON [Users]");
            migrationBuilder.Sql("DROP INDEX [IX_Users_Username] ON [Users]");

            // 3. Drop tables (RefreshTokens first due to FK)
            migrationBuilder.Sql("DROP TABLE [RefreshTokens]");
            migrationBuilder.Sql("DROP TABLE [Users]");

            // 4. Recreate Users with int IDENTITY PK
            migrationBuilder.Sql(@"
                CREATE TABLE [Users] (
                    [Id]           int           NOT NULL IDENTITY(1,1),
                    [Username]     nvarchar(100) NOT NULL,
                    [Email]        nvarchar(256) NOT NULL,
                    [PasswordHash] nvarchar(max) NOT NULL,
                    [Role]         nvarchar(50)  NOT NULL,
                    [IsActive]     bit           NOT NULL,
                    [CreatedAt]    bigint        NOT NULL,
                    [UpdatedAt]    bigint        NULL,
                    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
                )");

            migrationBuilder.Sql("CREATE UNIQUE INDEX [IX_Users_Email]    ON [Users] ([Email])");
            migrationBuilder.Sql("CREATE UNIQUE INDEX [IX_Users_Username] ON [Users] ([Username])");

            // 5. Recreate RefreshTokens with int IDENTITY PK and int FK
            migrationBuilder.Sql(@"
                CREATE TABLE [RefreshTokens] (
                    [Id]             int           NOT NULL IDENTITY(1,1),
                    [Token]          nvarchar(450) NOT NULL,
                    [ExpiresAt]      bigint        NOT NULL,
                    [IsRevoked]      bit           NOT NULL,
                    [ReplacedByToken] nvarchar(max) NULL,
                    [CreatedByIp]    nvarchar(max) NULL,
                    [RevokedByIp]    nvarchar(max) NULL,
                    [RevokedAt]      bigint        NULL,
                    [UserId]         int           NOT NULL,
                    [CreatedAt]      bigint        NOT NULL,
                    [UpdatedAt]      bigint        NULL,
                    CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_RefreshTokens_Users_UserId]
                        FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
                )");

            migrationBuilder.Sql("CREATE UNIQUE INDEX [IX_RefreshTokens_Token]  ON [RefreshTokens] ([Token])");
            migrationBuilder.Sql("CREATE INDEX        [IX_RefreshTokens_UserId] ON [RefreshTokens] ([UserId])");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop FK + indexes
            migrationBuilder.Sql("ALTER TABLE [RefreshTokens] DROP CONSTRAINT [FK_RefreshTokens_Users_UserId]");
            migrationBuilder.Sql("DROP INDEX [IX_RefreshTokens_Token]  ON [RefreshTokens]");
            migrationBuilder.Sql("DROP INDEX [IX_RefreshTokens_UserId] ON [RefreshTokens]");
            migrationBuilder.Sql("DROP INDEX [IX_Users_Email]    ON [Users]");
            migrationBuilder.Sql("DROP INDEX [IX_Users_Username] ON [Users]");

            migrationBuilder.Sql("DROP TABLE [RefreshTokens]");
            migrationBuilder.Sql("DROP TABLE [Users]");

            // Restore Users with uniqueidentifier PK
            migrationBuilder.Sql(@"
                CREATE TABLE [Users] (
                    [Id]           uniqueidentifier NOT NULL,
                    [Username]     nvarchar(100)    NOT NULL,
                    [Email]        nvarchar(256)    NOT NULL,
                    [PasswordHash] nvarchar(max)    NOT NULL,
                    [Role]         nvarchar(50)     NOT NULL,
                    [IsActive]     bit              NOT NULL,
                    [CreatedAt]    bigint           NOT NULL,
                    [UpdatedAt]    bigint           NULL,
                    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
                )");

            migrationBuilder.Sql("CREATE UNIQUE INDEX [IX_Users_Email]    ON [Users] ([Email])");
            migrationBuilder.Sql("CREATE UNIQUE INDEX [IX_Users_Username] ON [Users] ([Username])");

            // Restore RefreshTokens with uniqueidentifier
            migrationBuilder.Sql(@"
                CREATE TABLE [RefreshTokens] (
                    [Id]              uniqueidentifier NOT NULL,
                    [Token]           nvarchar(450)    NOT NULL,
                    [ExpiresAt]       bigint           NOT NULL,
                    [IsRevoked]       bit              NOT NULL,
                    [ReplacedByToken] nvarchar(max)    NULL,
                    [CreatedByIp]     nvarchar(max)    NULL,
                    [RevokedByIp]     nvarchar(max)    NULL,
                    [RevokedAt]       bigint           NULL,
                    [UserId]          uniqueidentifier NOT NULL,
                    [CreatedAt]       bigint           NOT NULL,
                    [UpdatedAt]       bigint           NULL,
                    CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_RefreshTokens_Users_UserId]
                        FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
                )");

            migrationBuilder.Sql("CREATE UNIQUE INDEX [IX_RefreshTokens_Token]  ON [RefreshTokens] ([Token])");
            migrationBuilder.Sql("CREATE INDEX        [IX_RefreshTokens_UserId] ON [RefreshTokens] ([UserId])");
        }
    }
}
