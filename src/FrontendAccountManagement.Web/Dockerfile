﻿FROM defradigital/dotnetcore-development:dotnet8.0 AS build-env
USER root

# Expose the app on a defined port, configurable via a build argument
ARG PORT=3000
ENV ASPNETCORE_URLS=http://*:${PORT}
EXPOSE ${PORT}

# Copy everything
COPY FrontendAccountManagement.Web/*.csproj ./FrontendAccountManagement.Web/
COPY FrontendAccountManagement.Core/*.csproj ./FrontendAccountManagement.Core/
COPY NuGet.Config ./

# Use Build Argument to insert PAT into Nuget file
ARG PAT=localhost
RUN sed -i "s|</configuration>|<packageSourceCredentials><epr-packaging-common><add key=\"Username\" value=\"PAT\" /><add key=\"ClearTextPassword\" value=\"${PAT}\" /></epr-packaging-common></packageSourceCredentials></configuration>|" NuGet.Config

# Restore as distinct layers
RUN dotnet restore "FrontendAccountManagement.Web/FrontendAccountManagement.Web.csproj" --configfile "NuGet.Config"

# Install NPM
USER root
RUN apk update && apk add --no-cache npm

ENV NODE_EXTRA_CA_CERTS=/usr/local/share/ca-certificates/internal-ca.crt

# Build and publish a release
COPY FrontendAccountManagement.Web/. ./FrontendAccountManagement.Web/.
COPY FrontendAccountManagement.Core/. ./FrontendAccountManagement.Core/.
WORKDIR /home/dotnet/FrontendAccountManagement.Web

RUN dotnet publish -c Release -o out

# Build runtime image
FROM defradigital/dotnetcore:dotnet8.0

#Temporariky switch to root user to install packages
USER root

# Install tzdata
USER root
RUN apk add --no-cache tzdata

# Add internationalisation support
RUN apk --no-cache add icu-libs icu-data-full libintl

# Switch back to the dotnet user
USER dotnet

COPY --from=build-env /home/dotnet/FrontendAccountManagement.Web/out .

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=0

# Expose the app on a defined port, configurable via a build argument
ARG PORT=3000
ENV ASPNETCORE_URLS=http://*:${PORT}
EXPOSE ${PORT}

CMD dotnet FrontendAccountManagement.Web.dll
