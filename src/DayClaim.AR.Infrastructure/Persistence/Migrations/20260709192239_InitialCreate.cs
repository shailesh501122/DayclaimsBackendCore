using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DayClaim.AR.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "importer_configs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientOrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RcmReportType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SourceType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DataFormat = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ScheduleTrigger = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ImportFrequencyCron = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_importer_configs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "importer_data_import_source_references",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientOrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImporterConfigId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ObjectStorageKey = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Sha256Checksum = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ReceivedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RowCount = table.Column<int>(type: "integer", nullable: false),
                    OverallStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    WasFallbackWebUpload = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_importer_data_import_source_references", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "masters_cpt_codes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    IsApprovalPending = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_masters_cpt_codes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "masters_payers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    IsApprovalPending = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_masters_payers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "masters_practices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    IsApprovalPending = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_masters_practices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "masters_providers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Specialty = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    IsApprovalPending = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_masters_providers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "masters_states",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    IsApprovalPending = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_masters_states", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "notes_claim_notes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RcmReportEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScenarioMasterId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    NoteText = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    ActionTaken = table.Column<string>(type: "text", nullable: true),
                    StatusSet = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notes_claim_notes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "notes_scenario_master",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientOrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StatusActionSubActionMapping = table.Column<string>(type: "text", nullable: false),
                    NoteTemplate = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notes_scenario_master", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "patient_master",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientOrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    account_number_ciphertext = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    account_number_blind_index = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    full_name_ciphertext = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    full_name_blind_index = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    dob_ciphertext = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    dob_blind_index = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patient_master", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "rd_ageing_data_sla_history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RcmReportEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    SnapshotDate = table.Column<DateOnly>(type: "date", nullable: false),
                    AgeInDays = table.Column<int>(type: "integer", nullable: false),
                    Bucket = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rd_ageing_data_sla_history", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "rd_enterprise_patient_index",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientOrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientMasterId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PayerMasterId = table.Column<Guid>(type: "uuid", nullable: true),
                    Location = table.Column<string>(type: "text", nullable: true),
                    ProviderMasterId = table.Column<Guid>(type: "uuid", nullable: true),
                    PracticeMasterId = table.Column<Guid>(type: "uuid", nullable: true),
                    CptCode = table.Column<string>(type: "text", nullable: true),
                    FinancialClass = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rd_enterprise_patient_index", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "rd_rcm_reports_data_entries_initial",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientOrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RcmReportType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DataImportSourceReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientMasterId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnterprisePatientIndexId = table.Column<Guid>(type: "uuid", nullable: false),
                    standard_fields = table.Column<string>(type: "jsonb", nullable: false),
                    customer_specific_fields = table.Column<string>(type: "jsonb", nullable: false),
                    custom_defined_fields = table.Column<string>(type: "jsonb", nullable: false),
                    status_storing_fields = table.Column<string>(type: "jsonb", nullable: false),
                    unmapped_fields = table.Column<string>(type: "jsonb", nullable: false),
                    balance_ciphertext = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    balance_blind_index = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Bucket = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Priority = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    OriginalEntryId = table.Column<Guid>(type: "uuid", nullable: true),
                    OpenedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ClosedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rd_rcm_reports_data_entries_initial", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "rd_rcm_reports_data_entry_sync_requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RcmReportEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SyncedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rd_rcm_reports_data_entry_sync_requests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "rule_engine_execution_results",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleExecutionRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    RcmReportEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Bucket = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Priority = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rule_engine_execution_results", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "rule_engine_execution_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientOrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClaimsProcessed = table.Column<int>(type: "integer", nullable: false),
                    ClaimsMatched = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rule_engine_execution_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "rule_engine_rules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Scope = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ClientOrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    PayerMasterId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConditionExpression = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ResultBucket = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ResultPriority = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ExcludeSpecialProjectClaims = table.Column<bool>(type: "boolean", nullable: false),
                    ExcludeManualAssignmentClaims = table.Column<bool>(type: "boolean", nullable: false),
                    EvaluationOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rule_engine_rules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users_client_organizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Size = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users_client_organizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users_permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Module = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users_permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users_refresh_tokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Jti = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RevokedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReplacedByTokenId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedByIp = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users_refresh_tokens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users_roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsClientRole = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users_roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users_users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PrimaryClientOrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    FailedLoginCount = table.Column<int>(type: "integer", nullable: false),
                    LastLoginAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "wfm_allocation_rules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientOrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetBucket = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TargetPriority = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    AllocationType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EqualDistribution = table.Column<bool>(type: "boolean", nullable: false),
                    LocationBased = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wfm_allocation_rules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "wfm_allocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RcmReportEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedToUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    AllocationType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AllocatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RolledBackAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RollbackReason = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wfm_allocations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "wfm_teams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientOrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TeamLeaderUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wfm_teams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "importer_field_mappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ImporterConfigId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceColumnName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    TargetFieldName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Classification = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IsMandatory = table.Column<bool>(type: "boolean", nullable: false),
                    IsUniquePrimaryIdentifier = table.Column<bool>(type: "boolean", nullable: false),
                    IsUniqueSecondaryIdentifier = table.Column<bool>(type: "boolean", nullable: false),
                    ContainsPhi = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_importer_field_mappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_importer_field_mappings_importer_configs_ImporterConfigId",
                        column: x => x.ImporterConfigId,
                        principalTable: "importer_configs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users_role_permissions",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users_role_permissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_users_role_permissions_users_permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "users_permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_users_role_permissions_users_roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "users_roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users_user_organizations",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientOrganizationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users_user_organizations", x => new { x.UserId, x.ClientOrganizationId });
                    table.ForeignKey(
                        name: "FK_users_user_organizations_users_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users_user_roles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users_user_roles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_users_user_roles_users_roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "users_roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_users_user_roles_users_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "wfm_team_members",
                columns: table => new
                {
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsExperienced = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wfm_team_members", x => new { x.TeamId, x.UserId });
                    table.ForeignKey(
                        name: "FK_wfm_team_members_wfm_teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "wfm_teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_importer_configs_ClientOrganizationId_RcmReportType",
                table: "importer_configs",
                columns: new[] { "ClientOrganizationId", "RcmReportType" });

            migrationBuilder.CreateIndex(
                name: "IX_importer_data_import_source_references_ClientOrganizationId~",
                table: "importer_data_import_source_references",
                columns: new[] { "ClientOrganizationId", "ReceivedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_importer_field_mappings_ImporterConfigId",
                table: "importer_field_mappings",
                column: "ImporterConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_masters_cpt_codes_Name",
                table: "masters_cpt_codes",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_masters_payers_Name",
                table: "masters_payers",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_masters_practices_Name",
                table: "masters_practices",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_masters_providers_Name",
                table: "masters_providers",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_masters_states_Name",
                table: "masters_states",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_notes_claim_notes_RcmReportEntryId",
                table: "notes_claim_notes",
                column: "RcmReportEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_notes_scenario_master_ClientOrganizationId",
                table: "notes_scenario_master",
                column: "ClientOrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_patient_master_account_number_blind_index",
                table: "patient_master",
                column: "account_number_blind_index");

            migrationBuilder.CreateIndex(
                name: "IX_patient_master_ClientOrganizationId",
                table: "patient_master",
                column: "ClientOrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_rd_ageing_data_sla_history_RcmReportEntryId_SnapshotDate",
                table: "rd_ageing_data_sla_history",
                columns: new[] { "RcmReportEntryId", "SnapshotDate" });

            migrationBuilder.CreateIndex(
                name: "IX_rd_enterprise_patient_index_ClientOrganizationId_PatientMas~",
                table: "rd_enterprise_patient_index",
                columns: new[] { "ClientOrganizationId", "PatientMasterId", "EncounterNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_rd_rcm_reports_data_entries_initial_ClientOrganizationId_Bu~",
                table: "rd_rcm_reports_data_entries_initial",
                columns: new[] { "ClientOrganizationId", "Bucket", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_rd_rcm_reports_data_entries_initial_ClientOrganizationId_Rc~",
                table: "rd_rcm_reports_data_entries_initial",
                columns: new[] { "ClientOrganizationId", "RcmReportType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_rd_rcm_reports_data_entry_sync_requests_RcmReportEntryId",
                table: "rd_rcm_reports_data_entry_sync_requests",
                column: "RcmReportEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_rule_engine_execution_results_RuleExecutionRunId",
                table: "rule_engine_execution_results",
                column: "RuleExecutionRunId");

            migrationBuilder.CreateIndex(
                name: "IX_rule_engine_execution_runs_ClientOrganizationId_StartedAtUtc",
                table: "rule_engine_execution_runs",
                columns: new[] { "ClientOrganizationId", "StartedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_rule_engine_rules_ClientOrganizationId_IsActive",
                table: "rule_engine_rules",
                columns: new[] { "ClientOrganizationId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_users_client_organizations_Code",
                table: "users_client_organizations",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_permissions_Code",
                table: "users_permissions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_refresh_tokens_TokenHash",
                table: "users_refresh_tokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_role_permissions_PermissionId",
                table: "users_role_permissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_users_roles_Name",
                table: "users_roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_user_roles_RoleId",
                table: "users_user_roles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_users_users_Username",
                table: "users_users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_wfm_allocation_rules_ClientOrganizationId_IsActive",
                table: "wfm_allocation_rules",
                columns: new[] { "ClientOrganizationId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_wfm_allocations_AssignedToUserId",
                table: "wfm_allocations",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_wfm_allocations_RcmReportEntryId",
                table: "wfm_allocations",
                column: "RcmReportEntryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "importer_data_import_source_references");

            migrationBuilder.DropTable(
                name: "importer_field_mappings");

            migrationBuilder.DropTable(
                name: "masters_cpt_codes");

            migrationBuilder.DropTable(
                name: "masters_payers");

            migrationBuilder.DropTable(
                name: "masters_practices");

            migrationBuilder.DropTable(
                name: "masters_providers");

            migrationBuilder.DropTable(
                name: "masters_states");

            migrationBuilder.DropTable(
                name: "notes_claim_notes");

            migrationBuilder.DropTable(
                name: "notes_scenario_master");

            migrationBuilder.DropTable(
                name: "patient_master");

            migrationBuilder.DropTable(
                name: "rd_ageing_data_sla_history");

            migrationBuilder.DropTable(
                name: "rd_enterprise_patient_index");

            migrationBuilder.DropTable(
                name: "rd_rcm_reports_data_entries_initial");

            migrationBuilder.DropTable(
                name: "rd_rcm_reports_data_entry_sync_requests");

            migrationBuilder.DropTable(
                name: "rule_engine_execution_results");

            migrationBuilder.DropTable(
                name: "rule_engine_execution_runs");

            migrationBuilder.DropTable(
                name: "rule_engine_rules");

            migrationBuilder.DropTable(
                name: "users_client_organizations");

            migrationBuilder.DropTable(
                name: "users_refresh_tokens");

            migrationBuilder.DropTable(
                name: "users_role_permissions");

            migrationBuilder.DropTable(
                name: "users_user_organizations");

            migrationBuilder.DropTable(
                name: "users_user_roles");

            migrationBuilder.DropTable(
                name: "wfm_allocation_rules");

            migrationBuilder.DropTable(
                name: "wfm_allocations");

            migrationBuilder.DropTable(
                name: "wfm_team_members");

            migrationBuilder.DropTable(
                name: "importer_configs");

            migrationBuilder.DropTable(
                name: "users_permissions");

            migrationBuilder.DropTable(
                name: "users_roles");

            migrationBuilder.DropTable(
                name: "users_users");

            migrationBuilder.DropTable(
                name: "wfm_teams");
        }
    }
}
