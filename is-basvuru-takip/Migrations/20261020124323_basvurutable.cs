using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace is_basvuru_takip.Migrations
{
    /// <inheritdoc />
    public partial class basvurutable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Basvurular",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    SirketAdi = table.Column<string>(type: "text", nullable: false),
                    Pozisyon = table.Column<string>(type: "text", nullable: false),
                    BasvuruTarihi = table.Column<DateOnly>(type: "date", nullable: false),
                    Maas = table.Column<decimal>(type: "numeric", nullable: true),
                    Durum = table.Column<int>(type: "integer", nullable: false),
                    Sekil = table.Column<int>(type: "integer", nullable: false),
                    Kaynak = table.Column<int>(type: "integer", nullable: false),
                    IlanLinki = table.Column<string>(type: "text", nullable: true),
                    Notlar = table.Column<string>(type: "text", nullable: true),
                    Sehir = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Basvurular", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Basvurular_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Basvurular_UserId",
                table: "Basvurular",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Basvurular");
        }
    }
}
