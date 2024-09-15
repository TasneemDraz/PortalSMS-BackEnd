using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalSMS.DAL.Data.Models
{
    public class Log
    {
        public int LogID { get; set; }
        public string MessageID { get; set; }
        public string LogMessage { get; set; }
        public DateTime LogDate { get; set; }
        public string LogLevel { get; set; }

        // Navigation Properties
        public SentMessage SentMessage { get; set; }
    }
}
