FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore in its own layer so it's cached until the csproj changes.
COPY OnlineShop.Api.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish OnlineShop.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

# Run as the non-root user built into the ASP.NET images.
USER $APP_UID

ENTRYPOINT ["dotnet", "OnlineShop.Api.dll"]
