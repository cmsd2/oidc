FROM microsoft/dotnet:1.0.4-runtime

ADD . /app

WORKDIR /app/wwwroot

EXPOSE 5000

ENTRYPOINT dotnet ../OpenIdConnectServer.dll