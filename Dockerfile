FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8000
EXPOSE 8001

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Astra/Astra.csproj", "Astra/"] 
RUN dotnet restore "./Astra/Astra.csproj"
COPY . .
WORKDIR "/src/Astra"
RUN dotnet build "./Astra.csproj" -c $BUILD_CONFIGURATION -o /app/build

# The stage is used to publish the service projectto be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Astra.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

#This stage is used in production or when running from VS in regular mode
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT [ "dotnet", "Astra.dll" ]