using System.ComponentModel.DataAnnotations;

namespace MesWEB.Data
{
    /// <summary>
    /// テンプレートをグルーピングするラベル(カテゴリ)
    /// </summary>
    public class CellMappingLabel
    {
        [Key]
        public int LabelId { get; set; }

        [Required]
        [MaxLength(64)]
        public string LabelName { get; set; } = string.Empty;

        /// <summary>
        /// 表示順
        /// </summary>
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// このラベルに属するテンプレート一覧
        /// </summary>
        public List<CellMappingTemplate> Templates { get; set; } = new();
    }

    /// <summary>
    /// セル比較用のテンプレート定義エンティティ
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
        /// 属するラベルID (ツリー上位) - null 可
        /// </summary>
        public int? LabelId { get; set; }

        /// <summary>
        /// ラベルナビゲーション
        /// </summary>
        public CellMappingLabel? Label { get; set; }

        /// <summary>
        /// このテンプレートに属するマッピング
        /// </summary>
        public List<CellMappingItem> MappingItems { get; set; } = new();
    }

    /// <summary>
    /// セル比較用の項目定義
    /// </summary>
    public class CellMappingItem
    {
        [Key]
        public int MappingItemId { get; set; }

        /// <summary>
        /// 親となるテンプレートID
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
        /// シート1の名称
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
        /// シート1の集計タイプ(0=None, 1=Max, 2=Min, 3=Average)
        /// </summary>
        public int Sheet1Formula { get; set; } = 0;

        /// <summary>
        /// シート2の名称
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
        /// シート2の集計タイプ(0=None, 1=Max, 2=Min, 3=Average)
        /// </summary>
        public int Sheet2Formula { get; set; } = 0;

        /// <summary>
        /// 数値比較の小数桁数(デフォルト: 2桁)
        /// </summary>
        public int DecimalPlaces { get; set; } = 2;

        /// <summary>
        /// 数値比較の許容誤差(単位値、デフォルト: 0 = 完全一致)
        /// </summary>
        public double Tolerance { get; set; } = 0.0;

        /// <summary>
        /// ナビゲーションテンプレート
        /// </summary>
        public CellMappingTemplate? Template { get; set; }
    }
}
