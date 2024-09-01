using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ETravelApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddingPackageTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomerData",
                columns: table => new
                {
                    CustomerDataId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerData", x => x.CustomerDataId);
                    table.ForeignKey(
                        name: "FK_CustomerData_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Packages",
                columns: table => new
                {
                    PackageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PackageName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Destination = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<int>(type: "int", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Packages", x => x.PackageId);
                });

            migrationBuilder.CreateTable(
                name: "CustomerFiles",
                columns: table => new
                {
                    CustomerFileId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Filename = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Filetype = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Filesize = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Filebytes = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    CustomerDataId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerFiles", x => x.CustomerFileId);
                    table.ForeignKey(
                        name: "FK_CustomerFiles_CustomerData_CustomerDataId",
                        column: x => x.CustomerDataId,
                        principalTable: "CustomerData",
                        principalColumn: "CustomerDataId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PackageData",
                columns: table => new
                {
                    PackageDataId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ViaDestination = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AvailableSeat = table.Column<int>(type: "int", nullable: false),
                    PackageId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageData", x => x.PackageDataId);
                    table.ForeignKey(
                        name: "FK_PackageData_Packages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "Packages",
                        principalColumn: "PackageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PackageImages",
                columns: table => new
                {
                    PackageImageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    filename = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    filetype = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    filesize = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    filebytes = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    PackageDataId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageImages", x => x.PackageImageId);
                    table.ForeignKey(
                        name: "FK_PackageImages_PackageData_PackageDataId",
                        column: x => x.PackageDataId,
                        principalTable: "PackageData",
                        principalColumn: "PackageDataId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerData_UserId",
                table: "CustomerData",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerFiles_CustomerDataId",
                table: "CustomerFiles",
                column: "CustomerDataId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageData_PackageId",
                table: "PackageData",
                column: "PackageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PackageImages_PackageDataId",
                table: "PackageImages",
                column: "PackageDataId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerFiles");

            migrationBuilder.DropTable(
                name: "PackageImages");

            migrationBuilder.DropTable(
                name: "CustomerData");

            migrationBuilder.DropTable(
                name: "PackageData");

            migrationBuilder.DropTable(
                name: "Packages");
        }
    }
}
