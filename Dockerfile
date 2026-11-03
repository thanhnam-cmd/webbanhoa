# ---------- Build ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY WebBanHoa/WebBanHoa.csproj WebBanHoa/
RUN dotnet restore WebBanHoa/WebBanHoa.csproj
COPY WebBanHoa/ WebBanHoa/
RUN dotnet publish WebBanHoa/WebBanHoa.csproj -c Release -o /app --no-restore

# ---------- Runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
# Thư viện ICU cho định dạng tiền tệ/ngày giờ tiếng Việt + dữ liệu múi giờ
RUN apt-get update && apt-get install -y --no-install-recommends libicu-dev tzdata && rm -rf /var/lib/apt/lists/*
ENV ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    TZ=Asia/Ho_Chi_Minh
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "WebBanHoa.dll"]
