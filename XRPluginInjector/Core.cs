using HarmonyLib;
using MelonLoader;
using System;
using System.IO;
using System.Linq;
using Unity.XR.OpenVR;
using UnityEngine;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;

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
        private bool useOpenVR = false;

        public override void OnPreInitialization()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Contains("-xr.disable"))
            {
                MelonLogger.Msg("XR injection is disabled.");
                shouldInject = false;
                IsInjected = shouldInject;
                return;
            }

            if (args.Contains("-xr.loadopenvr"))
            {
                useOpenVR = true;
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
                else if (fileName.StartsWith("UnitySubsystems.UnityOpenXR."))
                    CopyResourceToPath(res, Path.Combine(MelonUtils.GetGameDataDirectory(), "UnitySubsystems", "UnityOpenXR", fileName.Replace("UnitySubsystems.UnityOpenXR.", "")));
                else if (fileName.StartsWith("UnitySubsystems.XRSDKOpenVR."))
                    CopyResourceToPath(res, Path.Combine(MelonUtils.GetGameDataDirectory(), "UnitySubsystems", "XRSDKOpenVR", fileName.Replace("UnitySubsystems.XRSDKOpenVR.", "")));
                else if (fileName.StartsWith("StreamingAssets."))
                    CopyResourceToPath(res, Path.Combine(MelonUtils.GetGameDataDirectory(), "StreamingAssets", "SteamVR", fileName.Replace("StreamingAssets.SteamVR.", "")));
            }
        }

        public override void OnApplicationLateStart()
        {
            if (!shouldInject)
                return;

            var generalSettings = ScriptableObject.CreateInstance<XRGeneralSettings>();
            var managerSetings = ScriptableObject.CreateInstance<XRManagerSettings>();
            generalSettings.Manager = managerSetings;

            XRLoader loader = null;
            if (useOpenVR)
                loader = ScriptableObject.CreateInstance<OpenVRLoader>();
            else
                loader = ScriptableObject.CreateInstance<OpenXRLoader>();

            //typeof(XRManagerSettings).GetField("m_RegisteredLoaders", AccessTools.all).SetValue(managerSetings, new System.Collections.Generic.HashSet<XRLoader>() { loader });
            managerSetings.loaders.Clear();
            managerSetings.loaders.Add(loader);

            managerSetings.InitializeLoaderSync();

            typeof(XRGeneralSettings).GetMethod("AttemptInitializeXRSDKOnLoad", AccessTools.all).Invoke(null, null);
            typeof(XRGeneralSettings).GetMethod("AttemptStartXRSDKOnBeforeSplashScreen", AccessTools.all).Invoke(null, null);
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
