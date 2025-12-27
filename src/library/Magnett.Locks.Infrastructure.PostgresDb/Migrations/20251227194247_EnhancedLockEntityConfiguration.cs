using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Magnett.Locks.Infrastructure.PostgresDb.Migrations
{
    /// <inheritdoc />
    public partial class EnhancedLockEntityConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_locks_expires_at",
                table: "locks");

            migrationBuilder.DropIndex(
                name: "idx_locks_lock_id",
                table: "locks");

            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.RenameTable(
                name: "locks",
                newName: "locks",
                newSchema: "public");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                schema: "public",
                table: "locks",
                type: "TIMESTAMP WITH TIME ZONE",
                nullable: false,
                defaultValueSql: "NOW()",
                comment: "Timestamp when the lock was last updated (UTC)",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "NOW()");

            migrationBuilder.AlterColumn<string>(
                name: "owner_id",
                schema: "public",
                table: "locks",
                type: "VARCHAR(255)",
                maxLength: 255,
                nullable: false,
                comment: "Identifier of the process/instance that owns this lock",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "lock_id",
                schema: "public",
                table: "locks",
                type: "VARCHAR(255)",
                maxLength: 255,
                nullable: false,
                comment: "Unique identifier for this specific lock instance",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<DateTime>(
                name: "expires_at",
                schema: "public",
                table: "locks",
                type: "TIMESTAMP WITH TIME ZONE",
                nullable: false,
                comment: "Timestamp when the lock expires (UTC)",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                schema: "public",
                table: "locks",
                type: "TIMESTAMP WITH TIME ZONE",
                nullable: false,
                defaultValueSql: "NOW()",
                comment: "Timestamp when the lock was created (UTC)",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "NOW()");

            migrationBuilder.AlterColumn<string>(
                name: "resource_id",
                schema: "public",
                table: "locks",
                type: "VARCHAR(255)",
                maxLength: 255,
                nullable: false,
                comment: "Unique identifier of the resource being locked",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "namespace",
                schema: "public",
                table: "locks",
                type: "VARCHAR(255)",
                maxLength: 255,
                nullable: false,
                comment: "Logical namespace for organizing locks",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "environment",
                schema: "public",
                table: "locks",
                type: "VARCHAR(255)",
                maxLength: 255,
                nullable: false,
                comment: "Environment name (e.g., production, staging, development)",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "tenant_id",
                schema: "public",
                table: "locks",
                type: "VARCHAR(255)",
                maxLength: 255,
                nullable: false,
                comment: "Tenant identifier for multi-tenancy support",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.CreateIndex(
                name: "idx_locks_expires_at",
                schema: "public",
                table: "locks",
                column: "expires_at",
                filter: "\"expires_at\" > NOW()")
                .Annotation("Npgsql:IndexMethod", "btree");

            migrationBuilder.CreateIndex(
                name: "idx_locks_lock_id",
                schema: "public",
                table: "locks",
                column: "lock_id",
                unique: true)
                .Annotation("Npgsql:IndexMethod", "btree");

            migrationBuilder.CreateIndex(
                name: "idx_locks_tenant_env_namespace",
                schema: "public",
                table: "locks",
                columns: new[] { "tenant_id", "environment", "namespace" })
                .Annotation("Npgsql:IndexMethod", "btree");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_locks_expires_at",
                schema: "public",
                table: "locks");

            migrationBuilder.DropIndex(
                name: "idx_locks_lock_id",
                schema: "public",
                table: "locks");

            migrationBuilder.DropIndex(
                name: "idx_locks_tenant_env_namespace",
                schema: "public",
                table: "locks");

            migrationBuilder.RenameTable(
                name: "locks",
                schema: "public",
                newName: "locks");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "locks",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()",
                oldClrType: typeof(DateTime),
                oldType: "TIMESTAMP WITH TIME ZONE",
                oldDefaultValueSql: "NOW()",
                oldComment: "Timestamp when the lock was last updated (UTC)");

            migrationBuilder.AlterColumn<string>(
                name: "owner_id",
                table: "locks",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(255)",
                oldMaxLength: 255,
                oldComment: "Identifier of the process/instance that owns this lock");

            migrationBuilder.AlterColumn<string>(
                name: "lock_id",
                table: "locks",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(255)",
                oldMaxLength: 255,
                oldComment: "Unique identifier for this specific lock instance");

            migrationBuilder.AlterColumn<DateTime>(
                name: "expires_at",
                table: "locks",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TIMESTAMP WITH TIME ZONE",
                oldComment: "Timestamp when the lock expires (UTC)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "locks",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()",
                oldClrType: typeof(DateTime),
                oldType: "TIMESTAMP WITH TIME ZONE",
                oldDefaultValueSql: "NOW()",
                oldComment: "Timestamp when the lock was created (UTC)");

            migrationBuilder.AlterColumn<string>(
                name: "resource_id",
                table: "locks",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(255)",
                oldMaxLength: 255,
                oldComment: "Unique identifier of the resource being locked");

            migrationBuilder.AlterColumn<string>(
                name: "namespace",
                table: "locks",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(255)",
                oldMaxLength: 255,
                oldComment: "Logical namespace for organizing locks");

            migrationBuilder.AlterColumn<string>(
                name: "environment",
                table: "locks",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(255)",
                oldMaxLength: 255,
                oldComment: "Environment name (e.g., production, staging, development)");

            migrationBuilder.AlterColumn<string>(
                name: "tenant_id",
                table: "locks",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(255)",
                oldMaxLength: 255,
                oldComment: "Tenant identifier for multi-tenancy support");

            migrationBuilder.CreateIndex(
                name: "idx_locks_expires_at",
                table: "locks",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "idx_locks_lock_id",
                table: "locks",
                column: "lock_id");
        }
    }
}
