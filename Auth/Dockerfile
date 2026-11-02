FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY RideLink.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish RideLink.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish ./

USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "RideLink.dll"]

FROM build AS migrator
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"
ENTRYPOINT dotnet ef database update --connection "$ConnectionStrings__AuthDb"
