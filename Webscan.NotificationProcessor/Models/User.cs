using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Webscan.NotificationProcessor.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Username { get; set; }
        public string email { get; set; }
        public ICollection<StatusCheck> StatusChecks { get; } = new List<StatusCheck>(); 
    }
}
