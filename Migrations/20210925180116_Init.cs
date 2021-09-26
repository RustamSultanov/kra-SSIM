using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace kraSSIM.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    FullName = table.Column<string>(type: "text", nullable: false),
                    TimeZone = table.Column<string>(type: "text", nullable: true),
                    PhotoUrl = table.Column<string>(type: "text", nullable: true),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    God = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.FullName);
                });

            migrationBuilder.CreateTable(
                name: "ContactModes",
                columns: table => new
                {
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactModes", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Name = table.Column<string>(type: "text", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "Schedulers",
                columns: table => new
                {
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedulers", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    Name = table.Column<string>(type: "text", nullable: false),
                    SlackChannel = table.Column<string>(type: "text", nullable: true),
                    SlackChannelNotifications = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    SchedulingTimezone = table.Column<string>(type: "text", nullable: true),
                    Active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    FullName = table.Column<string>(type: "text", nullable: false),
                    TimeZone = table.Column<string>(type: "text", nullable: true),
                    PhotoUrl = table.Column<string>(type: "text", nullable: true),
                    Active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.FullName);
                });

            migrationBuilder.CreateTable(
                name: "AdminTeam",
                columns: table => new
                {
                    AdministrateFullName = table.Column<string>(type: "text", nullable: false),
                    AdministrateName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminTeam", x => new { x.AdministrateFullName, x.AdministrateName });
                    table.ForeignKey(
                        name: "FK_AdminTeam_Admins_AdministrateFullName",
                        column: x => x.AdministrateFullName,
                        principalTable: "Admins",
                        principalColumn: "FullName",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdminTeam_Teams_AdministrateName",
                        column: x => x.AdministrateName,
                        principalTable: "Teams",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Rosters",
                columns: table => new
                {
                    Name = table.Column<string>(type: "text", nullable: false),
                    TeamName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rosters", x => x.Name);
                    table.ForeignKey(
                        name: "FK_Rosters_Teams_TeamName",
                        column: x => x.TeamName,
                        principalTable: "Teams",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeamUser",
                columns: table => new
                {
                    DutyFullName = table.Column<string>(type: "text", nullable: false),
                    DutyName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamUser", x => new { x.DutyFullName, x.DutyName });
                    table.ForeignKey(
                        name: "FK_TeamUser_Teams_DutyName",
                        column: x => x.DutyName,
                        principalTable: "Teams",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamUser_Users_DutyFullName",
                        column: x => x.DutyFullName,
                        principalTable: "Users",
                        principalColumn: "FullName",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserContacts",
                columns: table => new
                {
                    UserFullName = table.Column<string>(type: "text", nullable: false),
                    ContactModeName = table.Column<string>(type: "text", nullable: false),
                    Destination = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserContacts", x => new { x.UserFullName, x.ContactModeName });
                    table.ForeignKey(
                        name: "FK_UserContacts_ContactModes_ContactModeName",
                        column: x => x.ContactModeName,
                        principalTable: "ContactModes",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserContacts_Users_UserFullName",
                        column: x => x.UserFullName,
                        principalTable: "Users",
                        principalColumn: "FullName",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RosterUsers",
                columns: table => new
                {
                    UserFullName = table.Column<string>(type: "text", nullable: false),
                    RosterName = table.Column<string>(type: "text", nullable: false),
                    RosterPriority = table.Column<int>(type: "integer", nullable: false),
                    InRotation = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RosterUsers", x => new { x.UserFullName, x.RosterName });
                    table.ForeignKey(
                        name: "FK_RosterUsers_Rosters_RosterName",
                        column: x => x.RosterName,
                        principalTable: "Rosters",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RosterUsers_Users_UserFullName",
                        column: x => x.UserFullName,
                        principalTable: "Users",
                        principalColumn: "FullName",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Schedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AutoPopulateThreshold = table.Column<int>(type: "integer", nullable: false),
                    LastEpochScheduled = table.Column<long>(type: "bigint", nullable: false),
                    AdvancedMode = table.Column<bool>(type: "boolean", nullable: false),
                    SchedulerName = table.Column<string>(type: "text", nullable: true),
                    TeamName = table.Column<string>(type: "text", nullable: true),
                    RoleName = table.Column<string>(type: "text", nullable: true),
                    RosterName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Schedules_Roles_RoleName",
                        column: x => x.RoleName,
                        principalTable: "Roles",
                        principalColumn: "Name");
                    table.ForeignKey(
                        name: "FK_Schedules_Rosters_RosterName",
                        column: x => x.RosterName,
                        principalTable: "Rosters",
                        principalColumn: "Name");
                    table.ForeignKey(
                        name: "FK_Schedules_Schedulers_SchedulerName",
                        column: x => x.SchedulerName,
                        principalTable: "Schedulers",
                        principalColumn: "Name");
                    table.ForeignKey(
                        name: "FK_Schedules_Teams_TeamName",
                        column: x => x.TeamName,
                        principalTable: "Teams",
                        principalColumn: "Name");
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Start = table.Column<long>(type: "bigint", nullable: false),
                    End = table.Column<long>(type: "bigint", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    ScheduleId = table.Column<int>(type: "integer", nullable: true),
                    TeamName = table.Column<string>(type: "text", nullable: true),
                    RoleName = table.Column<string>(type: "text", nullable: true),
                    UserFullName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Events_Roles_RoleName",
                        column: x => x.RoleName,
                        principalTable: "Roles",
                        principalColumn: "Name");
                    table.ForeignKey(
                        name: "FK_Events_Schedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "Schedules",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Events_Teams_TeamName",
                        column: x => x.TeamName,
                        principalTable: "Teams",
                        principalColumn: "Name");
                    table.ForeignKey(
                        name: "FK_Events_Users_UserFullName",
                        column: x => x.UserFullName,
                        principalTable: "Users",
                        principalColumn: "FullName");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdminTeam_AdministrateName",
                table: "AdminTeam",
                column: "AdministrateName");

            migrationBuilder.CreateIndex(
                name: "IX_Events_RoleName",
                table: "Events",
                column: "RoleName");

            migrationBuilder.CreateIndex(
                name: "IX_Events_ScheduleId",
                table: "Events",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_TeamName",
                table: "Events",
                column: "TeamName");

            migrationBuilder.CreateIndex(
                name: "IX_Events_UserFullName",
                table: "Events",
                column: "UserFullName");

            migrationBuilder.CreateIndex(
                name: "IX_Rosters_TeamName",
                table: "Rosters",
                column: "TeamName");

            migrationBuilder.CreateIndex(
                name: "IX_RosterUsers_RosterName",
                table: "RosterUsers",
                column: "RosterName");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_RoleName",
                table: "Schedules",
                column: "RoleName");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_RosterName",
                table: "Schedules",
                column: "RosterName");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_SchedulerName",
                table: "Schedules",
                column: "SchedulerName");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_TeamName",
                table: "Schedules",
                column: "TeamName");

            migrationBuilder.CreateIndex(
                name: "IX_TeamUser_DutyName",
                table: "TeamUser",
                column: "DutyName");

            migrationBuilder.CreateIndex(
                name: "IX_UserContacts_ContactModeName",
                table: "UserContacts",
                column: "ContactModeName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminTeam");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "RosterUsers");

            migrationBuilder.DropTable(
                name: "TeamUser");

            migrationBuilder.DropTable(
                name: "UserContacts");

            migrationBuilder.DropTable(
                name: "Admins");

            migrationBuilder.DropTable(
                name: "Schedules");

            migrationBuilder.DropTable(
                name: "ContactModes");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Rosters");

            migrationBuilder.DropTable(
                name: "Schedulers");

            migrationBuilder.DropTable(
                name: "Teams");
        }
    }
}
