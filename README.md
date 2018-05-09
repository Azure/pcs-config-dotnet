
[![Build][build-badge]][build-url]
[![Issues][issues-badge]][issues-url]
[![Gitter][gitter-badge]][gitter-url]

# Config Service Overview

This service handles communication with the [Storage Adapter] microservice to complete tasks.

The microservice provides a RESTful endpoint to make CRUD operations for
"devicegroups", "solution-settings", and "user-settings".
The data will be stored by the [Storage Adapter] microservice.

## Why?

This microservice was built as part of the 
[Azure IoT Remote Monitoring](https://github.com/Azure/azure-iot-pcs-remote-monitoring-dotnet)
project to provide a generic implementation for an end-to-end IoT solution.
More information [here][rm-arch-url].

## Features
* Create or update device groups
* Get all or a single device group
* Get or upload logo
* Get or set overall solution settings
* Get or set individual user settings

## Documentation
* View the API documentation in the [Wiki](https://github.com/Azure/pcs-config-dotnet/wiki)

# How to Use

## Running the Service with Docker
You can run the microservice and its dependencies using
[Docker](https://www.docker.com/) with the instructions
[here][run-with-docker-url].

## Running the Service Locally
## Prerequisites

### 1. Deploy Azure Services

This service has a dependency on the following Azure resources. 
Follow the instructions for 
[Deploy the Azure services](https://docs.microsoft.com/azure/iot-suite/iot-suite-remote-monitoring-deploy-local#deploy-the-azure-services).

* Cosmos DB
* Iot Hub
* Maps (optional)

### 2. Setup Dependencies

This service depends on the following repositories.
Run those services from the instructions in their READMEs in the following order.

1. [Storage Adapter Dotnet Microservice](https://github.com/Azure/pcs-storage-adapter-dotnet)
1. [Telemetry Dotnet Microservice](https://github.com/Azure/device-telemetry-dotnet)
1. [IoTHub Manager Dotnet Microservice](https://github.com/Azure/iothub-manager-dotnet)
1. [Device Simulation Dotnet Microservice](https://github.com/Azure/device-simulation-dotnet)

### 3. Environment variables required to run the service
In order to run the service, some environment variables need to be
created at least once. See specific instructions for IDE or command
line setup below for more information. More information on environment
variables [here](#configuration-and-environment-variables).

* `PCS_STORAGEADAPTER_WEBSERVICE_URL` - the url for
  the [Storage Adapter Webservice](https://github.com/Azure/pcs-storage-adapter-dotnet)
  used for key value storage
* `PCS_TELEMETRY_WEBSERVICE_URL` - the url for
  the [Telemetry Webservice](https://github.com/Azure/device-telemetry-dotnet.git)
  used for key value storage
* `PCS_DEVICESIMULATION_WEBSERVICE_URL` - the url for
  the [Device Simulation Webservice](https://github.com/Azure/device-simulation-dotnet.git)
  used for key value storage
* `PCS_IOTHUBMANAGER_WEBSERVICE_URL` - the url for
  the [IOT Hub Manager Webservice](https://github.com/Azure/iothub-manager-dotnet.git)
  used for key value storage
*  `PCS_AZUREMAPS_KEY` - the [Azure Maps](https://azure.microsoft.com/services/azure-maps/) 
  API Key. This can be set to "static" if you do not have one.

## Running the service with Visual Studio
1. Make sure the [Prerequisites](#prerequisites) are set up.
1. Install any edition of [Visual Studio 2017][vs-install-url] or Visual
   Studio for Mac. When installing check ".NET Core" workload. If you
   already have Visual Studio installed, then ensure you have
   [.NET Core Tools for Visual Studio 2017][dotnetcore-tools-url]
   installed (Windows only).
1. Open the solution in Visual Studio
1. Edit the WebService project properties by right clicking on the 
Webservice project > Properties > Debug. Add following required environment 
variables to the Debug settings. In Windows you can also set these 
[in your system][windows-envvars-howto-url].
   1. `PCS_STORAGEADAPTER_WEBSERVICE_URL` = http://localhost:9022/v1
   1. `PCS_DEVICESIMULATION_WEBSERVICE_URL` = http://localhost:9003/v1
   1. `PCS_IOTHUBMANAGER_WEBSERVICE_URL` = http://localhost:9002/v1
   1. `PCS_TELEMETRY_WEBSERVICE_URL` = http://localhost:9004/v1
   1. `PCS_AZUREMAPS_KEY` = static
1. In Visual Studio, start the WebService project
1. Using an HTTP client like [Postman][postman-url], use the 
[RESTful API](https://github.com/Azure/pcs-config-dotnet/wiki/API-Specs)
to test out the service.

## Running the service from the command line

1. Make sure the [Prerequisites](#prerequisites) are set up.
1. Set the following environment variables in your system. 
More information on environment variables
[here](#configuration-and-environment-variables).
   1. `PCS_STORAGEADAPTER_WEBSERVICE_URL` = http://localhost:9022/v1
   1. `PCS_DEVICESIMULATION_WEBSERVICE_URL` = http://localhost:9003/v1
   1. `PCS_IOTHUBMANAGER_WEBSERVICE_URL` = http://localhost:9002/v1
   1. `PCS_TELEMETRY_WEBSERVICE_URL` = http://localhost:9004/v1
   1. `PCS_AZUREMAPS_KEY` = static
1. Use the scripts in the [scripts](scripts) folder for many frequent tasks:
   *  `build`: compile all the projects and run the tests.
   *  `compile`: compile all the projects.
   *  `run`: compile the projects and run the service. This will prompt for
  elevated privileges in Windows to run the web service.

## Project Structure
This microservice contains the following projects:
* **WebService.csproj** - C# web service exposing REST interface for config functionality
* **WebService.Test.csproj** - Unit tests for web services functionality
* **Services.csproj** - C# assembly containining business logic for interacting 
with storage microserivce, telemetry microservice, device simulation microservice
and IoTHub manager microservice
* **Services.Test.csproj** - Unit tests for services functionality
* **Solution/scripts** - Contains build scripts, docker container creation scripts, 
and scripts for running the microservice from the command line

# Updating the Docker image
The `scripts` folder includes a [docker](scripts/docker) subfolder with the files
required to package the service into a Docker image:

* `Dockerfile`: docker images specifications
* `build`: build a Docker container and store the image in the local registry
* `run`: run the Docker container from the image stored in the local registry
* `content`: a folder with files copied into the image, including the entry point script

# Configuration and Environment variables

The service configuration is stored using ASP.NET Core configuration
adapters, in [appsettings.ini](WebService/appsettings.ini). The INI format allows to
store values in a readable format, with comments. The application also
supports references to environment variables, which is used to import
credentials and networking details.

The configuration files in the repository reference some environment
variables that need to be created at least once. Depending on your OS and
the IDE, there are several ways to manage environment variables:

* Windows: the variables can be set [in the system][windows-envvars-howto-url]
  as a one time only task. The
  [env-vars-setup.cmd](scripts/env-vars-setup.cmd) script included needs to
  be prepared and executed just once. The settings will persist across
  terminal sessions and reboots.
* Visual Studio: the variables can be set in the project settings for WebService
  under Project Properties -> Configuration
  Properties -> Environment
* For Linux and OSX environments, the [env-vars-setup](scripts/env-vars-setup)
  script needs to be executed every time a new console is opened.
  Depending on the OS and terminal, there are ways to persist values
  globally, for more information these pages should help:
  * https://stackoverflow.com/questions/13046624/how-to-permanently-export-a-variable-in-linux
  * https://stackoverflow.com/questions/135688/setting-environment-variables-in-os-x
  * https://help.ubuntu.com/community/EnvironmentVariables

# Contributing to the solution
Please follow our [contribution guildelines](CONTRIBUTING.md) and code style
conventions.

# Feedback
Please enter issues, bugs, or suggestions as 
[GitHub Issues](https://github.com/Azure/pcs-config-dotnet/issues).

# License
Copyright (c) Microsoft Corporation. All rights reserved.
Licensed under the [MIT](LICENSE) License.

[build-badge]: https://img.shields.io/travis/Azure/pcs-config-dotnet.svg
[build-url]: https://travis-ci.org/Azure/pcs-config-dotnet
[issues-badge]: https://img.shields.io/github/issues/azure/pcs-config-dotnet.svg
[issues-url]: https://github.com/azure/pcs-config-dotnet/issues
[gitter-badge]: https://img.shields.io/gitter/room/azure/iot-solutions.js.svg
[gitter-url]: https://gitter.im/azure/iot-solutions
[windows-envvars-howto-url]: https://superuser.com/questions/949560/how-do-i-set-system-environment-variables-in-windows-10
[Storage Adapter]:https://github.com/Azure/pcs-storage-adapter-dotnet/blob/master/README.md
[rm-arch-url]:https://docs.microsoft.com/en-us/azure/iot-suite/iot-suite-remote-monitoring-sample-walkthrough
[run-with-docker-url]:https://docs.microsoft.com/azure/iot-suite/iot-suite-remote-monitoring-deploy-local#run-the-microservices-in-docker
[postman-url]: https://www.getpostman.com
