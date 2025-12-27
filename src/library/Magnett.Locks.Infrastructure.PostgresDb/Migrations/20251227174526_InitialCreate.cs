using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Magnett.Locks.Infrastructure.PostgresDb.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "locks",
                columns: table => new
                {
                    tenant_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    environment = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    @namespace = table.Column<string>(name: "namespace", type: "character varying(255)", maxLength: 255, nullable: false),
                    resource_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    lock_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    owner_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_locks", x => new { x.tenant_id, x.environment, x.@namespace, x.resource_id });
                });

            migrationBuilder.CreateIndex(
                name: "idx_locks_expires_at",
                table: "locks",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "idx_locks_lock_id",
                table: "locks",
                column: "lock_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "locks");
        }
    }
}
