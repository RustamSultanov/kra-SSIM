CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210925180116_Init') THEN
    CREATE TABLE "Admins" (
        "FullName" text NOT NULL,
        "TimeZone" text NULL,
        "PhotoUrl" text NULL,
        "Active" boolean NOT NULL,
        "God" boolean NOT NULL,
        CONSTRAINT "PK_Admins" PRIMARY KEY ("FullName")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210925180116_Init') THEN
    CREATE TABLE "ContactModes" (
        "Name" text NOT NULL,
        CONSTRAINT "PK_ContactModes" PRIMARY KEY ("Name")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210925180116_Init') THEN
    CREATE TABLE "Roles" (
        "Name" text NOT NULL,
        "DisplayOrder" integer NOT NULL,
        CONSTRAINT "PK_Roles" PRIMARY KEY ("Name")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210925180116_Init') THEN
    CREATE TABLE "Schedulers" (
        "Name" text NOT NULL,
        "Description" text NOT NULL,
        CONSTRAINT "PK_Schedulers" PRIMARY KEY ("Name")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210925180116_Init') THEN
    CREATE TABLE "Teams" (
        "Name" text NOT NULL,
        "SlackChannel" text NULL,
        "SlackChannelNotifications" text NULL,
        "Email" text NULL,
        "SchedulingTimezone" text NULL,
        "Active" boolean NOT NULL,
        CONSTRAINT "PK_Teams" PRIMARY KEY ("Name")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210925180116_Init') THEN
    CREATE TABLE "Users" (
        "FullName" text NOT NULL,
        "TimeZone" text NULL,
        "PhotoUrl" text NULL,
        "Active" boolean NOT NULL,
        CONSTRAINT "PK_Users" PRIMARY KEY ("FullName")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210925180116_Init') THEN
    CREATE TABLE "AdminTeam" (
        "AdministrateFullName" text NOT NULL,
        "AdministrateName" text NOT NULL,
        CONSTRAINT "PK_AdminTeam" PRIMARY KEY ("AdministrateFullName", "AdministrateName"),
        CONSTRAINT "FK_AdminTeam_Admins_AdministrateFullName" FOREIGN KEY ("AdministrateFullName") REFERENCES "Admins" ("FullName") ON DELETE CASCADE,
        CONSTRAINT "FK_AdminTeam_Teams_AdministrateName" FOREIGN KEY ("AdministrateName") REFERENCES "Teams" ("Name") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210925180116_Init') THEN
    CREATE TABLE "Rosters" (
        "Name" text NOT NULL,
        "TeamName" text NOT NULL,
        CONSTRAINT "PK_Rosters" PRIMARY KEY ("Name"),
        CONSTRAINT "FK_Rosters_Teams_TeamName" FOREIGN KEY ("TeamName") REFERENCES "Teams" ("Name") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210925180116_Init') THEN
    CREATE TABLE "TeamUser" (
        "DutyFullName" text NOT NULL,
        "DutyName" text NOT NULL,
        CONSTRAINT "PK_TeamUser" PRIMARY KEY ("DutyFullName", "DutyName"),
        CONSTRAINT "FK_TeamUser_Teams_DutyName" FOREIGN KEY ("DutyName") REFERENCES "Teams" ("Name") ON DELETE CASCADE,
        CONSTRAINT "FK_TeamUser_Users_DutyFullName" FOREIGN KEY ("DutyFullName") REFERENCES "Users" ("FullName") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210925180116_Init') THEN
    CREATE TABLE "UserContacts" (
        "UserFullName" text NOT NULL,
        "ContactModeName" text NOT NULL,
        "Destination" text NOT NULL,
        CONSTRAINT "PK_UserContacts" PRIMARY KEY ("UserFullName", "ContactModeName"),
        CONSTRAINT "FK_UserContacts_ContactModes_ContactModeName" FOREIGN KEY ("ContactModeName") REFERENCES "ContactModes" ("Name") ON DELETE CASCADE,
        CONSTRAINT "FK_UserContacts_Users_UserFullName" FOREIGN KEY ("UserFullName") REFERENCES "Users" ("FullName") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210925180116_Init') THEN
    CREATE TABLE "RosterUsers" (
        "UserFullName" text NOT NULL,
        "RosterName" text NOT NULL,
        "RosterPriority" integer NOT NULL,
        "InRotation" boolean NOT NULL,
        CONSTRAINT "PK_RosterUsers" PRIMARY KEY ("UserFullName", "RosterName"),
        CONSTRAINT "FK_RosterUsers_Rosters_RosterName" FOREIGN KEY ("RosterName") REFERENCES "Rosters" ("Name") ON DELETE CASCADE,
        CONSTRAINT "FK_RosterUsers_Users_UserFullName" FOREIGN KEY ("UserFullName") REFERENCES "Users" ("FullName") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210925180116_Init') THEN
    CREATE TABLE "Schedules" (
        "Id" integer GENERATED BY DEFAULT AS IDENTITY,
        "AutoPopulateThreshold" integer NOT NULL,
        "LastEpochScheduled" bigint NOT NULL,
        "AdvancedMode" boolean NOT NULL,
        "SchedulerName" text NULL,
        "TeamName" text NULL,
        "RoleName" text NULL,
        "RosterName" text NULL,
        CONSTRAINT "PK_Schedules" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Schedules_Roles_RoleName" FOREIGN KEY ("RoleName") REFERENCES "Roles" ("Name"),
        CONSTRAINT "FK_Schedules_Rosters_RosterName" FOREIGN KEY ("RosterName") REFERENCES "Rosters" ("Name"),
        CONSTRAINT "FK_Schedules_Schedulers_SchedulerName" FOREIGN KEY ("SchedulerName") REFERENCES "Schedulers" ("Name"),
        CONSTRAINT "FK_Schedules_Teams_TeamName" FOREIGN KEY ("TeamName") REFERENCES "Teams" ("Name")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210925180116_Init') THEN
    CREATE TABLE "Events" (
        "Id" bigint GENERATED BY DEFAULT AS IDENTITY,
        "Start" bigint NOT NULL,
        "End" bigint NOT NULL,
        "Note" text NULL,
        "ScheduleId" integer NULL,
        "TeamName" text NULL,
        "RoleName" text NULL,
        "UserFullName" text NULL,
        CONSTRAINT "PK_Events" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Events_Roles_RoleName" FOREIGN KEY ("RoleName") REFERENCES "Roles" ("Name"),
        CONSTRAINT "FK_Events_Schedules_ScheduleId" FOREIGN KEY ("ScheduleId") REFERENCES "Schedules" ("Id"),
        CONSTRAINT "FK_Events_Teams_TeamName" FOREIGN KEY ("TeamName") REFERENCES "Teams" ("Name"),
        CONSTRAINT "FK_Events_Users_UserFullName" FOREIGN KEY ("UserFullName") REFERENCES "Users" ("FullName")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210925180116_Init') THEN
    CREATE INDEX "IX_AdminTeam_AdministrateName" ON "AdminTeam" ("AdministrateName");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210925180116_Init') THEN
    CREATE INDEX "IX_Events_RoleName" ON "Events" ("RoleName");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210925180116_Init') THEN
    CREATE INDEX "IX_Events_ScheduleId" ON "Events" ("ScheduleId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210925180116_Init') THEN
    CREATE INDEX "IX_Events_TeamName" ON "Events" ("TeamName");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210925180116_Init') THEN
    CREATE INDEX "IX_Events_UserFullName" ON "Events" ("UserFullName");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210925180116_Init') THEN
    CREATE INDEX "IX_Rosters_TeamName" ON "Rosters" ("TeamName");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210925180116_Init') THEN
    CREATE INDEX "IX_RosterUsers_RosterName" ON "RosterUsers" ("RosterName");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210925180116_Init') THEN
    CREATE INDEX "IX_Schedules_RoleName" ON "Schedules" ("RoleName");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210925180116_Init') THEN
    CREATE INDEX "IX_Schedules_RosterName" ON "Schedules" ("RosterName");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210925180116_Init') THEN
    CREATE INDEX "IX_Schedules_SchedulerName" ON "Schedules" ("SchedulerName");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210925180116_Init') THEN
    CREATE INDEX "IX_Schedules_TeamName" ON "Schedules" ("TeamName");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210925180116_Init') THEN
    CREATE INDEX "IX_TeamUser_DutyName" ON "TeamUser" ("DutyName");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210925180116_Init') THEN
    CREATE INDEX "IX_UserContacts_ContactModeName" ON "UserContacts" ("ContactModeName");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210925180116_Init') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20210925180116_Init', '6.0.0-rc.1.21452.10');
    END IF;
END $EF$;
COMMIT;

