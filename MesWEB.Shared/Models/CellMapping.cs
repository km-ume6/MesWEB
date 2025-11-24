namespace MesWEB.Shared.Models
{
    /// <summary>
    /// セル間の対応を定義するクラス（ファイルブック対応、ファイル名ベース）
    /// </summary>
    public class CellMapping
    {
        /// <summary>
        /// 項目名（何を比較しているか）
        /// </summary>
        public string ItemName { get; set; } = string.Empty;

        /// <summary>
        /// ブック1の番号（0始まり）- UI表示用
        /// </summary>
        public int Book1Index { get; set; } = -1;

        /// <summary>
        /// ブック1のファイル名 - テンプレート保存用
        /// </summary>
        public string Book1FileName { get; set; } = string.Empty;

        /// <summary>
        /// シート1の名前
        /// </summary>
        public string Sheet1Name { get; set; } = string.Empty;

        /// <summary>
        /// シート1のセルアドレス（例：A1, B5）または範囲（例：A1:A10）
        /// </summary>
        public string Sheet1Cell { get; set; } = string.Empty;

        /// <summary>
        /// シート1の数式タイプ（None, Max, Min, Average）
        /// </summary>
        public FormulaType Sheet1Formula { get; set; } = FormulaType.None;

        /// <summary>
        /// ブック2の番号（0始まり）- UI表示用
        /// </summary>
        public int Book2Index { get; set; } = -1;

        /// <summary>
        /// ブック2のファイル名 - テンプレート保存用
        /// </summary>
        public string Book2FileName { get; set; } = string.Empty;

        /// <summary>
        /// シート2の名前
        /// </summary>
        public string Sheet2Name { get; set; } = string.Empty;

        /// <summary>
        /// シート2のセルアドレス（例：C10, D20）または範囲（例：B1:B10）
        /// </summary>
        public string Sheet2Cell { get; set; } = string.Empty;

        /// <summary>
        /// シート2の数式タイプ（None, Max, Min, Average）
        /// </summary>
        public FormulaType Sheet2Formula { get; set; } = FormulaType.None;

        /// <summary>
        /// 数値比較の小数点以下の桁数（デフォルト: 2桁）
        /// </summary>
        public int DecimalPlaces { get; set; } = 2;

        /// <summary>
        /// 数値比較の許容誤差（絶対値、デフォルト: 0 = 完全一致）
        /// </summary>
        public double Tolerance { get; set; } = 0.0;
    }

    /// <summary>
    /// 数式タイプの列挙型
    /// </summary>
    public enum FormulaType
    {
        /// <summary>数式なし（通常のセル値）</summary>
        None = 0,
        /// <summary>最大値</summary>
        Max = 1,
        /// <summary>最小値</summary>
        Min = 2,
        /// <summary>平均値</summary>
        Average = 3
    }

    /// <summary>
    /// セル比較の結果を格納するクラス（ファイルブック対応）
    /// </summary>
    public class CellComparisonResult
    {
        public string ItemName { get; set; } = string.Empty;
        public string Book1Name { get; set; } = string.Empty;
        public string Sheet1Cell { get; set; } = string.Empty;
        public string Sheet1Value { get; set; } = string.Empty;
        public string Sheet1Formula { get; set; } = string.Empty;
        public string Book2Name { get; set; } = string.Empty;
        public string Sheet2Cell { get; set; } = string.Empty;
        public string Sheet2Value { get; set; } = string.Empty;
        public string Sheet2Formula { get; set; } = string.Empty;
        public bool IsMatch { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
