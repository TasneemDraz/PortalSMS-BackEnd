using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalSMS.BL.DTOs.Authentication
{
    public class LoginDTO
    {
        public string UserName { get; init; }
        public string Password { get; init; }
    }
}
