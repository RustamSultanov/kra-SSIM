using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace kraSSIM.Migrations
{
    public partial class InitVertical : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Concepts",
                columns: table => new
                {
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Concepts", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "Individs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Individs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Attributes",
                columns: table => new
                {
                    Name = table.Column<string>(type: "text", nullable: false),
                    ActualFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConceptName = table.Column<string>(type: "text", nullable: false),
                    ActualTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attributes", x => new { x.Name, x.ConceptName, x.ActualFrom });
                    table.ForeignKey(
                        name: "FK_Attributes_Concepts_ConceptName",
                        column: x => x.ConceptName,
                        principalTable: "Concepts",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AttributeValues",
                columns: table => new
                {
                    ActualFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IndividId = table.Column<int>(type: "integer", nullable: false),
                    AttributeName = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    ActualTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AttributeConceptName = table.Column<string>(type: "text", nullable: false),
                    AttributeActualFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttributeValues", x => new { x.IndividId, x.AttributeName, x.ActualFrom });
                    table.ForeignKey(
                        name: "FK_AttributeValues_Attributes_AttributeName_AttributeConceptNa~",
                        columns: x => new { x.AttributeName, x.AttributeConceptName, x.AttributeActualFrom },
                        principalTable: "Attributes",
                        principalColumns: new[] { "Name", "ConceptName", "ActualFrom" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AttributeValues_Individs_IndividId",
                        column: x => x.IndividId,
                        principalTable: "Individs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attributes_ConceptName",
                table: "Attributes",
                column: "ConceptName");

            migrationBuilder.CreateIndex(
                name: "IX_AttributeValues_AttributeName_AttributeConceptName_Attribut~",
                table: "AttributeValues",
                columns: new[] { "AttributeName", "AttributeConceptName", "AttributeActualFrom" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttributeValues");

            migrationBuilder.DropTable(
                name: "Attributes");

            migrationBuilder.DropTable(
                name: "Individs");

            migrationBuilder.DropTable(
                name: "Concepts");
        }
    }
}
