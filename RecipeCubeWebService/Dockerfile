# 請參閱 https://aka.ms/customizecontainer 了解如何自訂您的偵錯容器，以及 Visual Studio 如何使用此 Dockerfile 來組建您的映像，以加快偵錯速度。

# 此階段用於以快速模式從 VS 執行時 (偵錯設定的預設值)
# 使用 .NET 8 基礎映像來運行應用
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 5000


# 此階段是用來組建服務專案
# 使用 .NET SDK 作為基礎映像來構建應用
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
# 設置工作目錄
WORKDIR /src
# 將項目文件複製到容器中
COPY ["RecipeCubeWebService/RecipeCubeWebService.csproj", "RecipeCubeWebService/"]
# 恢復項目的依賴
RUN dotnet restore "./RecipeCubeWebService/RecipeCubeWebService.csproj"
# 複製所有檔案到容器中
COPY . .
# 設置工作目錄到你的 API 應用目錄
WORKDIR "/src/RecipeCubeWebService"
# 構建並發布應用
RUN dotnet build "./RecipeCubeWebService.csproj" -c $BUILD_CONFIGURATION -o /app/build

# 此階段可用來發佈要複製到最終階段的服務專案
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RecipeCubeWebService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# 此階段用於生產環境，或以一般模式從 VS 執行時 (未使用偵錯設定時的預設值)
FROM base AS final
WORKDIR /app
# 從前一階段複製已發布的應用
COPY --from=publish /app/publish .
# 設置容器啟動命令
ENTRYPOINT ["dotnet", "RecipeCubeWebService.dll"]