using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalSMS.BL.DTOs
{
    public class SendMessageDto
    {
        public int TemplateId { get; set; }
        public string Recipients { get; set; }
        public string MessageContent { get; set; }
        public string UserId { get; set; }
    }
}
