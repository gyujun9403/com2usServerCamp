FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env

WORKDIR /App
COPY . ./
RUN dotnet restore
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /App
COPY ./entrypoint.sh ./app/
RUN chmod +x ./app/entrypoint.sh
COPY --from=build-env /App/out .
ENTRYPOINT ["./app/entrypoint.sh"]
