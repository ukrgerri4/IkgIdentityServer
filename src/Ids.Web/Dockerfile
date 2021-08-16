#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/Ids.Web/Ids.Web.csproj", "src/Ids.Web/"]
RUN dotnet restore "src/Ids.Web/Ids.Web.csproj"
COPY src/Ids.Web/ ./Ids.Web
WORKDIR "/src/Ids.Web"
RUN dotnet build "Ids.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Ids.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ids.Web.dll"]