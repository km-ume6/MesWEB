using System.ComponentModel.DataAnnotations;

namespace MesWEB.Shared.Data
{
    public class PageAccessCounter
    {
        [Key]
        public string PageName { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
