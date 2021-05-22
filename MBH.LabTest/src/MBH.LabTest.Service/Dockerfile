FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
EXPOSE  5000:80
EXPOSE  5001:443
WORKDIR /app
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
ENV ASPNETCORE_URLS=http://localhost:5000;https://localhost:5001;
ENV ASPNETCORE_HTTPS_PORT=5001;

COPY ["MBH.LabTest/src/MBH.LabTest.Service/MBH.LabTest.Service.csproj", "MBH.LabTest/src/MBH.LabTest.Service/"]

COPY . .
WORKDIR "/src/MBH.LabTest/src/MBH.LabTest.Service"
RUN dotnet restore "MBH.LabTest.Service.csproj" 

RUN dotnet build "MBH.LabTest.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MBH.LabTest.Service.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MBH.LabTest.Service.dll"]
