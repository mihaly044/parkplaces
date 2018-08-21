# ParkPlaces
ParkPlaces is an all-in-one solution for managing and storing geographical information
about parking zones.

This is a free and open source application and takes tremendous amount of time to maintain.
 If this project has helped you, consider buying me a coffee:
 [![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://paypal.me/vodoc)

## Getting started

### Third party libraries
A number of awesome third party libraries have helped the development of ParkPlaces:

 -  [WatsonTcp](https://github.com/jchristn/WatsonTcp) by [jchristn](https://github.com/jchristn) [![nuget](https://img.shields.io/nuget/v/WatsonTcp.svg)](https://www.nuget.org/packages/WatsonTcp/)
 - [protobuf-net](https://github.com/mgravell/protobuf-net) by [mgravell](https://github.com/mgravell) [![nuget](https://img.shields.io/nuget/v/protobuf-net.svg)](https://www.nuget.org/packages/GMap.NET.WindowsForms/)
 - [greatmaps](https://github.com/radioman/greatmaps) by [radioman](https://github.com/radioman) [![nuget](https://img.shields.io/nuget/v/GMAP.Net.WindowsForms.svg)](https://www.nuget.org/packages/GMap.NET.WindowsForms/)
 - MySql.Data [![nuget](https://img.shields.io/nuget/v/Mysql.Data.svg)](https://www.nuget.org/packages/MySql.Data/8.0.12)
 - [DotUtils](https://github.com/mihaly044/parkplaces/tree/v2/ParkPlaces.DotUtils) by [J-kit](https://github.com/J-kit)

### Prequsities

 - .Net 4.6
 - Visual Studio 2017 Community or other 2017 editions
 - A MySQL database server - optional

### Building the project
The most straightforward way is to build the solution using Visual Studio 2017.
Although there are a number of command line build tools available, building from
the command line hasn't been tested with this project hence it's not recommended.

### Setting up the server
You may skip this section if you plan to use the client without a server.
**PPServer** is built to serve requests for the client and to manage the database.
Navigate to \PPServer\bin\Release\ and set up the server configuration as follows:

**PPServer.exe.config:**

Specify the port the server is going to listen on. Configuring an IP address is not necessary
as the server will try to guess it automatically. If you are not specifying an IP address, remember
to set **AutoConfig** to false.
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

