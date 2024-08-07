# frontend-account-management-microservice

This repostory contains the code for the frontend of the Account Management domain for Extended Producer Responsibility for Packaging (EPR).

## Prerequisites

* .NET 8.0 SDK.
* Any IDE able to open projects written in c# 10, like Visual Studio or VS Code.
* Access to epr-packaging-common NuGet package repository.

## Setup

In Visual Studio, or similar IDE:

1. Open ~/src/FrontendAccountManagement.sln
2. Build the project, if you have permission to access epr-packaging-common NuGet package repository, it will download any missing package.

## Running in development

This service can run in two different modes: `Packaging` and `Regulating`. To swap between one mode and the another, change `ServiceSettings.ServiceKey` in `appsettings.json`.

## Running tests

Using either the IDE, or dotnet CLI run following projects:

* FrontendAccountManagement.Core.UnitTests
* FrontendAccountManagement.IntegrationTests
* FrontendAccountManagement.Web.UnitTests

## Contributing to this project

Please read the [contribution guidelines](/CONTRIBUTING.md) before submitting a pull request.

## Licence

THIS INFORMATION IS LICENSED UNDER THE CONDITIONS OF THE OPEN GOVERNMENT LICENCE found at:

<http://www.nationalarchives.gov.uk/doc/open-government-licence/version/3>

The following attribution statement MUST be cited in your products and applications when using this information.

>Contains public sector information licensed under the Open Government licence v3

### About the licence

The Open Government Licence (OGL) was developed by the Controller of Her Majesty's Stationery Office (HMSO) to enable information providers in the public sector to license the use and re-use of their information under a common open licence.

It is designed to encourage use and re-use of information freely and flexibly, with only a few conditions.