# ---- Stage 1: Build the .NET application ----
# Start with the full .NET 8 SDK image (has tools to compile code)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# Set the working directory inside the container
WORKDIR /src

# Copy just the project file first and restore (download) dependencies
# This is done separately so Docker can cache this layer (faster rebuilds)
COPY LeadManagementApp/LeadManagementApp.csproj LeadManagementApp/
RUN dotnet restore LeadManagementApp/LeadManagementApp.csproj

# Copy all the source code and build the app in Release mode
COPY LeadManagementApp/ LeadManagementApp/
RUN dotnet publish LeadManagementApp/LeadManagementApp.csproj -c Release -o /app/publish

# ---- Stage 2: Run the application ----
# Use a smaller runtime-only image (no build tools needed to just run the app)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
# Copy the compiled application from the build stage above
COPY --from=build /app/publish .

# Tell the app to listen on port 5000 on all network interfaces
ENV ASPNETCORE_URLS=http://+:5000
# Let Docker know this container uses port 5000
EXPOSE 5000

# Start the application when the container runs
ENTRYPOINT ["dotnet", "LeadManagementApp.dll"]

# ==========================================================================
# SUMMARY:
# This Dockerfile builds and runs the .NET 8 backend API for the Lead
# Management System. It uses a two-stage build: Stage 1 compiles the C# code
# using the full SDK, and Stage 2 copies just the compiled output into a slim
# runtime image. This keeps the final image small and secure. The API listens
# on port 5000.
# ==========================================================================
