# Build Frontend
FROM node:24 AS frontend-build

WORKDIR /app/frontend

COPY frontend/package*.json ./

RUN npm ci

COPY frontend/ ./

RUN npm run build

# Build Backend
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build

WORKDIR /app

# Copy csproj files and restore first (for caching)
COPY api/CloudBoard.Api/*.csproj ./api/CloudBoard.Api/
RUN dotnet restore ./api/CloudBoard.Api/CloudBoard.Api.csproj

# Copy the rest of the backend
COPY api/ ./api/

# Copy built frontend files into wwwroot of backend
COPY --from=frontend-build /app/frontend/dist ./api/CloudBoard.Api/wwwroot

# Build backend
RUN dotnet publish ./api/CloudBoard.Api/CloudBoard.Api.csproj -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

# Copy published backend (with frontend in wwwroot)
COPY --from=backend-build /app/publish ./

# Expose port 80 (Render maps automatically)
EXPOSE 80

# Entry point
ENTRYPOINT ["dotnet", "CloudBoard.Api.dll"]
