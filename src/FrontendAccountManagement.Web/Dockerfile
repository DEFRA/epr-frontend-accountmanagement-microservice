FROM defradigital/dotnetcore-development:dotnet6.0 AS build-env

# Expose the app on a defined port, configurable via a build argument
ARG PORT=3000
ENV ASPNETCORE_URLS=http://*:${PORT}
EXPOSE ${PORT}

# Copy everything
COPY --chown=dotnet FrontendAccountManagement.Web/*.csproj ./FrontendAccountManagement.Web/
COPY --chown=dotnet FrontendAccountManagement.Core/*.csproj ./FrontendAccountManagement.Core/
COPY --chown=dotnet NuGet.Config ./

# Use Build Argument to insert PAT into Nuget file
ARG PAT=localhost
RUN sed -i "s|</configuration>|<packageSourceCredentials><epr-packaging-common><add key=\"Username\" value=\"PAT\" /><add key=\"ClearTextPassword\" value=\"${PAT}\" /></epr-packaging-common></packageSourceCredentials></configuration>|" NuGet.Config

# Restore as distinct layers
RUN dotnet restore "FrontendAccountManagement.Web/FrontendAccountManagement.Web.csproj" --configfile "NuGet.Config"

# Install NPM
USER root
RUN apk update && apk add --no-cache npm
USER dotnet
ENV NODE_EXTRA_CA_CERTS=/usr/local/share/ca-certificates/internal-ca.crt

# Build and publish a release
COPY --chown=dotnet FrontendAccountManagement.Web/. ./FrontendAccountManagement.Web/.
COPY --chown=dotnet FrontendAccountManagement.Core/. ./FrontendAccountManagement.Core/.
WORKDIR /home/dotnet/FrontendAccountManagement.Web

RUN dotnet publish -c Release -o out

# Build runtime image
FROM defradigital/dotnetcore:dotnet6.0
COPY --from=build-env --chown=dotnet /home/dotnet/FrontendAccountManagement.Web/out .

# Add internationalisation support
USER root
RUN apk add icu-libs icu-data-full libintl tzdata
USER dotnet

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=0

# Expose the app on a defined port, configurable via a build argument
ARG PORT=3000
ENV ASPNETCORE_URLS=http://*:${PORT}
EXPOSE ${PORT}

CMD dotnet FrontendAccountManagement.Web.dll
