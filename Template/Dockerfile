# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["Template/Template.csproj", "Template/"]
RUN dotnet restore "Template/Template.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/Template"
RUN dotnet build "Template.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "Template.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime (this is the stage Dokploy is looking for)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS production
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Copy published app
COPY --from=publish /app/publish .

# Set environment to Production
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Template.dll"]
