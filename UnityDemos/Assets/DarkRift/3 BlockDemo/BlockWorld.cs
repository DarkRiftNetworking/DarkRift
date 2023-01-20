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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

/// <summary>
///     Manages the world of blocks.
/// </summary>
internal class BlockWorld : MonoBehaviour
{
    /// <summary>
    ///     The client to communicate with the server via.
    /// </summary>
    [SerializeField]
    [Tooltip("The client to communicate with the server via.")]
    UnityClient client;

    /// <summary>
    ///     The block prefab to spawn in the world.
    /// </summary>
    [SerializeField]
    [Tooltip("The block object to spawn.")]
    GameObject blockPrefab;
    
    /// <summary>
    ///     The list of blocks spawned.
    /// </summary>
    List<GameObject> blocks = new List<GameObject>();

    void Awake()
    {
        if (client == null)
        {
            Debug.LogError("No client assigned to BlockWorld component!");
            return;
        }

        client.MessageReceived += Client_MessageReceived;
    }

    /// <summary>
    ///     INvoked when the server receives a message.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void Client_MessageReceived(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage() as Message)
        {
            //If we're placing a block we need to instantiate our prefab
            if (message.Tag == BlockTags.PlaceBlock)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    Vector3 position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                    GameObject o = Instantiate(
                        blockPrefab,
                        position,
                        Quaternion.identity
                    ) as GameObject;

                    o.transform.SetParent(transform);

                    blocks.Add(o);

                }
            }
            //If we're destroying we need to find the block and destroy it
            else if (message.Tag == BlockTags.DestroyBlock)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    Vector3 position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                    GameObject block = blocks.SingleOrDefault(b => b != null && b.transform.position == position);

                    if (block == null)
                        return;

                    Destroy(block);

                    blocks.Remove(block);
                }

            }
        }
    }

    internal void AddBlock(Vector3 position)
    {
        if (client == null)
        {
            Debug.LogError("No client assigned to BlockWorld component!");
            return;
        }

        //Don't worry about snapping, we'll do that on the server
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(position.x);
            writer.Write(position.y);
            writer.Write(position.z);

            using (Message message = Message.Create(BlockTags.PlaceBlock, writer))
                client.SendMessage(message, SendMode.Reliable);
        }
    }

    internal void DestroyBlock(Vector3 position)
    {
        if (client == null)
        {
            Debug.LogError("No client assigned to BlockWorld component!");
            return;
        }

        //Don't worry about snapping, we'll do that on the server
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(position.x);
            writer.Write(position.y);
            writer.Write(position.z);

            using (Message message = Message.Create(BlockTags.DestroyBlock, writer))
                client.SendMessage(message, SendMode.Reliable);
        }
    }

    internal void RemoveAllBlocks()
    {
        foreach (GameObject block in blocks)
            Destroy(block);

        blocks.Clear();
    }
}
