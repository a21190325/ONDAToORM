# .NET Core WebAPI ONDAtoOrm Docker Container
This Docker image hosts a .NET Core WebAPI ONDAtoOrm that is exposed on port 8080, allowing easy deployment and configuration for various databases like PostgreSQL, Oracle, MySQL, MariaDB, and SQLite. The WebAPI supports external configuration through a customizable JSON settings file, enabling flexible connection strings. This .NET Core Web API provides endpoints to convert SQL scripts into C# code.

## Key Features:
* Platform: .NET Core WebAPI
* Port: Exposes the WebAPI on port 8080.
* Database Support: Supports multiple database connections including PostgreSQL, Oracle, MySQL, MariaDB, and SQLite.
* Volume Mapping:
   * Map the SQLite database file and settings configuration to Docker using volume mounts.
   * Easily replace connection strings in `appsettings.json` to use your desired database settings.

## Example Docker Run Command:
```
docker run -p 8080:8080 -v C:\{DIRECTORY_PATH}\TempSQLiteDb.db:/app/Data/TempSQLiteDb.db \
-v C:\{DIRECTORY_PATH}\appsettings.json:/app/appsettings.json \
--name ONDAtoOrm -d a21190325/ondatoormwebapi
```

In this command:
* Port Mapping: Maps local port `8080` to container port `8080`.
* Volume Mapping:
   * Maps the local SQLite database file to `/app/Data/TempSQLiteDb.db` inside the container.
   * Maps the local configuration file `appsettings.json` to `/app/appsettings.json` inside the container.

## Customization:
To customize the connection strings for different databases, provide an `appsettings.json` file in the following structure and map it using the `-v` option:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "PostgreSQL": "Host={HOST};Username={USERNAME};Password={PASSWORD};POOLING=True;MINPOOLSIZE=1;MAXPOOLSIZE=100",
    "Oracle": "Data Source={HOST}:{PORT}/FREEPDB1;User Id={USERNAME};Password={PASSWORD};",
    "MySql": "Server={HOST};User ID={USERNAME};Password={PASSWORD}",
    "MariaDB": "Server={HOST};Port={PORT};User ID={USERNAME};Password={PASSWORD}",
    "SQLite": "Data Source=Data/TempSQLiteDb.db"
  }
}
```
> [!NOTE]
> Simply modify the connection string values for your specific database setup, and the WebAPI will connect to your desired backend with ease.

## Exposed endpoints

### A) Authentication request to get Bearer token

**POST URL:** /api/Identity/token

Body:
```json
{
  "username": "{USERNAME}",
  "password": "{PASSWORD}"
}
```

Response:
```json
{
  "data": {
    "user": {
      "id": {USER_ID},
      "username": "{USERNAME}",
      "password": "",
      "role": "{USER_ROLE}"
    },
    "token": "{BEARER_TOKEN}"
  },
  "errors": [],
  "isValid": true
}
```

### B) Convertion request to transform SQL scripts in C# classes
**POST URL:** /api/Identity/token

Body:
```json
{
    "DatabaseEngine": {Databse engine (example: Sqlite, Oracle, SQLServer)},
    "SqlContentInBase64": {SQL script encoded in base64}
}
```

Response:
```json
{
    "data": [
        {
            "code": "{Code of c# file encoded in base64}",
            "name": "{FILENAME_1}.cs"
        },
        {
            "code": "{Code of c# file encoded in base64}",
            "name": "{FILENAME_2}.cs"
        }
    ],
    "errors": [],
    "isValid": true
}
```