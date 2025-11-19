using System.ComponentModel.DataAnnotations;

namespace MesWEB.Data
{
    /// <summary>
    /// �Z���Ή��\�̃e���v���[�g��ۑ�����G���e�B�e�B
    /// </summary>
    public class CellMappingTemplate
    {
        [Key]
        public int TemplateId { get; set; }

        /// <summary>
        /// �e���v���[�g��
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string TemplateName { get; set; } = string.Empty;

        /// <summary>
        /// ����
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// �쐬����
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// �ŏI�X�V����
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// ���̃e���v���[�g�Ɋ܂܂��}�b�s���O
        /// </summary>
        public List<CellMappingItem> MappingItems { get; set; } = new();
    }

    /// <summary>
    /// �Z���Ή��\�̌ʍ���
    /// </summary>
    public class CellMappingItem
    {
        [Key]
        public int MappingItemId { get; set; }

        /// <summary>
        /// ��������e���v���[�gID
        /// </summary>
        public int TemplateId { get; set; }

        /// <summary>
        /// ���я�
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// ���ږ�
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string ItemName { get; set; } = string.Empty;

        /// <summary>
        /// �V�[�g1�̖��O
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Sheet1Name { get; set; } = "Sheet1";

        /// <summary>
        /// �V�[�g1�̃Z���A�h���X
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Sheet1Cell { get; set; } = string.Empty;

        /// <summary>
        /// �V�[�g1�̐����^�C�v�i0=None, 1=Max, 2=Min, 3=Average�j
        /// </summary>
        public int Sheet1Formula { get; set; } = 0;

        /// <summary>
        /// �V�[�g2�̖��O
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Sheet2Name { get; set; } = "Sheet2";

        /// <summary>
        /// �V�[�g2�̃Z���A�h���X
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Sheet2Cell { get; set; } = string.Empty;

        /// <summary>
        /// �V�[�g2�̐����^�C�v�i0=None, 1=Max, 2=Min, 3=Average�j
        /// </summary>
        public int Sheet2Formula { get; set; } = 0;

        /// <summary>
        /// ���l��r���̏����_�ȉ��̌����i�f�t�H���g: 2���j
        /// </summary>
        public int DecimalPlaces { get; set; } = 2;

        /// <summary>
        /// ���l��r���̋��e�덷�i��Βl�A�f�t�H���g: 0 = ���S��v�j
        /// </summary>
        public double Tolerance { get; set; } = 0.0;

        /// <summary>
        /// �i�r�Q�[�V�����v���p�e�B
        /// </summary>
        public CellMappingTemplate? Template { get; set; }
    }
}
