using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MesWEB.Data
{
    public class GrowthNoteItem
    {
        [Key]
        [Column("Id")]
        public int CGNoteId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CrystalGrowthDate { get; set; } = null;
        [StringLength(8)] public string? Operator { get; set; } = null;
        [StringLength(8)] public string? MachineName { get; set; } = null;
        [StringLength(8)] public string? Dopants { get; set; } = null;
        public float? DopantAmount { get; set; } = null;
        public int? ppm { get; set; } = null;
        [StringLength(24)] public string? CrystalLot { get; set; } = null;
        public int? PelletFeed { get; set; } = null;
        [StringLength(8)] public string? PelletLot1 { get; set; } = null;
        [StringLength(8)] public string? PelletLot2 { get; set; } = null;
        [StringLength(8)] public string? PelletLot3 { get; set; } = null;
        public int? CulletFeed { get; set; } = null;
        [StringLength(16)] public string? CulletLot1 { get; set; } = null;
        [StringLength(16)] public string? CulletLot2 { get; set; } = null;
        [StringLength(16)] public string? CulletLot3 { get; set; } = null;
        [StringLength(8)] public string? CrucibleName { get; set; } = null;
        public int? CrucibleCount { get; set; } = null;
        public int? DeltaT { get; set; } = null;
        [StringLength(8)] public string? InsideInsulationComposition { get; set; } = null;
        [StringLength(8)] public string? BottomInsulationComposition { get; set; } = null;
        [StringLength(16)] public string? FurnaceCondition1 { get; set; } = null;
        [StringLength(16)] public string? FurnaceCondition2 { get; set; } = null;
        [StringLength(16)] public string? RingHeightPosition { get; set; } = null;
        [StringLength(16)] public string? UsingDisk { get; set; } = null;
        public TimeSpan? GrowthStartTime { get; set; } = null;
        public TimeSpan? ShoulderEndTime { get; set; } = null;
        public float? NeckingVoltage { get; set; } = null;
        public float? GrowthVoltage { get; set; } = null;
        public float? SeedHeightPosition { get; set; } = null;
        public float? StartPullingSpeed { get; set; } = null;
        public float? ShoulderEndPullingSpeed { get; set; } = null;
        public float? LastPullingSpeed { get; set; } = null;
        public float? FirstRotationalSpeed { get; set; } = null;
        public float? ShoulderEndRotationalSpeed { get; set; } = null;
        public float? LastRotationalSpeed { get; set; } = null;
        [StringLength(16)] public string? GravityCenterWC { get; set; } = null;
        [StringLength(16)] public string? HeightPositionWC { get; set; } = null;
        public int? HeadDiameter { get; set; } = null;
        public int? TailDiameter { get; set; } = null;
        public int? OutputVoltage { get; set; } = null;
        public int? OutputCurrent { get; set; } = null;
        [StringLength(16)] public string? LiquidLevel { get; set; } = null;
        [StringLength(128)] public string? Remarks { get; set; } = null;
    }
}