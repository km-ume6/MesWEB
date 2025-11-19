namespace MesWEB.Models
{
    /// <summary>
    /// �Z���Ԃ̑Ή����`����N���X�i�����u�b�N�Ή��A�t�@�C�����x�[�X�j
    /// </summary>
    public class CellMapping
    {
        /// <summary>
        /// ���ږ��i�����r���Ă��邩�j
        /// </summary>
        public string ItemName { get; set; } = string.Empty;

        /// <summary>
        /// �u�b�N1�̔ԍ��i0�n�܂�j- UI�\���p
        /// </summary>
        public int Book1Index { get; set; } = -1;

        /// <summary>
        /// �u�b�N1�̃t�@�C���� - �e���v���[�g�ۑ��p
        /// </summary>
        public string Book1FileName { get; set; } = string.Empty;

        /// <summary>
        /// �V�[�g1�̖��O
        /// </summary>
        public string Sheet1Name { get; set; } = string.Empty;

        /// <summary>
        /// �V�[�g1�̃Z���A�h���X�i��FA1, B5�j�܂��͔͈́i��FA1:A10�j
        /// </summary>
        public string Sheet1Cell { get; set; } = string.Empty;

        /// <summary>
        /// �V�[�g1�̐����^�C�v�iNone, Max, Min, Average�j
        /// </summary>
        public FormulaType Sheet1Formula { get; set; } = FormulaType.None;

        /// <summary>
        /// �u�b�N2�̔ԍ��i0�n�܂�j- UI�\���p
        /// </summary>
        public int Book2Index { get; set; } = -1;

        /// <summary>
        /// �u�b�N2�̃t�@�C���� - �e���v���[�g�ۑ��p
        /// </summary>
        public string Book2FileName { get; set; } = string.Empty;

        /// <summary>
        /// �V�[�g2�̖��O
        /// </summary>
        public string Sheet2Name { get; set; } = string.Empty;

        /// <summary>
        /// �V�[�g2�̃Z���A�h���X�i��FC10, D20�j�܂��͔͈́i��FB1:B10�j
        /// </summary>
        public string Sheet2Cell { get; set; } = string.Empty;

        /// <summary>
        /// �V�[�g2�̐����^�C�v�iNone, Max, Min, Average�j
        /// </summary>
        public FormulaType Sheet2Formula { get; set; } = FormulaType.None;

        /// <summary>
        /// ���l��r���̏����_�ȉ��̌����i�f�t�H���g: 2���j
        /// </summary>
        public int DecimalPlaces { get; set; } = 2;

        /// <summary>
        /// ���l��r���̋��e�덷�i��Βl�A�f�t�H���g: 0 = ���S��v�j
        /// </summary>
        public double Tolerance { get; set; } = 0.0;
    }

    /// <summary>
    /// �����^�C�v�̗񋓌^
    /// </summary>
    public enum FormulaType
    {
        /// <summary>�����Ȃ��i�ʏ�̃Z���l�j</summary>
        None = 0,
        /// <summary>�ő�l</summary>
        Max = 1,
        /// <summary>�ŏ��l</summary>
        Min = 2,
        /// <summary>���ϒl</summary>
        Average = 3
    }

    /// <summary>
    /// �Z����r�̌��ʂ��i�[����N���X�i�����u�b�N�Ή��j
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
