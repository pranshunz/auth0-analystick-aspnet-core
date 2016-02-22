namespace Analystick.Web.Config
{
    public class Auth0Config
    {
        public string Token { get; set; }

        public string Domain { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string RedirectUri { get; set; }

        public string Connection { get; set; }
    }
}
