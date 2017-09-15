@ECHO off & setlocal enableextensions enabledelayedexpansion

IF "%PCS_STORAGEADAPTER_WEBSERVICE_URL%" == "" (
    echo Error: the PCS_STORAGEADAPTER_WEBSERVICE_URL environment variable is not defined.
    exit /B 1
)

IF "%PCS_DEVICESIMULATION_WEBSERVICE_URL%" == "" (
    echo Error: the PCS_DEVICESIMULATION_WEBSERVICE_URL environment variable is not defined.
    exit /B 1
)

IF "%PCS_DEVICETELEMETRY_WEBSERVICE_URL%" == "" (
    echo Error: the PCS_DEVICETELEMETRY_WEBSERVICE_URL environment variable is not defined.
    exit /B 1
)

endlocal