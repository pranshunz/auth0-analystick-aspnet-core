using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Analystick.Web.Auth0
{
    public class AuthenticationTransaction
    {
        public string Nonce
        {
            get;
        }

        public string State
        {
            get;
        }

        public AuthenticationTransaction(string nonce, string state)
        {
            this.Nonce = nonce;
            this.State = state;
        }
    }
}
