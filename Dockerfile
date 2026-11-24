FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY MiniShop.sln ./
COPY src/MiniShop.Web/MiniShop.Web.csproj src/MiniShop.Web/
COPY src/MiniShop.Application/MiniShop.Application.csproj src/MiniShop.Application/
COPY src/MiniShop.Domain/MiniShop.Domain.csproj src/MiniShop.Domain/
COPY src/MiniShop.Infrastructure/MiniShop.Infrastructure.csproj src/MiniShop.Infrastructure/
RUN dotnet restore src/MiniShop.Web/MiniShop.Web.csproj
COPY src/ src/
RUN dotnet publish src/MiniShop.Web/MiniShop.Web.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MiniShop.Web.dll"]
