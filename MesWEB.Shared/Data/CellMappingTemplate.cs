using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MesWEB.Shared.Data
{
    /// <summary>
    /// セル対応表のテンプレートを保存するエンティティ
    /// </summary>
    public class CellMappingTemplate
    {
        [Key]
        public int TemplateId { get; set; }

        /// <summary>
        /// テンプレート名
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string TemplateName { get; set; } = string.Empty;

        /// <summary>
        /// 説明
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// 作成日時
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 最終更新日時
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// このテンプレートに含まれるマッピング
        /// </summary>
        public List<CellMappingItem> MappingItems { get; set; } = new();
    }

    /// <summary>
    /// セル対応表の個別項目
    /// </summary>
    public class CellMappingItem
    {
        [Key]
        public int MappingItemId { get; set; }

        /// <summary>
        /// 所属するテンプレートID
        /// </summary>
        public int TemplateId { get; set; }

        /// <summary>
        /// 並び順
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 項目名
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string ItemName { get; set; } = string.Empty;

        /// <summary>
        /// シート1の名前
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Sheet1Name { get; set; } = "Sheet1";

        /// <summary>
        /// シート1のセルアドレス
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Sheet1Cell { get; set; } = string.Empty;

        /// <summary>
        /// シート1の数式タイプ（0=None, 1=Max, 2=Min, 3=Average）
        /// </summary>
        public int Sheet1Formula { get; set; } = 0;

        /// <summary>
        /// シート2の名前
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Sheet2Name { get; set; } = "Sheet2";

        /// <summary>
        /// シート2のセルアドレス
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Sheet2Cell { get; set; } = string.Empty;

        /// <summary>
        /// シート2の数式タイプ（0=None, 1=Max, 2=Min, 3=Average）
        /// </summary>
        public int Sheet2Formula { get; set; } = 0;

        /// <summary>
        /// 数値比較の小数点以下の桁数（デフォルト: 2桁）
        /// </summary>
        public int DecimalPlaces { get; set; } = 2;

        /// <summary>
        /// 数値比較の許容誤差（絶対値、デフォルト: 0 = 完全一致）
        /// </summary>
        public double Tolerance { get; set; } = 0.0;

        /// <summary>
        /// ナビゲーションプロパティ
        /// </summary>
        public CellMappingTemplate? Template { get; set; }
    }
}
