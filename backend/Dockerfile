# 使用 .NET 9.0 SDK 建置映像
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# 複製 csproj 並還原依賴項
COPY ["NGO_WebAPI_Backend.csproj", "./"]
RUN dotnet restore "NGO_WebAPI_Backend.csproj"

# 複製所有檔案並建置
COPY . .
RUN dotnet build "NGO_WebAPI_Backend.csproj" -c Release -o /app/build

# 發布應用程式
FROM build AS publish
RUN dotnet publish "NGO_WebAPI_Backend.csproj" -c Release -o /app/publish

# 使用 ASP.NET Core 執行階段映像
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# 設定環境變數
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# 暴露 port
EXPOSE 8080

# 啟動應用程式
ENTRYPOINT ["dotnet", "NGO_WebAPI_Backend.dll"]
