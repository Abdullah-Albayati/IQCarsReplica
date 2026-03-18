# Use the official .NET 9.0 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["Template/Template.csproj", "Template/"]
RUN dotnet restore "Template/Template.csproj"

# Copy the rest of the source code
COPY . .
WORKDIR "/src/Template"

# Build the application
RUN dotnet build "Template.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "Template.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the official .NET 9.0 runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS production
WORKDIR /app

# Expose ports
EXPOSE 8080
EXPOSE 8081

# Copy the published app from the publish stage
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "Template.dll"]
