using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rapsodia.Blue.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssetTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OLP_Credentials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    UserId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Key = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: false),
                    EncryptedValue = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Environment = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: false),
                    Category = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OLP_Credentials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OLP_Documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    UserId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(512)", maxLength: 512, nullable: false),
                    FileName = table.Column<string>(type: "NVARCHAR2(512)", maxLength: 512, nullable: false),
                    ContentType = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: false),
                    Size = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    StoragePath = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Category = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: false),
                    Tags = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OLP_Documents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OLP_TotpAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    UserId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Uri = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Secret = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: false),
                    Label = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: false),
                    Issuer = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: false),
                    Algorithm = table.Column<string>(type: "NVARCHAR2(16)", maxLength: 16, nullable: false),
                    Digits = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Period = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OLP_TotpAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SYS_SyncQueue",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    EntityType = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: false),
                    EntityId = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: false),
                    Operation = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: false),
                    Payload = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Status = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    RetryCount = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    MaxRetries = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    LastError = table.Column<string>(type: "NVARCHAR2(1024)", maxLength: 1024, nullable: true),
                    SyncedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SYS_SyncQueue", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Username = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    PasswordHash = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Role = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Email = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    FullName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    AllowedModules = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vulns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Code = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Title = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Description = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Level = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Environment = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ParentVulnId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vulns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vulns_Vulns_ParentVulnId",
                        column: x => x.ParentVulnId,
                        principalTable: "Vulns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Assets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    AssetTypeId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Environment = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    IsEnabled = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    ParentAssetId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assets_AssetTypes_AssetTypeId",
                        column: x => x.AssetTypeId,
                        principalTable: "AssetTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Assets_Assets_ParentAssetId",
                        column: x => x.ParentAssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VulnVuln",
                columns: table => new
                {
                    RelatedVulnsId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    VulnId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VulnVuln", x => new { x.RelatedVulnsId, x.VulnId });
                    table.ForeignKey(
                        name: "FK_VulnVuln_Vulns_RelatedVulnsId",
                        column: x => x.RelatedVulnsId,
                        principalTable: "Vulns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VulnVuln_Vulns_VulnId",
                        column: x => x.VulnId,
                        principalTable: "Vulns",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AssetAsset",
                columns: table => new
                {
                    AssetId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    RelatedAssetsId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetAsset", x => new { x.AssetId, x.RelatedAssetsId });
                    table.ForeignKey(
                        name: "FK_AssetAsset_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssetAsset_Assets_RelatedAssetsId",
                        column: x => x.RelatedAssetsId,
                        principalTable: "Assets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AssetVulns",
                columns: table => new
                {
                    AssetId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    VulnId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DiscoveredAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    Status = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Notes = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    RemediatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetVulns", x => new { x.AssetId, x.VulnId });
                    table.ForeignKey(
                        name: "FK_AssetVulns_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssetVulns_Vulns_VulnId",
                        column: x => x.VulnId,
                        principalTable: "Vulns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssetAsset_RelatedAssetsId",
                table: "AssetAsset",
                column: "RelatedAssetsId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_AssetTypeId",
                table: "Assets",
                column: "AssetTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_ParentAssetId",
                table: "Assets",
                column: "ParentAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetVulns_VulnId",
                table: "AssetVulns",
                column: "VulnId");

            migrationBuilder.CreateIndex(
                name: "IX_OLP_Credentials_UserId_Key_Environment",
                table: "OLP_Credentials",
                columns: new[] { "UserId", "Key", "Environment" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OLP_TotpAccounts_UserId_Issuer_Label",
                table: "OLP_TotpAccounts",
                columns: new[] { "UserId", "Issuer", "Label" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SYS_SyncQueue_Status",
                table: "SYS_SyncQueue",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vulns_ParentVulnId",
                table: "Vulns",
                column: "ParentVulnId");

            migrationBuilder.CreateIndex(
                name: "IX_VulnVuln_VulnId",
                table: "VulnVuln",
                column: "VulnId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssetAsset");

            migrationBuilder.DropTable(
                name: "AssetVulns");

            migrationBuilder.DropTable(
                name: "OLP_Credentials");

            migrationBuilder.DropTable(
                name: "OLP_Documents");

            migrationBuilder.DropTable(
                name: "OLP_TotpAccounts");

            migrationBuilder.DropTable(
                name: "SYS_SyncQueue");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "VulnVuln");

            migrationBuilder.DropTable(
                name: "Assets");

            migrationBuilder.DropTable(
                name: "Vulns");

            migrationBuilder.DropTable(
                name: "AssetTypes");
        }
    }
}
