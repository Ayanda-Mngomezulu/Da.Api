# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Daf.Api/Daf.Api.csproj", "Daf.Api/"]
RUN dotnet restore "Daf.Api/Daf.Api.csproj"
COPY . .
WORKDIR /src/Daf.Api
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENTRYPOINT ["dotnet", "Daf.Api.dll"]
