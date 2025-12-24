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
###                                               FINAL RUNTIME IMAGE                                  ####
###                                                                                                    ####
###  Layer Optimization Strategy:                                                                     ####
###  - Layers ordered from most stable (framework) to least stable (app code)                         ####
###  - This reduces update downloads from ~18 MB to ~200 KB (99% reduction)                           ####
###  - See docs/DOCKER-OPTIMIZATION.md for detailed explanation                                       ####
###                                                                                                    ####
###  Layer Breakdown:                                                                                 ####
###    1-3:  Config & static assets       (~75 KB)   - Rarely change                                  ####
###    4-6:  .NET runtime & Blazor        (~10 MB)   - Stable across app updates                      ####
###    7-8:  Microsoft/System frameworks  (~6 MB)    - Stable                                         ####
###    9-10: Third-party dependencies     (~780 KB)  - Occasionally change                            ####
###   11-13: Application code             (~200 KB)  - Changes with every update ⚡                    ####
###                                                                                                    ####
###########################################################################################################
###########################################################################################################

#
#  Final image (optimized layer caching)
#
FROM nginx:alpine AS final

# Install findutils for better file operations
RUN apk add --no-cache findutils

# Layer 1: nginx config (rarely changes)
COPY nginx.conf /etc/nginx/nginx.conf

# Layer 2: Static assets (icons, manifest - rarely change)
COPY --from=publish /pub/wwwroot/favicon.svg \
                    /pub/wwwroot/icon.svg \
                    /pub/wwwroot/icon-192.png \
                    /pub/wwwroot/icon-512.png \
                    /pub/wwwroot/manifest.json \
                    /usr/share/nginx/html/

# Layer 3: CSS and MudBlazor content (rarely changes)
COPY --from=publish /pub/wwwroot/css /usr/share/nginx/html/css
COPY --from=publish /pub/wwwroot/_content /usr/share/nginx/html/_content

# Layer 4: .NET Runtime files (rarely change - ~2MB, stable across app updates)
COPY --from=publish /pub/wwwroot/_framework/dotnet.* /usr/share/nginx/html/_framework/
COPY --from=publish /pub/wwwroot/_framework/blazor.* /usr/share/nginx/html/_framework/
COPY --from=publish /pub/wwwroot/_framework/icudt* /usr/share/nginx/html/_framework/

# Layer 5: Microsoft framework WASMs (stable - ~1MB)
COPY --from=publish /pub/wwwroot/_framework/Microsoft.*.wasm* /usr/share/nginx/html/_framework/
COPY --from=publish /pub/wwwroot/_framework/System.*.wasm* /usr/share/nginx/html/_framework/

# Layer 6: Third-party dependencies (occasionally change - ~500KB)
COPY --from=publish /pub/wwwroot/_framework/Blazored.*.wasm* /usr/share/nginx/html/_framework/
COPY --from=publish /pub/wwwroot/_framework/MudBlazor.*.wasm* /usr/share/nginx/html/_framework/

# Layer 7: Application WASMs (frequently change - ~50KB) - THIS IS THE LAYER THAT CHANGES MOST
COPY --from=publish /pub/wwwroot/_framework/Pianoteq.*.wasm* /usr/share/nginx/html/_framework/

# Layer 8: Service worker and index (change with every app update)
COPY --from=publish /pub/wwwroot/service-worker*.js* /usr/share/nginx/html/
COPY --from=publish /pub/wwwroot/index.html* /usr/share/nginx/html/

EXPOSE 80
