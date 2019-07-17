FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["AnimeTube_Linux/AnimeTube_Linux.csproj", "AnimeTube_Linux/"]
RUN dotnet restore "AnimeTube_Linux/AnimeTube_Linux.csproj"
COPY . .
WORKDIR "/src/AnimeTube_Linux"
RUN dotnet build "AnimeTube_Linux.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "AnimeTube_Linux.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "AnimeTube_Linux.dll"]