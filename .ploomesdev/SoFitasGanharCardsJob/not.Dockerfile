FROM mcr.microsoft.com/dotnet/sdk:7.0 as build-stage
WORKDIR /src
ARG MSBUILD_CONFIGURATION
<@ ARGS @>
ENV ENV_MSBUILD_CONFIGURATION=$MSBUILD_CONFIGURATION
COPY . .
RUN dotnet build
RUN dotnet publish -c ${ENV_MSBUILD_CONFIGURATION} -o /publish

FROM mcr.microsoft.com/dotnet/runtime:7.0 as serve-stage
WORKDIR /app
ARG BUILD_TIMESTAMP
ARG GIT_COMMIT
ENV BUILD_TIMESTAMP=$BUILD_TIMESTAMP
ENV GIT_COMMIT=$GIT_COMMIT
ENV TZ=America/Sao_Paulo
COPY startup.sh .
COPY --from=build-stage /publish/ .
#begin-links:serve
#end-links:serve
RUN apt-get update && apt-get install -y dos2unix && dos2unix startup.sh
ENTRYPOINT bash startup.sh && dotnet "SoFitasGanharCardsJob.dll"
