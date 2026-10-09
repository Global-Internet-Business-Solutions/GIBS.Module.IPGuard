using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Oqtane.Models;

namespace GIBS.Module.IPGuard.Models
{
    [Table("GIBSIPGuard")]
    public class IPGuard : ModelBase
    {
        [Key]
        public int IPGuardId { get; set; }
        public int ModuleId { get; set; }
        public string Name { get; set; }
    }
}
