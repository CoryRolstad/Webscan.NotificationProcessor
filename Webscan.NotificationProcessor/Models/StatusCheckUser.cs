using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webscan.NotificationProcessor.Models
{
    public class StatusCheckUser
    {
        public int StatusCheckId { get; set; }
        public int UserId { get; set; }
        public bool Enabled { get; set; }
    }
}
