using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalSMS.DAL.Data.Models
{
    public class MessageTemplate
    {
        public int TemplateID { get; set; }
        public string TemplateName { get; set; }
        public string TemplateContent { get; set; }
        //stores the ID of the user who originally created the record
        public String CreatedBy { get; set; }
        // when the record was created
        public DateTime CreatedDate { get; set; }
        //who made the most recent update.
        public String? LastModifiedBy { get; set; }
        //when the last modification occurred
        public DateTime? LastModifiedDate { get; set; }

        // Navigation Properties
        public User Creator { get; set; }
        public User LastModifier { get; set; }
        public ICollection<SentMessage> SentMessages { get; set; }
    }
}
