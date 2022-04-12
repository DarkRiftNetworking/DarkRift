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

using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using UnityEngine;
using UnityEngine.UI;

public class ChatDemo : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The client to communicate with the server via.")]
    UnityClient client;

    [SerializeField]
    [Tooltip("The InputField the user can type in.")]
    InputField input;

    [SerializeField]
    [Tooltip("The transform to place new messages in.")]
    Transform chatWindow;

    [SerializeField]
    [Tooltip("The scrollrect for the chat window (if present).")]
    ScrollRect scrollRect;

    [SerializeField]
    [Tooltip("The message prefab where messages will be added.")]
    GameObject messagePrefab;

    void Awake()
    {
        //Check we have a client to send/receive from
        if (client == null)
        {
            Debug.LogError("No client assigned to Chat component!");
            return;
        }

        //Subscribe to the event for when we receive messages
        client.MessageReceived += Client_MessageReceived;
        client.Disconnected += Client_Disconnected;
    }

    private void Client_MessageReceived(object sender, MessageReceivedEventArgs e)
    {
        //Get an instance of the message received
        using (Message message = e.GetMessage() as Message)
        {
            //Get the DarkRiftReader from the message and read the text in it into the UI
            using (DarkRiftReader reader = message.GetReader())
                AddMessage(reader.ReadString());
        }
    }

    void Client_Disconnected(object sender, DisconnectedEventArgs e)
    {
        //If we've disconnected add a message to say whether it was us or the server that triggered the 
        //disconnection
        if (e.LocalDisconnect)
            AddMessage("You have disconnected from the server.");
        else
            AddMessage("You were disconnected from the server.");
    }

    void AddMessage(string message)
    {
        //Now we need to create a new UI object to put the message in so instantiate our prefab and add it 
        //as a child to the chat window
        GameObject messageObj = Instantiate(messagePrefab) as GameObject;
        messageObj.transform.SetParent(chatWindow);

        //We need the Text component so search for it
        Text text = messageObj.GetComponentInChildren<Text>();

        //If the Text component is present then assign the text out message
        if (text != null)
            text.text = message;
        else
            Debug.LogError("Message object does not contain a Text component!");

        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    //This will be called when the user presses enter in the input field
    public void MessageEntered()
    {
        //Check we have a client to send from
        if (client == null)
        {
            Debug.LogError("No client assigned to Chat component!");
            return;
        }

        //First we need to build a DarkRiftWriter to put the data we want to send in, it'll default to Unicode 
        //encoding so we don't need to worry about that
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            //We can then write the input text into it
            writer.Write(input.text);

            //Next we construct a message, in this case we can just use a default tag because there is nothing fancy
            //that needs to happen before we read the data.
            using (Message message = Message.Create(0, writer))
            {
                //Finally we send the message to everyone connected!
                client.SendMessage(message, SendMode.Reliable);
            }
        }
    }
}
