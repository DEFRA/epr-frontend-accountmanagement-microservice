FROM defradigital/dotnetcore-development:dotnet10.0 AS build-env
USER root
WORKDIR /home/dotnet

# Keep restore inputs in an earlier layer so source changes do not invalidate restore.
COPY FrontendAccountManagement.Web/*.csproj ./FrontendAccountManagement.Web/
COPY FrontendAccountManagement.Core/*.csproj ./FrontendAccountManagement.Core/
COPY Directory.Build.props SonarAnalyzer.ruleset ./
COPY NuGet.Config ./

# Use a build argument to insert the package feed token into NuGet configuration.
ARG PAT=localhost
RUN sed -i "s|</configuration>|<packageSourceCredentials><epr-packaging-common><add key=\"Username\" value=\"PAT\" /><add key=\"ClearTextPassword\" value=\"${PAT}\" /></epr-packaging-common></packageSourceCredentials></configuration>|" NuGet.Config
RUN dotnet restore FrontendAccountManagement.Web/FrontendAccountManagement.Web.csproj --configfile NuGet.Config

RUN apk add --no-cache npm
ENV NODE_EXTRA_CA_CERTS=/usr/local/share/ca-certificates/internal-ca.crt

COPY FrontendAccountManagement.Web/. ./FrontendAccountManagement.Web/
COPY FrontendAccountManagement.Core/. ./FrontendAccountManagement.Core/
WORKDIR /home/dotnet/FrontendAccountManagement.Web
RUN dotnet publish -c Release -o out --no-restore

FROM defradigital/dotnetcore:dotnet10.0

# Install only the runtime packages required by the application.
USER root
RUN apk add --no-cache tzdata icu-libs icu-data-full libintl
USER dotnet

COPY --from=build-env /home/dotnet/FrontendAccountManagement.Web/out .
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=0

ARG PORT=3000
ENV ASPNETCORE_URLS=http://*:${PORT}
EXPOSE ${PORT}

CMD ["dotnet", "FrontendAccountManagement.Web.dll"]
