using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalSMS.DAL.Migrations
{
    /// <inheritdoc />
    public partial class Migration5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Logs_SentMessages_MessageID",
                table: "Logs");

            migrationBuilder.DropIndex(
                name: "IX_Logs_MessageID",
                table: "Logs");

            migrationBuilder.AlterColumn<string>(
                name: "MessageID",
                table: "Logs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "SentMessageMessageID",
                table: "Logs",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Logs_SentMessageMessageID",
                table: "Logs",
                column: "SentMessageMessageID");

            migrationBuilder.AddForeignKey(
                name: "FK_Logs_SentMessages_SentMessageMessageID",
                table: "Logs",
                column: "SentMessageMessageID",
                principalTable: "SentMessages",
                principalColumn: "MessageID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Logs_SentMessages_SentMessageMessageID",
                table: "Logs");

            migrationBuilder.DropIndex(
                name: "IX_Logs_SentMessageMessageID",
                table: "Logs");

            migrationBuilder.DropColumn(
                name: "SentMessageMessageID",
                table: "Logs");

            migrationBuilder.AlterColumn<string>(
                name: "MessageID",
                table: "Logs",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Logs_MessageID",
                table: "Logs",
                column: "MessageID");

            migrationBuilder.AddForeignKey(
                name: "FK_Logs_SentMessages_MessageID",
                table: "Logs",
                column: "MessageID",
                principalTable: "SentMessages",
                principalColumn: "MessageID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
