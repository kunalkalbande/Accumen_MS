FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000

# Creates a non-root user with an explicit UID and adds permission to access the /app folder
# For more info, please refer to https://aka.ms/vscode-docker-dotnet-configure-containers
RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["MBH.Clinician/src/MBH.Clinician.Service/MBH.Clinician.Service.csproj", "MBH.Clinician/src/MBH.Clinician.Service/"]

COPY . .
WORKDIR "/src/MBH.Clinician/src/MBH.Clinician.Service"
RUN dotnet restore "MBH.Clinician.Service.csproj" 

RUN dotnet build "MBH.Clinician.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MBH.Clinician.Service.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MBH.Clinician.Service.dll"]
