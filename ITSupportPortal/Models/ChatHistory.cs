using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITSupportPortal.Models
{
    public class ChatHistory
    {
        [Required]
        public string CaseID { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public string Message { get; set; }

    }
    
}
