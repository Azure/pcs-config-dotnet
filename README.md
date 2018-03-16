
[![Build][build-badge]][build-url]
[![Issues][issues-badge]][issues-url]
[![Gitter][gitter-badge]][gitter-url]

Config service Overview
=======================
This service handles communication with the [Storage Adapter] microservice to complete tasks.

The microservice provides a RESTful endpoint to make CRUD operations for "devicegroups", "solution-settings", and "user-settings". The data will be stored by the [Storage Adapter] microservice.

Dependencies
============
- [Storage adapter microservice] used to store data

How to use the microservice
===========================
## Local Setup

### 1. Environment Variables

Run `scripts\env-vars-setup.cmd` on Windows or `source scripts/env-vars-setup`
on Mac/Linux to set up the environment variables needed to run the service locally.
In Windows you can also set these [in your system][windows-envvars-howto-url].

If using envornemnt variables, this service requires the following environment
variables to be set:
- `PCS_STORAGEADAPTER_WEBSERVICE_URL` - the url for
  the [Storage Adapter Webservice](https://github.com/Azure/pcs-storage-adapter-dotnet)
  used for key value storage
- `PCS_TELEMETRY_WEBSERVICE_URL` - the url for
  the [Telemetry Webservice](https://github.com/Azure/device-telemetry-dotnet.git)
  used for key value storage
- `PCS_DEVICESIMULATION_WEBSERVICE_URL` - the url for
  the [Device Simulation Webservice](https://github.com/Azure/device-simulation-dotnet.git)
  used for key value storage
- `PCS_IOTHUBMANAGER_WEBSERVICE_URL` - the url for
  the [IOT Hub Manager Webservice](https://github.com/Azure/iothub-manager-dotnet.git)
  used for key value storage

## Quickstart - Running the service with Docker
You can quickly start the Config service and its dependencies in one simple step, using Docker Compose with the
[docker-compose.yml](scripts/docker/docker-compose.yml) file in the project:

```
cd scripts/docker
docker-compose up
```

The Docker compose configuration requires the `PCS_STORAGEADAPTER_WEBSERVICE_URL` environment variable.

Build and Run from the command line
===================================
The [scripts](scripts) folder contains scripts for many frequent tasks:

* `compile`: compile the projects.
* `build`: compile the projects and run the tests.
* `run`: compile the projects and run the service. This will prompt for
  elevated privileges in Windows to run the web service.

The scripts check for the environment variables setup. You can set the
environment variables globally in your OS, or use the "env-vars-setup"
script in the scripts folder.

Updating the Docker image
=========================

The `scripts` folder includes a [docker](scripts/docker) subfolder with the
files required to package the service into a Docker image:

* `build`: build a Docker container and store the image in the local registry
* `run`: run the Docker container from the image stored in the local registry

Contributing to the solution
============================
Please follow our [contribution guildelines](CONTRIBUTING.md) and code style
conventions.

Feedback
========
Please enter issues, bugs, or suggestions as GitHub Issues here:
https://github.com/Azure/pcs-config-dotnet/issues.

[build-badge]: https://img.shields.io/travis/Azure/pcs-config-dotnet.svg
[build-url]: https://travis-ci.org/Azure/pcs-config-dotnet
[issues-badge]: https://img.shields.io/github/issues/azure/pcs-config-dotnet.svg
[issues-url]: https://github.com/azure/pcs-config-dotnet/issues
[gitter-badge]: https://img.shields.io/gitter/room/azure/iot-solutions.js.svg
[gitter-url]: https://gitter.im/azure/iot-solutions
[windows-envvars-howto-url]: https://superuser.com/questions/949560/how-do-i-set-system-environment-variables-in-windows-10
[Storage Adapter]:https://github.com/Azure/pcs-storage-adapter-dotnet/blob/master/README.md
[Azure DocumentDB]:(https://ms.portal.azure.com/#create/Microsoft.DocumentDB)
