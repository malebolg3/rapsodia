FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore Rapsodia.csproj
RUN dotnet restore Rapsodia.Blue/Rapsodia.Blue.csproj
RUN dotnet restore Rapsodia.Red/Rapsodia.Red.csproj
RUN dotnet restore Rapsodia.Silver/Rapsodia.Silver.csproj

FROM build AS build-blue
RUN dotnet publish Rapsodia.Blue/Rapsodia.Blue.csproj -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM build AS build-red
RUN dotnet publish Rapsodia.Red/Rapsodia.Red.csproj -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM build AS build-violet
RUN dotnet publish Rapsodia.csproj -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM build AS build-silver
RUN dotnet publish Rapsodia.Silver/Rapsodia.Silver.csproj -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy AS blue
WORKDIR /app
COPY --from=build-blue /app/publish .
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000
ENTRYPOINT ["dotnet", "Rapsodia.Blue.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy AS red
WORKDIR /app
COPY --from=build-red /app/publish .
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000
ENTRYPOINT ["dotnet", "Rapsodia.Red.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy AS violet
WORKDIR /app
COPY --from=build-violet /app/publish .
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000
ENTRYPOINT ["dotnet", "Rapsodia.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy AS silver
WORKDIR /app
COPY --from=build-silver /app/publish .
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000
ENTRYPOINT ["dotnet", "Rapsodia.Silver.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy-chiseled AS blue-chiseled
WORKDIR /app
COPY --from=build-blue --chown=1654:1654 /app/publish .
USER 1654
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000
ENTRYPOINT ["dotnet", "Rapsodia.Blue.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy-chiseled AS red-chiseled
WORKDIR /app
COPY --from=build-red --chown=1654:1654 /app/publish .
USER 1654
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000
ENTRYPOINT ["dotnet", "Rapsodia.Red.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy-chiseled AS violet-chiseled
WORKDIR /app
COPY --from=build-violet --chown=1654:1654 /app/publish .
USER 1654
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000
ENTRYPOINT ["dotnet", "Rapsodia.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy-chiseled AS silver-chiseled
WORKDIR /app
COPY --from=build-silver --chown=1654:1654 /app/publish .
USER 1654
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000
ENTRYPOINT ["dotnet", "Rapsodia.Silver.dll"]