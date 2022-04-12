/*
Copyright (c) 2022 Unordinal AB

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System.Net;
using DarkRift;
using DarkRift.Server;
using DarkRift.Client.Unity;
using DarkRift.Server.Unity;
using UnityEngine;
using ServerMessageReceivedEventArgs = DarkRift.Server.MessageReceivedEventArgs;

public class Issue75 : MonoBehaviour
{
    public UnityClient Client;
    public XmlUnityServer Server;

    void Start()
    {
        Server.Server.ClientManager.ClientConnected += OnClientConnected;
        Client.Connect(IPAddress.Parse("127.0.0.1"), 4296, true);
    }

    private void OnClientConnected(object sender, ClientConnectedEventArgs e)
    {
        e.Client.MessageReceived += ServerOnMessageReceived;
    }

    private void ServerOnMessageReceived(object sender, ServerMessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage())
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                for (int i = 0; i < 30; i++)
                {
                    ushort test = reader.ReadUInt16();
                    if (test != 1)
                    {
                        Debug.Log("Received malformatted message!");
                    }
                }
            }
        }
    }

    private ushort counter;
    private ushort counter2;
    void FixedUpdate()
    {
        for (int i = 0; i < 25; i++)
        {
            send();
        }
    }


    void send()
    {
        if (counter2 >= 5000)
        {
            return;
        }
        counter++;
        counter %= 10;
        if (counter == 0)
        {
            counter2++;
        }
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            for (int i = 0; i < 30; i++)
            {
                writer.Write((ushort)1);
            }
            using (Message message = Message.Create(counter, writer))
            {
                Client.SendMessage(message, SendMode.Reliable);
            }
        }
    }

}
