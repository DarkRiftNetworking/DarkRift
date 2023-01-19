# Hello!
Welcome to the DarkRift 2 open source project!

DarkRift 1 and 2 were originally written by Jamie Read. DarkRift 2 has since been open sourced under the care of Unordinal AB and the DarkRift community.

# Getting Started
Grab the latest stable version from the [download page](https://github.com/DarkRiftNetworking/DarkRift/releases/).

You can find an example of a [minimal embedded .NET server and client](#minimal-example) or follow the longer .NET plugin with Unity client tutorial at [darkriftnetworking.com](https://www.darkriftnetworking.com/).

## Building
This project requires Microsoft Visual Studio 2022 (the free Community edition is fine) or at least one Visual C# project will fail to build in VS2019 and below. See detailed exposition in [BUILDING.md](BUILDING.md)

## Source Code License
Most source files are licensed under MPL 2.0, with some exceptions where MIT applies. See [LICENSE.md](LICENSE.md)

## Contributing
We are happy to see community contributions to this project. See [CONTRIBUTING.md](CONTRIBUTING.md)

## Code of Conduct
Be civil. See [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md)

## Wiki
The [wiki](https://github.com/DarkRiftNetworking/DarkRift-Networking/wiki) is publicly editable and is a place for anyone to add content, code snippets, tutorials and anything that would be useful to other members of the DarkRift Networking community.

Feel free to add pages and use the space as you wish. You are more than welcome (and even encouraged) to cross post from personal blogs and link to external sites (as long as it's relevant)!

DarkRift Networking is not responsible for any content or links on the wiki, although we will monitor it nevertheless.

## Minimal Example

Examples are using plain .NET with C# 9.0 top level statements for clarity. You can also use similar code in Unity.

First, we start a server that gets its settings from the local file server.config.

```csharp
using DarkRift;
using DarkRift.Server;

ServerSpawnData spawnData = ServerSpawnData.CreateFromXml("Server.config");

var server = new DarkRiftServer(spawnData);

void Client_MessageReceived(object? sender, MessageReceivedEventArgs e)
{
    using Message message = e.GetMessage();
    using DarkRiftReader reader = message.GetReader();
    Console.WriteLine("Received a message from the client: " + reader.ReadString());
}

void ClientManager_ClientConnected(object? sender, ClientConnectedEventArgs e)
{
    e.Client.MessageReceived += Client_MessageReceived;

    using DarkRiftWriter writer = DarkRiftWriter.Create();
    writer.Write("World of Hel!");

    using Message secretMessage = Message.Create(666, writer);
    e.Client.SendMessage(secretMessage, SendMode.Reliable);
}

server.ClientManager.ClientConnected += ClientManager_ClientConnected;

server.StartServer();

Console.ReadKey(); // Wait until key press. Not necessary in Unity.
```

The XML file server.config looks like this (hard to make shorter).

```xml
<?xml version="1.0" encoding="utf-8" ?>
<!--
  Configuring DarkRift server to listen at ports TCP 4296 and UDP 4297.
-->
<configuration xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="https://www.darkriftnetworking.com/DarkRift2/Schemas/2.3.1/Server.config.xsd">
  <server maxStrikes="5" />
  
  <pluginSearch/>
 
  <logging>
    <logWriters>
      <logWriter name="ConsoleWriter1" type="ConsoleWriter" levels="trace, info, warning, error, fatal">
        <settings useFastAnsiColoring="false" />
      </logWriter>
    </logWriters>
  </logging>

  <plugins loadByDefault="false"/>

  <data directory="Data/"/>

  <listeners>
    <listener name="DefaultNetworkListener" type="BichannelListener" address="0.0.0.0" port="4296">
      <settings noDelay="true" udpPort="4297" />
    </listener>
  </listeners>
</configuration>

```

And finally, here is a client that connects to the server and sends "Hello world!" whilst receiving a string that should be "World of Hel!" (just be mindful about pressing any key since that terminates the program early).

```csharp
using DarkRift;
using DarkRift.Client;
using System.Net;

var client = new DarkRiftClient();

void Client_MessageReceived(object? sender, MessageReceivedEventArgs e)
{
    using Message message = e.GetMessage();
    using DarkRiftReader reader = message.GetReader();
    Console.WriteLine("Received a message from the server: " + reader.ReadString());
}

client.MessageReceived += Client_MessageReceived;

client.Connect(IPAddress.Loopback, tcpPort:4296, udpPort:4297, noDelay:true);

Console.WriteLine("Connected!");

using DarkRiftWriter writer = DarkRiftWriter.Create();
writer.Write("Hello world!");

using Message secretMessage = Message.Create(1337, writer);
client.SendMessage(secretMessage, SendMode.Reliable);

Console.ReadKey(); // Wait until key press. Not necessary in Unity.
```

Do note that "Connected!" message can be printed even after "World of Hel!" since DR2 is multithreaded.

This was an example of embedding DarkRift into your own programs. You can instead choose to implement DarkRift.Server.Plugin (see the manual) for looser coupling.