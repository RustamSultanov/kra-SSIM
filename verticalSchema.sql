CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210925224312_InitVertical') THEN
    CREATE TABLE "Concepts" (
        "Name" text NOT NULL,
        CONSTRAINT "PK_Concepts" PRIMARY KEY ("Name")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210925224312_InitVertical') THEN
    CREATE TABLE "Individs" (
        "Id" integer GENERATED BY DEFAULT AS IDENTITY,
        CONSTRAINT "PK_Individs" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210925224312_InitVertical') THEN
    CREATE TABLE "Attributes" (
        "Name" text NOT NULL,
        "ActualFrom" timestamp with time zone NOT NULL,
        "ConceptName" text NOT NULL,
        "ActualTo" timestamp with time zone NULL,
        CONSTRAINT "PK_Attributes" PRIMARY KEY ("Name", "ConceptName", "ActualFrom"),
        CONSTRAINT "FK_Attributes_Concepts_ConceptName" FOREIGN KEY ("ConceptName") REFERENCES "Concepts" ("Name") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210925224312_InitVertical') THEN
    CREATE TABLE "AttributeValues" (
        "ActualFrom" timestamp with time zone NOT NULL,
        "IndividId" integer NOT NULL,
        "AttributeName" text NOT NULL,
        "Id" integer NOT NULL,
        "Value" jsonb NOT NULL,
        "ActualTo" timestamp with time zone NULL,
        "AttributeConceptName" text NOT NULL,
        "AttributeActualFrom" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_AttributeValues" PRIMARY KEY ("IndividId", "AttributeName", "ActualFrom"),
        CONSTRAINT "FK_AttributeValues_Attributes_AttributeName_AttributeConceptNa~" FOREIGN KEY ("AttributeName", "AttributeConceptName", "AttributeActualFrom") REFERENCES "Attributes" ("Name", "ConceptName", "ActualFrom") ON DELETE CASCADE,
        CONSTRAINT "FK_AttributeValues_Individs_IndividId" FOREIGN KEY ("IndividId") REFERENCES "Individs" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210925224312_InitVertical') THEN
    CREATE INDEX "IX_Attributes_ConceptName" ON "Attributes" ("ConceptName");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210925224312_InitVertical') THEN
    CREATE INDEX "IX_AttributeValues_AttributeName_AttributeConceptName_Attribut~" ON "AttributeValues" ("AttributeName", "AttributeConceptName", "AttributeActualFrom");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20210925224312_InitVertical') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20210925224312_InitVertical', '6.0.0-rc.1.21452.10');
    END IF;
END $EF$;
COMMIT;

