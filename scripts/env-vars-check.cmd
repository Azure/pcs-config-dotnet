@ECHO off & setlocal enableextensions enabledelayedexpansion

IF "%PCS_UICONFIG_WEBSERVICE_PORT%" == "" (
    echo Error: the PCS_UICONFIG_WEBSERVICE_PORT environment variable is not defined.
    exit /B 1
)

endlocal