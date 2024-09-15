using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalSMS.BL.DTOs
{
    public class GetMessageTemplateDto
    {
        public int TemplateID { get; set; }
        public string TemplateName { get; set; }
        public string TemplateContent { get; set; }
        public string CreatedByUsername { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
    }

}
