# Build stage
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

WORKDIR /app

# Copy project file and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy source code
COPY . ./

# Publish application during build stage so publish output is available
# Build & publish to /app/publish in the build stage
RUN dotnet publish -c Release -o /app/publish

# Publish stage
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS publish

WORKDIR /app

# Copy already-published output from build stage
COPY --from=build /app/publish ./

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime

WORKDIR /app

# Copy published application
COPY --from=publish /app/publish ./

# Create volume for database persistence
VOLUME ["/app/data"]

# Expose port
EXPOSE 5000

# Health check
HEALTHCHECK --interval=10s --timeout=5s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:5000/health || exit 1

# Set environment variable for URLs
ENV ASPNETCORE_URLS=http://+:5000

# Run application
ENTRYPOINT ["dotnet", "EStoreManagementAPI.dll"]
