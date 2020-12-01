FROM mcr.microsoft.com/dotnet/sdk:3.1-bionic AS build-env 
COPY . /deploy/
WORKDIR /deploy
RUN dotnet restore  ./deploy/deploy.csproj 
RUN dotnet build  ./deploy/deploy.csproj

FROM mcr.microsoft.com/dotnet/runtime:3.1-bionic
COPY --from=build-env /deploy/deploy/bin/Debug/netcoreapp3.1 ./app

ENTRYPOINT ["/app/deploy"]
