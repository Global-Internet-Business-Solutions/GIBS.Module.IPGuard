using System;
using System.ComponentModel.DataAnnotations;
using Oqtane.Models;

namespace GIBS.Module.IPGuard.Models
{
    public class ErrorAttempt : ModelBase
    {
        [Key]
        public int ErrorAttemptId { get; set; }

        [Required]
        [MaxLength(45)]
        public string IpAddress { get; set; }

        [MaxLength(2048)]
        public string RequestedUrl { get; set; }

        public DateTime AttemptDate { get; set; }
    }
}
