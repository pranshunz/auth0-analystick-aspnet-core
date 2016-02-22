using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Analystick.Web.Auth0;
using Analystick.Web.Config;
using Analystick.Web.Models;
using Auth0.Core;
using Auth0.ManagementApi.Clients;
using Auth0.ManagementApi.Models;
using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.AspNet.Authentication.OpenIdConnect;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.OptionsModel;

namespace Analystick.Web.Controllers
{
    public class AccountController
        : Controller
    {
        private readonly IOptions<AnalystickConfig> _analystickConfig;
        private readonly IOptions<Auth0Config> _auth0Config;
        private readonly IUsersClient _usersClient;
        private const string NonceProperty = "N";
        private static readonly RandomNumberGenerator CryptoRandom = RandomNumberGenerator.Create();


        public AccountController(IOptions<AnalystickConfig> analystickConfig,
            IOptions<Auth0Config> auth0Config, IUsersClient usersClient)
        {
            _analystickConfig = analystickConfig;
            _auth0Config = auth0Config;
            _usersClient = usersClient;
        }


        public IActionResult Login(string returnUrl)
        {
            returnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : "/";

            var transaction = HttpContext.PrepareAuthentication(_auth0Config.Value.RedirectUri, returnUrl);

            // Return nonce to the Lock.
            return this.View(new LoginModel { ReturnUrl = returnUrl, Nonce = transaction.Nonce, State = transaction.State });
        }


        [HttpPost]
        public ActionResult Logout(string returnUrl)
        {
            var baseUrl = Microsoft.AspNet.Http.Extensions.UriHelper.Encode(HttpContext.Request.Scheme, HttpContext.Request.Host, HttpContext.Request.PathBase, HttpContext.Request.Path, HttpContext.Request.QueryString);
            var absoluteReturnUrl = string.IsNullOrEmpty(returnUrl) ?
                Url.Action("Index", "Home", new { }, Request.Scheme) :
                Url.IsLocalUrl(returnUrl) ?
                    new Uri(new Uri(baseUrl), returnUrl).AbsoluteUri : returnUrl;

            if (HttpContext.User.Identity.IsAuthenticated)
            {
                HttpContext.Authentication.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties() { RedirectUri = absoluteReturnUrl });
                HttpContext.Authentication.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }

            return Redirect("/");
        }


        public async Task<ViewResult> Activate(string userToken)
        {
            dynamic metadata = JWT.JsonWebToken.DecodeToObject(userToken, _analystickConfig.Value.SigningKey);
            
            var user = await GetUserProfile(metadata["id"]);
            if (user.UserId != null)
                return View(new UserActivationModel { Email = user.Email, UserToken = userToken });

            return View("ActivationError",
                new UserActivationErrorModel("Error activating user, could not find an exact match for this email address."));
        }


        [HttpPost]
        public async Task<IActionResult> Activate(UserActivationModel model)
        {
            dynamic metadata = JWT.JsonWebToken.DecodeToObject(model.UserToken, _analystickConfig.Value.SigningKey);
            if (metadata == null)
            {
                return View("ActivationError",
                    new UserActivationErrorModel("Unable to find the token."));
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await GetUserProfile(metadata["id"]);
            if (user.UserId != null)
            {
                if (user.UserMetadata != null && !((bool)user.UserMetadata["activation_pending"]))
                    return View("ActivationError", new UserActivationErrorModel("Error activating user, the user is already active."));


                _usersClient.Update(user.UserId, new UserUpdateRequest()
                {
                    Password = model.Password,
                    UserMetadata = new
                    {
                        activation_pending = false
                    }
                });

                return View("Activated");
            }

            return View("ActivationError",
                new UserActivationErrorModel("Error activating user, could not find an exact match for this email address."));
        }


        private async Task<User> GetUserProfile(string id)
        {
            return await _usersClient.Get(id);
        }
    }
}
