using HarmonyLib;
using MelonLoader;
using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace XRPluginInjector
{
    public static class BuildInfo
    {
        public const string Name = "XRPluginInjector";
        public const string Author = "trev";
        public const string Company = null;
        public const string Version = "0.1.0";
        public const string DownloadLink = null;
    }

    public class Core : MelonPlugin
    {
        public static bool IsInjected { get; private set; }

        private bool shouldInject = true;

        public override void OnPreInitialization()
        {
            if (Environment.GetCommandLineArgs().Contains("-xr.disable"))
            {
                MelonLogger.Msg("XR injection is disabled.");
                shouldInject = false;
                IsInjected = shouldInject;
                return;
            }

            IsInjected = true;

            foreach (string res in Assembly.GetManifestResourceNames())
            {
                // This could probably be more dynamic but due to Unity dlls using periods I can't just do a simple Replace('.', '/')
                string fileName = res.Replace("XRPluginInjector.Resources.XRFiles.", "");
                if (fileName.StartsWith("Managed."))
                    CopyResourceToPath(res, Path.Combine(MelonUtils.GetGameDataDirectory(), "Managed", fileName.Replace("Managed.", "")));
                else if (fileName.StartsWith("Plugins."))
                    CopyResourceToPath(res, Path.Combine(MelonUtils.GetGameDataDirectory(), "Plugins", "x86_64", fileName.Replace("Plugins.x86_64.", "")));
                else if (fileName.StartsWith("UnitySubsystems."))
                    CopyResourceToPath(res, Path.Combine(MelonUtils.GetGameDataDirectory(), "UnitySubsystems", "UnityOpenXR", fileName.Replace("UnitySubsystems.UnityOpenXR.", "")));
            }
        }

        public override void OnApplicationLateStart()
        {
            if (!shouldInject)
                return;

            foreach (var xrManager in GetAssetBundle("xrmanager").LoadAllAssets())
                MelonLogger.Msg($"Loaded xrManager: {xrManager.name}");

            // Using AppDomain and Reflection to prevent any dependency warnings from MelonLoader
            var unityXrAsm = AppDomain.CurrentDomain.GetAssemblies().First(a => a.FullName.StartsWith("Unity.XR.Management"));
            unityXrAsm.GetType("UnityEngine.XR.Management.XRGeneralSettings").GetMethod("AttemptInitializeXRSDKOnLoad", AccessTools.all).Invoke(null, null);
            unityXrAsm.GetType("UnityEngine.XR.Management.XRGeneralSettings").GetMethod("AttemptStartXRSDKOnBeforeSplashScreen", AccessTools.all).Invoke(null, null);
        }

        public AssetBundle GetAssetBundle(string name)
        {
            MemoryStream memoryStream;
            using (Stream stream = Assembly.GetManifestResourceStream("XRPluginInjector.Resources." + name))
            {
                memoryStream = new MemoryStream((int)stream.Length);
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                memoryStream.Write(buffer, 0, buffer.Length);
            }
            return AssetBundle.LoadFromMemory(memoryStream.ToArray());
        }

        public void CopyResourceToPath(string resource, string path)
        {
            if (File.Exists(path))
            {
                MelonLogger.Msg("Resource " + resource + " already exists, skipping.");
                return;
            }

            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using (Stream stream = Assembly.GetManifestResourceStream(resource))
            {
                using (FileStream fileStream = new FileStream(path, FileMode.Create))
                {
                    MelonLogger.Msg("Copying " + resource + "...");
                    stream.CopyTo(fileStream);
                }
            }
        }
    }
}
