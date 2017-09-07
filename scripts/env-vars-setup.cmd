:: Prepare the environment variables used by the application

:: Endpoint to reach the storage adapter
SETX PCS_STORAGEADAPTER_WEBSERVICE_URL "http://127.0.0.1:9022/v1"
:: Endpoint to reach the device simlation
SETX PCS_DEVICESIMULATION_WEBSERVICE_URL "http://127.0.0.1:9003/v1"
:: Endpoint to reach the device simlation
SETX PCS_IOTHUB_MANAGER_WEBSERVICE_URL "http://127.0.0.1:9002/v1"