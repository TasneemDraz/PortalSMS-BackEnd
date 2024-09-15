using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalSMS.BL.DTOs
{
    public class MessageTemplateDto
    {
        public int TemplateID { get; set; }
        public string TemplateName { get; set; }
        public string TemplateContent { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string LastModifiedBy { get; set; }
        public DateTime LastModifiedDate { get; set; }
    }
}
