using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalSMS.DAL.Data.Models
{
    public class SentMessage
    {
        public string MessageID { get; set; }
        public String UserID { get; set; }
        //the phone number of the recipient to whom the SMS is sent.
        public string RecipientPhoneNumber { get; set; }
        public string MessageContent { get; set; }
        public int? TemplateID { get; set; }
        public DateTime SentDate { get; set; }
        //the status of the sent message, such as "Sent", "Failed", "Delivered", or "Pending".
        public string Status { get; set; }

        // Navigation Properties
        public User User { get; set; }
        public MessageTemplate Template { get; set; }
        public ICollection<Log> Logs { get; set; }
    }
}
