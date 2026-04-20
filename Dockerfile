# syntax=docker/dockerfile:1.7

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/Mts2.Web/Mts2.Web.csproj src/Mts2.Web/
RUN dotnet restore src/Mts2.Web/Mts2.Web.csproj

COPY src/Mts2.Web/ src/Mts2.Web/
RUN dotnet publish src/Mts2.Web/Mts2.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Mts2.Web.dll"]
