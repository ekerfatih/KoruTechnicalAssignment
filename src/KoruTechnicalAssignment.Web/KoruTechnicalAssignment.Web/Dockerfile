FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/KoruTechnicalAssignment.Web/KoruTechnicalAssignment.Web/KoruTechnicalAssignment.Web.csproj", "src/KoruTechnicalAssignment.Web/KoruTechnicalAssignment.Web/"]
COPY ["src/KoruTechnicalAssignment.Web/KoruTechnicalAssignment.Web.Client/KoruTechnicalAssignment.Web.Client.csproj", "src/KoruTechnicalAssignment.Web/KoruTechnicalAssignment.Web.Client/"]
COPY ["src/KoruTechnicalAssignment.Infrastructure/KoruTechnicalAssignment.Infrastructure.csproj", "src/KoruTechnicalAssignment.Infrastructure/"]
COPY ["src/KoruTechnicalAssignment.Application/KoruTechnicalAssignment.Application.csproj", "src/KoruTechnicalAssignment.Application/"]
COPY ["src/KoruTechnicalAssignment.Domain/KoruTechnicalAssignment.Domain.csproj", "src/KoruTechnicalAssignment.Domain/"]
RUN dotnet restore "src/KoruTechnicalAssignment.Web/KoruTechnicalAssignment.Web/KoruTechnicalAssignment.Web.csproj"
COPY . .
WORKDIR "/src/src/KoruTechnicalAssignment.Web/KoruTechnicalAssignment.Web"
RUN dotnet publish KoruTechnicalAssignment.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "KoruTechnicalAssignment.Web.dll"]
