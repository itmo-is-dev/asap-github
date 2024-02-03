FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source
COPY ./src ./src
COPY ./*.sln .
COPY ./*.props ./
COPY ./.editorconfig .

RUN dotnet restore "src/Itmo.Dev.Asap.Github/Itmo.Dev.Asap.Github.csproj"

FROM build AS publish
WORKDIR "/source/src/Itmo.Dev.Asap.Github"
RUN dotnet publish "Itmo.Dev.Asap.Github.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
EXPOSE 8022
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Itmo.Dev.Asap.Github.dll"]
