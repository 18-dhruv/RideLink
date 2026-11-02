FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Ride.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish Ride.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish ./

FROM build AS migrator
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"
ENTRYPOINT dotnet ef database update --connection "$ConnectionStrings__AuthDb"

USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "Ride.dll"]