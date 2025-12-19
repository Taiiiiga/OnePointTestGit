# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copie du csproj et restore (ajustez le chemin si nécessaire)
COPY ["TestOnePoint/TestOnePoint.csproj", "TestOnePoint/"]
RUN dotnet restore "TestOnePoint/TestOnePoint.csproj"

# Copie du reste et publication
COPY . .
WORKDIR "/src/TestOnePoint"
RUN dotnet publish "TestOnePoint.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:80
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TestOnePoint.dll"]