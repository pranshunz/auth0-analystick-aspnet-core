using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Analystick.Web.Areas.Admin.Models;
using Analystick.Web.Config;
using Auth0.ManagementApi.Clients;
using Auth0.ManagementApi.Models;
using JWT;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.OptionsModel;

namespace Analystick.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UsersController 
        : Controller
    {
        private readonly IOptions<Auth0Config> _auth0Config;
        private readonly IOptions<AnalystickConfig> _analystickConfig;
        private readonly IOptions<SendgridConfig> _sendgridConfig;
        private readonly IUsersClient _usersClient;
        private readonly ITicketsClient _ticketsClient;

        public UsersController(IOptions<Auth0Config> auth0Config,
            IOptions<AnalystickConfig> analystickConfig,
            IOptions<SendgridConfig> sendgridConfig,
            IUsersClient usersClient, 
            ITicketsClient ticketsClient)
        {
            _auth0Config = auth0Config;
            _analystickConfig = analystickConfig;
            this._usersClient = usersClient;
            _ticketsClient = ticketsClient;
            _sendgridConfig = sendgridConfig;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _usersClient.GetAll();
            var model =
                users.Select(
                    u =>
                        new UserModel
                        {
                            UserId = u.UserId,
                            GivenName = u.FirstName,
                            FamilyName = u.LastName,
                            Email = u.Email
                        }).ToList();

            return View(model);
        }

        public ActionResult New()
        {
            return View(Enumerable.Range(0, 5).Select(i => new UserModel()).ToList());
        }


        [HttpPost]
        public async Task<IActionResult> New(IEnumerable<UserModel> users)
        {
            if (users != null)
            {
                foreach (var user in users.Where(u => !String.IsNullOrEmpty(u.Email)))
                {
                    var randomPassword = Guid.NewGuid().ToString();
                    var metadata = new
                    {
                        user.GivenName,
                        user.FamilyName,
                        activation_pending = true
                    };

                    //user.Email, randomPassword, _auth0Config.Value.Connection, false, metadata
                    var profile = await _usersClient.Create(new UserCreateRequest()
                    {
                        FirstName = user.GivenName,
                        LastName = user.FamilyName,
                        Email = user.Email,
                        Password = randomPassword,
                        UserMetadata = new
                        {
                            activation_pending = true
                        },
                        Connection = _auth0Config.Value.Connection
                    });

                    var userToken = JWT.JsonWebToken.Encode(
                        new { id = profile.UserId, email = profile.Email }, _analystickConfig.Value.SigningKey, JwtHashAlgorithm.HS256);

                    var verificationUrl = await _ticketsClient.CreateEmailVerificationTicket(new EmailVerificationTicketRequest()
                    {
                        ResultUrl = Url.Action("Activate", "Account", new { area = "", userToken }, Request.Scheme),
                        UserId = profile.UserId
                    });

                    var body = "Hello {0}, " +
                      "Great that you're using our application. " +
                      "Please click <a href='{1}'>ACTIVATE</a> to activate your account." +
                      "The Analystick team!";

                    var fullName = $"{user.GivenName} {user.FamilyName}".Trim();
                    //var mail = new MailMessage("app@auth0.com", user.Email, "Hello there!",
                    //    String.Format(body, fullName, verificationUrl));
                    //mail.IsBodyHtml = true;

                    //var mailClient = new SmtpClient();
                    //mailClient.Send(mail);
                    var myMessage = new SendGrid.SendGridMessage();
                    myMessage.AddTo(profile.Email);
                    myMessage.From = new MailAddress("digital@ystringtheory.me", "String Digital");
                    myMessage.Subject = "Sending with SendGrid is Fun";
                    myMessage.Html = $"Hello {fullName}, " +
                      "Great that you're using our application. " +
                      $"Please click <a href='{verificationUrl.Value}'>ACTIVATE</a> to activate your account." +
                      "The Analystick team!";

                    var transportWeb = new SendGrid.Web(new NetworkCredential(_sendgridConfig.Value.Username, _sendgridConfig.Value.Password));
                    await transportWeb.DeliverAsync(myMessage);
                }
            }

            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
                await _usersClient.Delete(id);

            return RedirectToAction("Index");
        }
    }
}