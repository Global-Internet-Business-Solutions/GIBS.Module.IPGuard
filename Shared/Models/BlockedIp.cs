using System;
using System.ComponentModel.DataAnnotations;
using Oqtane.Models;

namespace GIBS.Module.IPGuard.Models
{
    public class BlockedIp : ModelBase
    {
        [Key]
        public int BlockedIpId { get; set; }

        [Required]
        [MaxLength(45)]
        public string IpAddress { get; set; }

        [MaxLength(100)]
        public string CountryLookup { get; set; }

        public int ErrorCount { get; set; }

        public DateTime BlockedDate { get; set; }

        [MaxLength(2000)]
        public string Reason { get; set; }
    }
}
