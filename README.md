# Invite Only Sample - Auth0 - ASP.NET Core
This is ASP.NET Core (previously ASP.NET 5) implementation of [invite only sample] from Auth0. Also have a look at [article] (https://auth0.com/docs/invite-only) about the application.

## Getting Started
In order to run this sample, you will require an Auth0 and SendGrid accounts. Have a look at appsettings.json and set all the required configuration values:

```json
  "Auth0": {
    "Token": "[Auth0 API Token will all the required permissions]",
    "Domain": "[Auth0 domain]",
    "ClientId": "[App's ClientId]",
    "ClientSecret": "[App's client secret]",
    "RedirectUri": "[After logging in, the users will be redirected to this uri]",
    "Connection": "[Auth0 user store]"
  },
  "Analystick": {
    "SigningKey": "[A unique string that will be used to sign JWTs]" 
  },
  "SendGrid": {
    "Username": "[SendGrid username]",
    "Password": "[SendGrid password]"
  }
```

It is recommended that you use [Secret Manager] to store the aforementioned application settings.

![Screenshot](/Screenshots/ss1.PNG?raw=true)

## License
This project is licensed under the MIT license. See the [LICENSE](LICENSE) file for more info.

[invite only sample]: <https://github.com/auth0/auth0-invite-only-sample>
[Secret Manager]: <https://docs.asp.net/en/latest/security/app-secrets.html#secret-manager>
