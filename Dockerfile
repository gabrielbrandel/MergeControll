FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 7171

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MergeControll.csproj", "./"]
RUN dotnet restore "./MergeControll.csproj"

COPY . .
WORKDIR "/src"
RUN dotnet build "MergeControll.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MergeControll.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MergeControll.dll"]
