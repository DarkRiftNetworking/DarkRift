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
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Net;

namespace DarkRift.Server.Unity
{
    [CustomEditor(typeof(XmlUnityServer))]
    [CanEditMultipleObjects]
    public class XmlUnityClientEditor : Editor
    {
        private SerializedProperty configuration;
        private SerializedProperty createOnEnable;
        private SerializedProperty eventsFromDispatcher;

        private void OnEnable()
        {
            configuration = serializedObject.FindProperty("configuration");
            createOnEnable = serializedObject.FindProperty("createOnEnable");
            eventsFromDispatcher = serializedObject.FindProperty("eventsFromDispatcher");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(configuration);

            if (configuration.objectReferenceValue == null)
                EditorGUILayout.HelpBox("There is currently no configuration file assigned for the XmlUnityServer. The server will not be able to start!\n\nConsider adding the ExampleConfiguration.xml file here to get started.", MessageType.Warning);

            EditorGUILayout.PropertyField(createOnEnable);

            //Alert to changes when this is unticked!
            bool old = eventsFromDispatcher.boolValue;
            EditorGUILayout.PropertyField(eventsFromDispatcher);

            if (eventsFromDispatcher.boolValue != old && !eventsFromDispatcher.boolValue)
            {
                eventsFromDispatcher.boolValue = !EditorUtility.DisplayDialog(
                    "Danger!",
                    "Unchecking " + eventsFromDispatcher.displayName + " will cause DarkRift to fire events from the .NET thread pool. unless you are confident using multithreading with Unity you should not disable this.\n\nAre you sure you want to proceed?",
                    "Yes",
                    "No (Save me!)"
                );
            }

            EditorGUILayout.Separator();

            IEnumerable<Type> pluginTypes = UnityServerHelper.SearchForPlugins();

            if (pluginTypes.Count() > 0)
            {
                string pluginList = pluginTypes.Select(t => "\t\u2022 " + t.Name).Aggregate((a, b) => a + "\n" + b);

                EditorGUILayout.HelpBox("The following plugin types were found and will be loaded into the server:\n" + pluginList, MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("No plugins were found to load!", MessageType.Info);
            }

            EditorGUILayout.Separator();

            if (GUILayout.Button("Open Configuration"))
            {
                if (configuration != null)
                    AssetDatabase.OpenAsset(configuration.objectReferenceValue);
                else
                    Debug.LogError("No configuration file specified!");
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
