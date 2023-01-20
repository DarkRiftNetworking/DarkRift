#if UNITY_2017_3_OR_NEWER // PackageInfo
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
using System.IO;

namespace DarkRift.Unity.Editor
{
    [InitializeOnLoad]
    internal static class PackageUpdater
    {
#if UNITY_2020_2_OR_NEWER // Events
        private static PackageInfo CurrentPackageInfo => PackageInfo.FindForAssembly(typeof(PackageUpdater).Assembly);

        static PackageUpdater()
        {
            PackageInfo currentPackageInfo = CurrentPackageInfo;
            if (currentPackageInfo.source == PackageSource.Embedded || currentPackageInfo.source == PackageSource.Local)
            {
                Events.registeringPackages += OnRegisteringPackages;
            }
        }

#if UNITY_2020_2_OR_NEWER // Events + InitializeOnEnterPlayMode (2019.3+)
        [InitializeOnEnterPlayMode]
        private static void OnEnterPlaymodeInEditor(EnterPlayModeOptions options)
        {
            if (options.HasFlag(EnterPlayModeOptions.DisableDomainReload))
            {
                Events.registeredPackages -= OnRegisteringPackages;
            }
        }
#endif

        private static void OnRegisteringPackages(PackageRegistrationEventArgs args)
        {
            PackageInfo currentPackageInfo = CurrentPackageInfo;

            foreach (PackageInfo packageInfo in args.changedTo)
            {
                if (packageInfo.packageId == currentPackageInfo.packageId)
                {
                    if (packageInfo.version != currentPackageInfo.version)
                    {
                        UpdateAssemblyInfos(currentPackageInfo, packageInfo);
                    }
                    break;
                }
            }
        }
#endif

        private static void UpdateAssemblyInfos(PackageInfo packageInfoFrom, PackageInfo packageInfoTo)
        {
            string[] dirs = Directory.GetFiles(packageInfoTo.assetPath, "AssemblyInfo.gen.cs", SearchOption.AllDirectories);
            foreach (string dir in dirs)
            {
                string text = File.ReadAllText(dir);
                if (text.Contains(packageInfoFrom.version))
                {
                    File.WriteAllText(dir, text.Replace(packageInfoFrom.version, packageInfoTo.version));
                    Debug.Log($"Modified {dir} ({packageInfoFrom.version} -> {packageInfoTo.version})");
                    AssetDatabase.ImportAsset(dir);
                }
            }
        }
    }
}
#endif
