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
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Since <see cref="ObjectCacheSettings"/> uses properties, Unity can't serialize it to we clone it here and provide conversion methods.
/// </summary>
[Serializable]
public sealed class SerializableObjectCacheSettings
{
#pragma warning disable IDE0044 // Add readonly modifier, Unity can't serialize readonly fields
    [Tooltip("")]
    [SerializeField]
    private int maxWriters = 2;

    [Tooltip("")]
    [SerializeField]
    private int maxReaders = 2;

    [Tooltip("")]
    [SerializeField]
    private int maxMessages = 4;

    [Tooltip("")]
    [SerializeField]
    private int maxMessageBuffers = 4;

    [Tooltip("")]
    [SerializeField]
    private int maxSocketAsyncEventArgs = 32;

    [Tooltip("")]
    [SerializeField]
    private int maxActionDispatcherTasks = 16;

    [Tooltip("")]
    [SerializeField]
    private int maxAutoRecyclingArrays = 4;

    [Tooltip("")]
    [SerializeField]
    private int extraSmallMemoryBlockSize = 16;

    [Tooltip("")]
    [SerializeField]
    private int maxExtraSmallMemoryBlocks = 2;

    [Tooltip("")]
    [SerializeField]
    private int smallMemoryBlockSize = 64;

    [Tooltip("")]
    [SerializeField]
    private int maxSmallMemoryBlocks = 2;

    [Tooltip("")]
    [SerializeField]
    private int mediumMemoryBlockSize = 256;

    [Tooltip("")]
    [SerializeField]
    private int maxMediumMemoryBlocks = 2;

    [Tooltip("")]
    [SerializeField]
    private int largeMemoryBlockSize = 1024;

    [Tooltip("")]
    [SerializeField]
    private int maxLargeMemoryBlocks = 2;

    [Tooltip("")]
    [SerializeField]
    private int extraLargeMemoryBlockSize = 4096;

    [Tooltip("")]
    [SerializeField]
    private int maxExtraLargeMemoryBlocks = 2;

    [Tooltip("")]
    [SerializeField]
    private int maxMessageReceivedEventArgs = 4;
#pragma warning restore IDE0044 // Add readonly modifier, Unity can't serialize readonly fields

    public ClientObjectCacheSettings ToClientObjectCacheSettings()
    {
        return new ClientObjectCacheSettings {
            MaxWriters = maxWriters,
            MaxReaders = maxReaders,
            MaxMessages = maxMessages,
            MaxMessageBuffers = maxMessageBuffers,
            MaxSocketAsyncEventArgs = maxSocketAsyncEventArgs,
            MaxActionDispatcherTasks = maxActionDispatcherTasks,
            MaxAutoRecyclingArrays = maxAutoRecyclingArrays,

            ExtraSmallMemoryBlockSize = extraSmallMemoryBlockSize,
            MaxExtraSmallMemoryBlocks = maxExtraSmallMemoryBlocks,
            SmallMemoryBlockSize = smallMemoryBlockSize,
            MaxSmallMemoryBlocks = maxSmallMemoryBlocks,
            MediumMemoryBlockSize = mediumMemoryBlockSize,
            MaxMediumMemoryBlocks = maxMediumMemoryBlocks,
            LargeMemoryBlockSize = largeMemoryBlockSize,
            MaxLargeMemoryBlocks = maxLargeMemoryBlocks,
            ExtraLargeMemoryBlockSize = extraLargeMemoryBlockSize,
            MaxExtraLargeMemoryBlocks = maxExtraLargeMemoryBlocks,

            MaxMessageReceivedEventArgs = maxMessageReceivedEventArgs
        };
    }

    [Obsolete("Use ToClientObjectCacheSettings() in order to get the full set of settings.")]
    public ObjectCacheSettings ToObjectCacheSettings()
    {
        return ToClientObjectCacheSettings();
    }
}
