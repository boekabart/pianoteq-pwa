# REFS

# Optional support for using a private 'proxy' registry
ARG REGISTRY_PREFIX=
ARG MICROSOFT_REGISTRY=${REGISTRY_PREFIX}mcr.microsoft.com
FROM --platform=$BUILDPLATFORM ${MICROSOFT_REGISTRY}/dotnet/sdk:10.0 AS sdk10

#
# RESTORE
#
FROM sdk10 AS restore
WORKDIR /code

COPY . .
ENV CI=true
RUN dotnet restore

#
# BUILD
#
FROM restore AS build
ARG VERSION=0.0.0-local
RUN dotnet build -p:VERSION=${VERSION} --no-restore -c Release

#
# TEST
#
FROM build AS test
ARG DOCKER_HOST=
ENV DOCKER_HOST=${DOCKER_HOST}

RUN dotnet test --no-build -c Release

###########################################################################################################
###########################################################################################################
###                                                                                                    ####
###                                               PUBLISH RELEASE BUILDS                               ####
###                                                                                                    ####
###########################################################################################################
###########################################################################################################

FROM restore AS publish
ARG VERSION=0.0.0-local

RUN dotnet publish src/Pianoteq.Pwa \
 -c Release \
 -p:VERSION=${VERSION} \
 -o /pub

###########################################################################################################
###########################################################################################################
###                                                                                                    ####
###                                               FINAL RUNTIME IMAGES                                 ####
###                                                                                                    ####
###########################################################################################################
###########################################################################################################

#
#  Final image
#
FROM nginx:alpine AS final

COPY nginx.conf /etc/nginx/nginx.conf
COPY --from=publish /pub/wwwroot /usr/share/nginx/html

EXPOSE 80
