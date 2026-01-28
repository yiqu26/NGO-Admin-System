# 使用官方 .NET 8 SDK 作為 build 環境
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 複製 sln 和 csproj
COPY *.sln .
COPY NGOPlatformWeb/*.csproj ./NGOPlatformWeb/

# 還原相依
RUN dotnet restore

# 複製整個專案並建置
COPY . .
WORKDIR /src/NGOPlatformWeb
RUN dotnet publish -c Release -o /app/out

# 使用 .NET 8 Runtime 執行
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "NGOPlatformWeb.dll"]
