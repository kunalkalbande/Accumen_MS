FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
EXPOSE  5000:80
WORKDIR /app
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
ENV ASPNETCORE_URLS=http://+:5000;
RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser
COPY ["MBH.Application/src/MBH.Application.Service/MBH.Application.Service.csproj", "MBH.Application/src/MBH.Application.Service/"]

COPY . .
WORKDIR "/src/MBH.Application/src/MBH.Application.Service"
RUN dotnet restore "MBH.Application.Service.csproj" 

RUN dotnet build "MBH.Application.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MBH.Application.Service.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MBH.Application.Service.dll"]
