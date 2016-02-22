using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Analystick.Web.Models
{
    public class LoginModel
    {
        public string ReturnUrl { get; set; }

        public string State { get; set; }

        public string Nonce { get; set; }
    }
}
