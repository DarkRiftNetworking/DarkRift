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

using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.Net;

namespace DarkRift.Client.Unity
{
    [CustomEditor(typeof(UnityClient))]
    [CanEditMultipleObjects]
    public class UnityClientEditor : Editor
    {
        private SerializedProperty host;
        private SerializedProperty port;
        private SerializedProperty connectOnStart;
        private SerializedProperty eventsFromDispatcher;
        private SerializedProperty sniffData;

        private SerializedProperty objectCacheSettings;

        private void OnEnable()
        {
            host                    = serializedObject.FindProperty("host");
            port                    = serializedObject.FindProperty("port");
            connectOnStart          = serializedObject.FindProperty("connectOnStart");
            eventsFromDispatcher    = serializedObject.FindProperty("eventsFromDispatcher");
            sniffData               = serializedObject.FindProperty("sniffData");

            objectCacheSettings     = serializedObject.FindProperty("objectCacheSettings");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            //Display IP address
            EditorGUILayout.PropertyField(host);
            EditorGUILayout.PropertyField(port);
            EditorGUILayout.PropertyField(connectOnStart);

            //Alert to changes when this is unticked!
            bool old = eventsFromDispatcher.boolValue;
            EditorGUILayout.PropertyField(eventsFromDispatcher);

            if (eventsFromDispatcher.boolValue != old && !eventsFromDispatcher.boolValue)
            {
                eventsFromDispatcher.boolValue = !EditorUtility.DisplayDialog(
                    "Danger!",
                    "Unchecking " + eventsFromDispatcher.displayName + " will cause DarkRift to fire events from the .NET thread pool, unless you are confident using multithreading with Unity you should not disable this. Are you 100% sure you want to proceed?",
                    "Yes",
                    "No (Save me!)"
                );
            }

            EditorGUILayout.PropertyField(sniffData);
            
            EditorGUILayout.PropertyField(objectCacheSettings, true);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
