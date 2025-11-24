using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MesWEB.Shared.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "GrowthNotes");

            migrationBuilder.AddColumn<string>(
                name: "BottomInsulationComposition",
                table: "GrowthNotes",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CrucibleCount",
                table: "GrowthNotes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CrucibleName",
                table: "GrowthNotes",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CrystalGrowthDate",
                table: "GrowthNotes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CrystalLot",
                table: "GrowthNotes",
                type: "nvarchar(24)",
                maxLength: 24,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CulletFeed",
                table: "GrowthNotes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CulletLot1",
                table: "GrowthNotes",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CulletLot2",
                table: "GrowthNotes",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CulletLot3",
                table: "GrowthNotes",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeltaT",
                table: "GrowthNotes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "DopantAmount",
                table: "GrowthNotes",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Dopants",
                table: "GrowthNotes",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "FirstRotationalSpeed",
                table: "GrowthNotes",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FurnaceCondition1",
                table: "GrowthNotes",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FurnaceCondition2",
                table: "GrowthNotes",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GravityCenterWC",
                table: "GrowthNotes",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "GrowthStartTime",
                table: "GrowthNotes",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "GrowthVoltage",
                table: "GrowthNotes",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HeadDiameter",
                table: "GrowthNotes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeightPositionWC",
                table: "GrowthNotes",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InsideInsulationComposition",
                table: "GrowthNotes",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "LastPullingSpeed",
                table: "GrowthNotes",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "LastRotationalSpeed",
                table: "GrowthNotes",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LiquidLevel",
                table: "GrowthNotes",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MachineName",
                table: "GrowthNotes",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "NeckingVoltage",
                table: "GrowthNotes",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Operator",
                table: "GrowthNotes",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OutputCurrent",
                table: "GrowthNotes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OutputVoltage",
                table: "GrowthNotes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PelletFeed",
                table: "GrowthNotes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PelletLot1",
                table: "GrowthNotes",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PelletLot2",
                table: "GrowthNotes",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PelletLot3",
                table: "GrowthNotes",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Remarks",
                table: "GrowthNotes",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RingHeightPosition",
                table: "GrowthNotes",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "SeedHeightPosition",
                table: "GrowthNotes",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "ShoulderEndPullingSpeed",
                table: "GrowthNotes",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "ShoulderEndRotationalSpeed",
                table: "GrowthNotes",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "ShoulderEndTime",
                table: "GrowthNotes",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "StartPullingSpeed",
                table: "GrowthNotes",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TailDiameter",
                table: "GrowthNotes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsingDisk",
                table: "GrowthNotes",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ppm",
                table: "GrowthNotes",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BottomInsulationComposition",
                table: "GrowthNotes");

            migrationBuilder.DropColumn(
                name: "CrucibleCount",
                table: "GrowthNotes");

            migrationBuilder.DropColumn(
                name: "CrucibleName",
                table: "GrowthNotes");

            migrationBuilder.DropColumn(
                name: "CrystalGrowthDate",
                table: "GrowthNotes");

            migrationBuilder.DropColumn(
                name: "CrystalLot",
                table: "GrowthNotes");

            migrationBuilder.DropColumn(
                name: "CulletFeed",
                table: "GrowthNotes");

            migrationBuilder.DropColumn(
                name: "CulletLot1",
                table: "GrowthNotes");

            migrationBuilder.DropColumn(
                name: "CulletLot2",
                table: "GrowthNotes");

            migrationBuilder.DropColumn(
                name: "CulletLot3",
                table: "GrowthNotes");

            migrationBuilder.DropColumn(
                name: "DeltaT",
                table: "GrowthNotes");

            migrationBuilder.DropColumn(
                name: "DopantAmount",
                table: "GrowthNotes");

            migrationBuilder.DropColumn(
                name: "Dopants",
                table: "GrowthNotes");

            migrationBuilder.DropColumn(
                name: "FirstRotationalSpeed",
                table: "GrowthNotes");

            migrationBuilder.DropColumn(
                name: "FurnaceCondition1",
                table: "GrowthNotes");

            migrationBuilder.DropColumn(
                name: "FurnaceCondition2",
                table: "GrowthNotes");

            migrationBuilder.DropColumn(
                name: "GravityCenterWC",
                table: "GrowthNotes");

            migrationBuilder.DropColumn(
                name: "GrowthStartTime",
                table: "GrowthNotes");

            migrationBuilder.DropColumn(
                name: "GrowthVoltage",
                table: "GrowthNotes");

            migrationBuilder.DropColumn(
                name: "HeadDiameter",
                table: "GrowthNotes");

            migrationBuilder.DropColumn(
                name: "HeightPositionWC",
                table: "GrowthNotes");

            migrationBuilder.DropColumn(
                name: "InsideInsulationComposition",
                table: "GrowthNotes");

            migrationBuilder.DropColumn(
                name: "LastPullingSpeed",
                table: "GrowthNotes");

            migrationBuilder.DropColumn(
                name: "LastRotationalSpeed",
                table: "GrowthNotes");

            migrationBuilder.DropColumn(
                name: "LiquidLevel",
                table: "GrowthNotes");

            migrationBuilder.DropColumn(
                name: "MachineName",
                table: "GrowthNotes");

            migrationBuilder.DropColumn(
                name: "NeckingVoltage",
                table: "GrowthNotes");

            migrationBuilder.DropColumn(
                name: "Operator",
                table: "GrowthNotes");

            migrationBuilder.DropColumn(
                name: "OutputCurrent",
                table: "GrowthNotes");

            migrationBuilder.DropColumn(
                name: "OutputVoltage",
                table: "GrowthNotes");

            migrationBuilder.DropColumn(
                name: "PelletFeed",
                table: "GrowthNotes");

            migrationBuilder.DropColumn(
                name: "PelletLot1",
                table: "GrowthNotes");

            migrationBuilder.DropColumn(
                name: "PelletLot2",
                table: "GrowthNotes");

            migrationBuilder.DropColumn(
                name: "PelletLot3",
                table: "GrowthNotes");

            migrationBuilder.DropColumn(
                name: "Remarks",
                table: "GrowthNotes");

            migrationBuilder.DropColumn(
                name: "RingHeightPosition",
                table: "GrowthNotes");

            migrationBuilder.DropColumn(
                name: "SeedHeightPosition",
                table: "GrowthNotes");

            migrationBuilder.DropColumn(
                name: "ShoulderEndPullingSpeed",
                table: "GrowthNotes");

            migrationBuilder.DropColumn(
                name: "ShoulderEndRotationalSpeed",
                table: "GrowthNotes");

            migrationBuilder.DropColumn(
                name: "ShoulderEndTime",
                table: "GrowthNotes");

            migrationBuilder.DropColumn(
                name: "StartPullingSpeed",
                table: "GrowthNotes");

            migrationBuilder.DropColumn(
                name: "TailDiameter",
                table: "GrowthNotes");

            migrationBuilder.DropColumn(
                name: "UsingDisk",
                table: "GrowthNotes");

            migrationBuilder.DropColumn(
                name: "ppm",
                table: "GrowthNotes");

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "GrowthNotes",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
