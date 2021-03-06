# ParkPlaces [![Build status](https://ci.appveyor.com/api/projects/status/w20s8o9fxb5aqcj7?svg=true)](https://ci.appveyor.com/project/mihaly044/parkplaces) 

ParkPlaces is an all-in-one solution for managing and storing geographical information
about parking zones.


[![Build history](https://buildstats.info/appveyor/chart/mihaly044/parkplaces)](https://ci.appveyor.com/project/mihaly044/parkplaces/history)

This is a free and open source application and takes tremendous amount of time to maintain.
 If this project has helped you, consider buying me a coffee:
 
 [![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://paypal.me/vodoc)
 

### Third party libraries
A number of awesome third party libraries have helped the development of ParkPlaces:

 - [ParkPlaces.DotUtils](https://github.com/mihaly044/parkplaces/tree/v2/ParkPlaces.DotUtils) by [J-kit](https://github.com/J-kit)
 - [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json) by [JamesNK](https://github.com/JamesNK/)
 - [WatsonTcp](https://github.com/jchristn/WatsonTcp) by [jchristn](https://github.com/jchristn) 
 - [protobuf-net](https://github.com/mgravell/protobuf-net) by [mgravell](https://github.com/mgravell)
 - [GMap.NET.WindowsForms](https://github.com/radioman/greatmaps) by [radioman](https://github.com/radioman)
 - MySql.Data [![nuget](https://img.shields.io/nuget/v/Mysql.Data.svg)](https://www.nuget.org/packages/MySql.Data/8.0.12)

## Getting started

### Prequsities

 - .NET 4.6.1, .NET core 2.1 and .NET standard 2.0
 - Visual Studio 2017 Community or other 2017 editions
 - A MySQL database server

### Building the project
#### Using prebuilt binaries
If you don't want to build everything yourself, you might grab the prebuilt release binaries from [here](https://github.com/mihaly044/parkplaces/releases/latest).

#### Building from sources and targeting  specific platforms
##### Building from Visual Studio 2017 (Windows only):
Open `ParkPlaces.sln` in Visual Studio and build the project.
The client files will be in the `ParkPlaces/bin/Release` folder and the server files will be under `PPServer/bin/Release`.

##### Deploying the server
To build the deployable binaries of PPServer, select  `Publish-win-x86` or `Publish-linux-x64` build configuration in Visual Studio and build the project.
The deployable binaries will be in `PPServer\Deploy` folder.
***Note***: Do **not** use binaries from  `PPServer\bin` for production deployments.

##### Building from the CLI, Windows only (not recommended):
```batch
msbuild /p:WarningLevel=0 /p:Configuration=Release
```
or with deployable binaries:
```batch
msbuild /p:WarningLevel=0 /p:Configuration=Publish-win-x86
```
#### Building from Linux
PPServer can also run on Linux thanks to .NET core 2.1.  To build the server navigate to `PPServer` and build with dotnet:
```bash
dotnet publish -c Release -r=linux-x64
```
The client is Windows only and cannot be built on Linux.

### Setting up the server
You may skip this section if you plan to use the client without a server.
**PPServer** is built to serve requests for the client and to manage the database.
Navigate to \PPServer\bin\Release\ and set up the server configuration as follows:

**PPServer.dll.config:**

Specify the port the server is going to listen on. Configuring an IP address is not necessary
as the server will try to guess it automatically. If you are not specifying an IP address, remember
to set **AutoConfig** to true.
```xml
...
<ServerConfiguration>
   <add key="IPAddress" value="192.168.0.1" />
   <add key="Port" value="11000" />
   <add key="AutoConfig" value="true"/>
</ServerConfiguration>
...
```

#### Database configuration

There is a Main and an Alt database connection. This is useful when switcing between development and production environments. To specify which configuration the server will use, use either "**alt**" or "**main**"
keyword as follows:

```xml
...
<appSettings>
   ...
   <add key="DBConnection" value="alt" />
   ...
</appSettings>
...
```

```xml
...
<AltDBConnection>
   <add key="server" value="localhost" />
   <add key="user" value="root" />
   <add key="password" value="" />
   <add key="database" value="parkplaces" />
   <add key="port" value="3306" />
</AltDBConnection>
...
```

### Setting up the client
You may skip this section if you plan to use the client without the server.
Navigate to \ParkPlaces\bin\Release\ and set the server
port and ip address as follows:

**ParkPlaces.exe.config:**
```xml
...
<appSettings>
   ...
   <add key="ServerIP" value="192.168.1.100" />
   <add key="ServerPort" value="11000" />
   ...
</appSettings>
...
```
The ServerPort key can be left out. The default port number is **11000** unless else specified.
#### The default login credentials are 
|Username|Password |
|--|--|
| admin | admin |
