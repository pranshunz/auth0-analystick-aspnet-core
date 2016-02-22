using Analystick.Web.Auth0;
using Analystick.Web.Config;
using Auth0.Core.Http;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Clients;
using Microsoft.AspNet.Authentication;
using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Analystick.Web
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
                builder.AddApplicationInsightsSettings(developerMode: true);
                builder.AddUserSecrets();
            }
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddApplicationInsightsTelemetry(Configuration);

            services.Configure<SharedAuthenticationOptions>(options =>
            {
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });


            services.AddOptions();
            services.Configure<Auth0Config>(Configuration.GetSection("Auth0"));
            services.Configure<AnalystickConfig>(Configuration.GetSection("Analystick"));
            services.Configure<SendgridConfig>(Configuration.GetSection("SendGrid"));

            var auth0Config = Configuration.Get<Auth0Config>("Auth0");
            services.AddInstance(new ApiConnection(auth0Config.Token, $"https://{auth0Config.Domain}/api/v2", DiagnosticsHeader.Default));
            services.AddTransient<IApiConnection>(provider => provider.GetService<ApiConnection>());
            services.AddTransient<IUsersClient, UsersClient>();
            services.AddTransient<ITicketsClient, TicketsClient>();

            services.UseAuth0(auth0Config.Domain, auth0Config.ClientId, auth0Config.ClientSecret, auth0Config.RedirectUri);

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseApplicationInsightsRequestTelemetry();

            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
  
            app.UseApplicationInsightsExceptionTelemetry();

            app.UseStaticFiles();

            app.UseCookieAuthentication(options =>
            {
                options.AutomaticChallenge = true;
                options.AutomaticAuthenticate = true;
                options.LoginPath = "/Account/Login";
                options.AuthenticationScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });

            app.UseAuth0();

            app.UseMvc(routes =>
            {
                // add the new route here.
                routes.MapRoute(name: "areaRoute",
                    template: "{area:exists}/{controller}/{action}",
                    defaults: new { controller = "Home", action = "Index" });

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        // Entry point for the application.
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
