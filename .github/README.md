# Hello!
Welcome to the DarkRift 2 open source project!

DarkRift 1 and 2 were originally written by Jamie Read. DarkRift 2 has since been open sourced under the care of Unordinal AB and the DarkRift community. Unfortunatly, Unordinal is no longer able to maintain the project, so it is being cared for by the DarkRift 2 community. Support for DarkRift is 100% community driven, and we encourage new users to join our [community discord](https://discord.gg/ufr5m7bX) 

# Features

DarkRift is an extremely performant multithreaded networking library best used for creating multiplayer experiance that require authoritative server. 

| **High Performance**                                                                                                                                                                         | **Unlimited CCU**                                                                                                                  | **Extremely Low Overhead**                                                                                                                                                                           | **Full TCP & UDP Support**                                                                                                                                               |   |
|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------|---|
| DarkRift was designed to be as fast, efficient and as powerful as you could ever need. Our multithreading expertise helps you take advantage of every CPU core and thread in your servers.   | DarkRift has no CCU limits. Scale to thousands of players without worrying about CCU restrictions.                                 | DarkRift overhead can go as low as just 3 bytes on UDP.                                                                                                                                              | DarkRift 2 introduces bi-channel communication allowing you to mix and  send UDP and TCP messages quickly and reliably depending on your needs.                          |   |
| **Embedded or Standalone**                                                                                                                                                                   | **Free, Forever**                                                                                                                  | **Authoritative**                                                                                                                                                                                    | **Flexible**                                                                                                                                                             |   |
| DarkRift provides support for both Unity embedded and standalone servers allowing you to  take advantage of existing Unity features or push for extreme performance with a standalone build. | DarkRift 2 is and will remain free and open source.                                                                                | DarkRift servers are fully authoritative. They own the communication and control  exactly what the client can and cannot do on your system. You write the logic, DarkRift will handle the messaging. | DarkRift is a message passing framework. It doesn't provide opinionated ways of doing things or try to force you into a programming paradigm that doesn't fit your needs |   |
| **Loves your protocol**                                                                                                                                                                      | **Scalability**                                                                                                                    | **Deep Metrics**                                                                                                                                                                                     | **Chat Filter (Bad Word Filter) and other goodies**                                                                                                                      |   |
| Got a favourite low level networking library you want to continue using?  Swap out DarkRift's Bichannel Network Listener for any library you like.                                           | With DarkRift's state of the art server clustering,  you can build a backend capable of seamlessly scaling  with your player base. | Built in support for Prometheus metrics means you can directly integrate with your existing metrics and monitoring solution like Grafana or Datadog.                                                 | DarkRift comes with some quality of life features including a chat filter, basic matchmaking support, custom metrics, and more.                                          |   |

# Getting Started
Grab the latest stable version from the [download page](https://github.com/DarkRiftNetworking/DarkRift/releases/).

You can find an example of a [minimal embedded .NET server and client](#minimal-example) here, or follow Any of the below community tutorials to get started.

[Bottom to Top Multiplayer with DarkRift](https://dev.to/robodoig/unity-multiplayer-bottom-to-top-46cj) - @Robodoig
[Source Code](https://github.com/RoboDoig/multiplayer-tutorial)

[FPS style tutorial](https://lukestampfli.github.io/EmbeddedFPSExample/guide/introduction.html) - @lukesta
[Source Code](https://github.com/LukeStampfli/EmbeddedFPSExample)

[Lets make An "MMO" series tutorial](https://benderj.com/lets-make-an-mmo-with-unity-darkrift-playfab-1/) - @Ace
[Source Code](https://github.com/MrBabadook/)

[How To Make A Multiplayer Game With DarkRift - Video Tutorial](https://www.youtube.com/watch?v=P1SayM0sqcA) - @Dexter

[Tic Tac Toe Tutorial - Video Tutorial by](https://www.youtube.com/watch?v=wqs39RIXmxc) - @HappySpider

For more resources and other guides related to DarkRift and multiplayer development in general, join the [community discord](https://discord.gg/ufr5m7bX) and see #resources.

## Building
This project requires Microsoft Visual Studio 2022 (the free Community edition is fine) or at least one Visual C# project will fail to build in VS2019 and below. See detailed exposition in [BUILDING.md](BUILDING.md)

## Source Code License
Most source files are licensed under MPL 2.0, with some exceptions where MIT applies. See [LICENSE.md](../LICENSE.md)

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
