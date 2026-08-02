FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore

RUN dotnet publish -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:9.0

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

# Fix Render Linux inotify/file watcher limit
ENV DOTNET_USE_POLLING_FILE_WATCHER=1

ENTRYPOINT ["dotnet", "ExpenseManagement.dll"]