FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY LeadManagementApp/LeadManagementApp.csproj LeadManagementApp/
RUN dotnet restore LeadManagementApp/LeadManagementApp.csproj

COPY LeadManagementApp/ LeadManagementApp/
RUN dotnet publish LeadManagementApp/LeadManagementApp.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "LeadManagementApp.dll"]
