# OpenIdConnect Authorisation Service

Uses OpenIddict, AspNetCore.Identity.DynamoDB and others to implement
an Oidc authorisation service.

## Features

* Local user registration and login
* TOTP two-factor auth

## Running

1. Install dotnet core sdk.
2. Restore packages
3. Build
4. Create a jwt signing certificate at `src/OpenIdConnectServer/cert.pfx`
5. Create a settings json file in `src/OpenIdConnectServer/appsettings.development.json` with the cert password.
6. Run
