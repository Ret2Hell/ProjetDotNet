using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetDotNet.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FileCollectionModelId",
                table: "Files",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FileCollections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    ParentCollectionId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileCollections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileCollections_FileCollections_ParentCollectionId",
                        column: x => x.ParentCollectionId,
                        principalTable: "FileCollections",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Files_FileCollectionModelId",
                table: "Files",
                column: "FileCollectionModelId");

            migrationBuilder.CreateIndex(
                name: "IX_FileCollections_ParentCollectionId",
                table: "FileCollections",
                column: "ParentCollectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_FileCollections_FileCollectionModelId",
                table: "Files",
                column: "FileCollectionModelId",
                principalTable: "FileCollections",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_FileCollections_FileCollectionModelId",
                table: "Files");

            migrationBuilder.DropTable(
                name: "FileCollections");

            migrationBuilder.DropIndex(
                name: "IX_Files_FileCollectionModelId",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "FileCollectionModelId",
                table: "Files");
        }
    }
}
