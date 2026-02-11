using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RailcarTrips.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TimeZone = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventCodeDefinitions",
                columns: table => new
                {
                    Code = table.Column<string>(type: "TEXT", maxLength: 1, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    LongDescription = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventCodeDefinitions", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "Trips",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EquipmentId = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    OriginCityId = table.Column<int>(type: "INTEGER", nullable: false),
                    DestinationCityId = table.Column<int>(type: "INTEGER", nullable: false),
                    StartUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TotalTripHours = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trips_Cities_DestinationCityId",
                        column: x => x.DestinationCityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Trips_Cities_OriginCityId",
                        column: x => x.OriginCityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EquipmentEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EquipmentId = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    EventCode = table.Column<string>(type: "TEXT", maxLength: 1, nullable: false),
                    EventTimeLocal = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EventTimeUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CityId = table.Column<int>(type: "INTEGER", nullable: false),
                    TripId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EquipmentEvents_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EquipmentEvents_Trips_TripId",
                        column: x => x.TripId,
                        principalTable: "Trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "Name", "TimeZone" },
                values: new object[,]
                {
                    { 1, "Vancouver", "Pacific Standard Time" },
                    { 2, "Victoria", "Pacific Standard Time" },
                    { 3, "Kelowna", "Pacific Standard Time" },
                    { 4, "Kamloops", "Pacific Standard Time" },
                    { 5, "Prince George", "Pacific Standard Time" },
                    { 6, "Calgary", "Mountain Standard Time" },
                    { 7, "Edmonton", "Mountain Standard Time" },
                    { 8, "Lethbridge", "Mountain Standard Time" },
                    { 9, "Red Deer", "Mountain Standard Time" },
                    { 10, "Fort McMurray", "Mountain Standard Time" },
                    { 11, "Regina", "Canada Central Standard Time" },
                    { 12, "Saskatoon", "Canada Central Standard Time" },
                    { 13, "Moose Jaw", "Canada Central Standard Time" },
                    { 14, "Brandon", "Central Standard Time" },
                    { 15, "Winnipeg", "Central Standard Time" },
                    { 16, "Thunder Bay", "Eastern Standard Time" },
                    { 17, "Sault Ste. Marie", "Eastern Standard Time" },
                    { 18, "Sudbury", "Eastern Standard Time" },
                    { 19, "North Bay", "Eastern Standard Time" },
                    { 20, "Barrie", "Eastern Standard Time" },
                    { 21, "Toronto", "Eastern Standard Time" },
                    { 22, "Mississauga", "Eastern Standard Time" },
                    { 23, "Hamilton", "Eastern Standard Time" },
                    { 24, "London", "Eastern Standard Time" },
                    { 25, "Kitchener", "Eastern Standard Time" },
                    { 26, "Windsor", "Eastern Standard Time" },
                    { 27, "St. Catharines", "Eastern Standard Time" },
                    { 28, "Oshawa", "Eastern Standard Time" },
                    { 29, "Kingston", "Eastern Standard Time" },
                    { 30, "Ottawa", "Eastern Standard Time" },
                    { 31, "Gatineau", "Eastern Standard Time" },
                    { 32, "Montreal", "Eastern Standard Time" },
                    { 33, "Quebec City", "Eastern Standard Time" },
                    { 34, "Sherbrooke", "Eastern Standard Time" },
                    { 35, "Trois-Rivières", "Eastern Standard Time" },
                    { 36, "Saguenay", "Eastern Standard Time" },
                    { 37, "Rimouski", "Eastern Standard Time" },
                    { 38, "Edmundston", "Atlantic Standard Time" },
                    { 39, "Fredericton", "Atlantic Standard Time" },
                    { 40, "Moncton", "Atlantic Standard Time" },
                    { 41, "Saint John", "Atlantic Standard Time" },
                    { 42, "Bathurst", "Atlantic Standard Time" },
                    { 43, "Charlottetown", "Atlantic Standard Time" },
                    { 44, "Summerside", "Atlantic Standard Time" },
                    { 45, "Sydney", "Atlantic Standard Time" },
                    { 46, "Truro", "Atlantic Standard Time" },
                    { 47, "New Glasgow", "Atlantic Standard Time" },
                    { 48, "Dartmouth", "Atlantic Standard Time" },
                    { 49, "Halifax", "Atlantic Standard Time" }
                });

            migrationBuilder.InsertData(
                table: "EventCodeDefinitions",
                columns: new[] { "Code", "Description", "LongDescription" },
                values: new object[,]
                {
                    { "A", "Arrived", "Railcar equipment arrives at a city on route" },
                    { "D", "Departed", "Railcar equipment departs from a city on route" },
                    { "W", "Released", "Railcar equipment is released from origin" },
                    { "Z", "Placed", "Railcar equipment is placed at destination" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentEvents_CityId",
                table: "EquipmentEvents",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentEvents_EquipmentId",
                table: "EquipmentEvents",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentEvents_TripId",
                table: "EquipmentEvents",
                column: "TripId");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_DestinationCityId",
                table: "Trips",
                column: "DestinationCityId");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_EquipmentId",
                table: "Trips",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_OriginCityId",
                table: "Trips",
                column: "OriginCityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EquipmentEvents");

            migrationBuilder.DropTable(
                name: "EventCodeDefinitions");

            migrationBuilder.DropTable(
                name: "Trips");

            migrationBuilder.DropTable(
                name: "Cities");
        }
    }
}
