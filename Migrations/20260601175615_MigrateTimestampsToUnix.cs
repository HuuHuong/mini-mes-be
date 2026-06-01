using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mini_mes_be.Migrations
{
    /// <inheritdoc />
    public partial class MigrateTimestampsToUnix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // SQL Server cannot directly ALTER COLUMN datetime2 → bigint.
            // Strategy: add new bigint columns, copy converted data, drop old, rename.

            // ── Users ────────────────────────────────────────────────────────────
            migrationBuilder.Sql("ALTER TABLE [Users] ADD [CreatedAt_New] bigint NOT NULL DEFAULT 0");
            migrationBuilder.Sql("ALTER TABLE [Users] ADD [UpdatedAt_New] bigint NULL");

            migrationBuilder.Sql(@"
                UPDATE [Users] SET
                    [CreatedAt_New] = DATEDIFF_BIG(SECOND, '1970-01-01 00:00:00', [CreatedAt]),
                    [UpdatedAt_New] = CASE WHEN [UpdatedAt] IS NULL THEN NULL
                                      ELSE DATEDIFF_BIG(SECOND, '1970-01-01 00:00:00', [UpdatedAt]) END");

            migrationBuilder.Sql("ALTER TABLE [Users] DROP COLUMN [CreatedAt]");
            migrationBuilder.Sql("ALTER TABLE [Users] DROP COLUMN [UpdatedAt]");
            migrationBuilder.Sql("EXEC sp_rename '[Users].[CreatedAt_New]', 'CreatedAt', 'COLUMN'");
            migrationBuilder.Sql("EXEC sp_rename '[Users].[UpdatedAt_New]', 'UpdatedAt', 'COLUMN'");

            // ── RefreshTokens ────────────────────────────────────────────────────
            migrationBuilder.Sql("ALTER TABLE [RefreshTokens] ADD [CreatedAt_New] bigint NOT NULL DEFAULT 0");
            migrationBuilder.Sql("ALTER TABLE [RefreshTokens] ADD [UpdatedAt_New] bigint NULL");
            migrationBuilder.Sql("ALTER TABLE [RefreshTokens] ADD [ExpiresAt_New] bigint NOT NULL DEFAULT 0");
            migrationBuilder.Sql("ALTER TABLE [RefreshTokens] ADD [RevokedAt_New] bigint NULL");

            migrationBuilder.Sql(@"
                UPDATE [RefreshTokens] SET
                    [CreatedAt_New] = DATEDIFF_BIG(SECOND, '1970-01-01 00:00:00', [CreatedAt]),
                    [UpdatedAt_New] = CASE WHEN [UpdatedAt] IS NULL THEN NULL
                                      ELSE DATEDIFF_BIG(SECOND, '1970-01-01 00:00:00', [UpdatedAt]) END,
                    [ExpiresAt_New] = DATEDIFF_BIG(SECOND, '1970-01-01 00:00:00', [ExpiresAt]),
                    [RevokedAt_New] = CASE WHEN [RevokedAt] IS NULL THEN NULL
                                      ELSE DATEDIFF_BIG(SECOND, '1970-01-01 00:00:00', [RevokedAt]) END");

            migrationBuilder.Sql("ALTER TABLE [RefreshTokens] DROP COLUMN [CreatedAt]");
            migrationBuilder.Sql("ALTER TABLE [RefreshTokens] DROP COLUMN [UpdatedAt]");
            migrationBuilder.Sql("ALTER TABLE [RefreshTokens] DROP COLUMN [ExpiresAt]");
            migrationBuilder.Sql("ALTER TABLE [RefreshTokens] DROP COLUMN [RevokedAt]");
            migrationBuilder.Sql("EXEC sp_rename '[RefreshTokens].[CreatedAt_New]', 'CreatedAt', 'COLUMN'");
            migrationBuilder.Sql("EXEC sp_rename '[RefreshTokens].[UpdatedAt_New]', 'UpdatedAt', 'COLUMN'");
            migrationBuilder.Sql("EXEC sp_rename '[RefreshTokens].[ExpiresAt_New]', 'ExpiresAt', 'COLUMN'");
            migrationBuilder.Sql("EXEC sp_rename '[RefreshTokens].[RevokedAt_New]', 'RevokedAt', 'COLUMN'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ── Users ────────────────────────────────────────────────────────────
            migrationBuilder.Sql("ALTER TABLE [Users] ADD [CreatedAt_Old] datetime2 NOT NULL DEFAULT '0001-01-01'");
            migrationBuilder.Sql("ALTER TABLE [Users] ADD [UpdatedAt_Old] datetime2 NULL");

            migrationBuilder.Sql(@"
                UPDATE [Users] SET
                    [CreatedAt_Old] = DATEADD(SECOND, CAST([CreatedAt] AS bigint), '1970-01-01 00:00:00'),
                    [UpdatedAt_Old] = CASE WHEN [UpdatedAt] IS NULL THEN NULL
                                      ELSE DATEADD(SECOND, CAST([UpdatedAt] AS bigint), '1970-01-01 00:00:00') END");

            migrationBuilder.Sql("ALTER TABLE [Users] DROP COLUMN [CreatedAt]");
            migrationBuilder.Sql("ALTER TABLE [Users] DROP COLUMN [UpdatedAt]");
            migrationBuilder.Sql("EXEC sp_rename '[Users].[CreatedAt_Old]', 'CreatedAt', 'COLUMN'");
            migrationBuilder.Sql("EXEC sp_rename '[Users].[UpdatedAt_Old]', 'UpdatedAt', 'COLUMN'");

            // ── RefreshTokens ────────────────────────────────────────────────────
            migrationBuilder.Sql("ALTER TABLE [RefreshTokens] ADD [CreatedAt_Old] datetime2 NOT NULL DEFAULT '0001-01-01'");
            migrationBuilder.Sql("ALTER TABLE [RefreshTokens] ADD [UpdatedAt_Old] datetime2 NULL");
            migrationBuilder.Sql("ALTER TABLE [RefreshTokens] ADD [ExpiresAt_Old] datetime2 NOT NULL DEFAULT '0001-01-01'");
            migrationBuilder.Sql("ALTER TABLE [RefreshTokens] ADD [RevokedAt_Old] datetime2 NULL");

            migrationBuilder.Sql(@"
                UPDATE [RefreshTokens] SET
                    [CreatedAt_Old] = DATEADD(SECOND, CAST([CreatedAt] AS bigint), '1970-01-01 00:00:00'),
                    [UpdatedAt_Old] = CASE WHEN [UpdatedAt] IS NULL THEN NULL
                                      ELSE DATEADD(SECOND, CAST([UpdatedAt] AS bigint), '1970-01-01 00:00:00') END,
                    [ExpiresAt_Old] = DATEADD(SECOND, CAST([ExpiresAt] AS bigint), '1970-01-01 00:00:00'),
                    [RevokedAt_Old] = CASE WHEN [RevokedAt] IS NULL THEN NULL
                                      ELSE DATEADD(SECOND, CAST([RevokedAt] AS bigint), '1970-01-01 00:00:00') END");

            migrationBuilder.Sql("ALTER TABLE [RefreshTokens] DROP COLUMN [CreatedAt]");
            migrationBuilder.Sql("ALTER TABLE [RefreshTokens] DROP COLUMN [UpdatedAt]");
            migrationBuilder.Sql("ALTER TABLE [RefreshTokens] DROP COLUMN [ExpiresAt]");
            migrationBuilder.Sql("ALTER TABLE [RefreshTokens] DROP COLUMN [RevokedAt]");
            migrationBuilder.Sql("EXEC sp_rename '[RefreshTokens].[CreatedAt_Old]', 'CreatedAt', 'COLUMN'");
            migrationBuilder.Sql("EXEC sp_rename '[RefreshTokens].[UpdatedAt_Old]', 'UpdatedAt', 'COLUMN'");
            migrationBuilder.Sql("EXEC sp_rename '[RefreshTokens].[ExpiresAt_Old]', 'ExpiresAt', 'COLUMN'");
            migrationBuilder.Sql("EXEC sp_rename '[RefreshTokens].[RevokedAt_Old]', 'RevokedAt', 'COLUMN'");
        }
    }
}
