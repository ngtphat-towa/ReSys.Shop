using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace ReSys.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:example_status", "draft,active,archived")
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.CreateTable(
                name: "example_categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_example_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "examples",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    image_url = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    hex_color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_examples", x => x.id);
                    table.ForeignKey(
                        name: "fk_examples_example_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "example_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "example_embedding",
                columns: table => new
                {
                    example_id = table.Column<Guid>(type: "uuid", nullable: false),
                    embedding = table.Column<Vector>(type: "vector(384)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_example_embedding", x => x.example_id);
                    table.ForeignKey(
                        name: "fk_example_embedding_examples_example_id",
                        column: x => x.example_id,
                        principalTable: "examples",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_example_categories_name",
                table: "example_categories",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_examples_category_id",
                table: "examples",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_examples_name",
                table: "examples",
                column: "name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "example_embedding");

            migrationBuilder.DropTable(
                name: "examples");

            migrationBuilder.DropTable(
                name: "example_categories");
        }
    }
}
