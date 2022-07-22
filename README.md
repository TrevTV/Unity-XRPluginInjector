# Unity XR Plugin Injector [![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/S6S244CYE)

[![forthebadge](https://forthebadge.com/images/badges/works-on-my-machine.svg)](https://forthebadge.com)

 A MelonLoader plugin to enable OpenXR on games without it enabled.

## Usage Notices
- The XR assets included in this plugin were built with Unity 2020.2.6f1, if the bundle is unable to be read by your game you will need to [build your own XR AssetBundle](#building-a-xr-assetbundle).
- This plugin will not support every game, it depends on the Unity version, if the engine was modified, or even some random edge cases, though if an issue is made I may try to see if I can do anything.
- If you need to disable the plugin from enabling XR, add `-xr.disable` to your game's launch options.
- If you're game supports Single Pass Instancing and you want to use OpenVR, use the launch option `-xr.loadopenvr`.

## Installation
1. Follow the [Automated Installation guide](https://melonwiki.xyz/#/?id=automated-installation) on the MelonLoader wiki page, installing to the game's exe.
2. Download the latest release from [here](https://github.com/TrevTV/Unity-XRPluginInjector/releases/latest).
3. Put the DLL into `<GamePath>\Plugins`

## Compilation
1. Open the solution file
2. Replace the references to 0Harmony, MelonLoader, UnityEngine.AssetBundleModule, and UnityEngine.CoreModule with ones from the game you installed MelonLoader to.
3. Use Build > Build XRPluginInjector and copy the output DLL into your game's plugin folder.

## Building a XR AssetBundle
1. Create a new Unity project with the version you need the bundle for.
2. Enable XR Plugin Managment and install the OpenXR package
   - Some versions may require you to copy the package from a newer version of Unity and move it back to your project to fix some scripts.
3. Add all of the files in the Assets > XR folder to an AssetBundle
4. Build the AssetBundle and replace the one in this project with your new one and recompile it.

## Credits and Licensing
- [Unity OpenXR Plugin](https://docs.unity3d.com/Packages/com.unity.xr.openxr@1.4/manual/index.html) is licensed uder the Unity Companion License. See [Unity3D.com](https://unity3d.com/legal/licenses/Unity_Companion_License) for the full license.
- [Raicuparta](https://github.com/Raicuparta/) for telling me how to fix URP rendering issues, how to use OpenXR in some Unity versions that don't directly support it, and the code for generating the ScriptableObjects during runtime.
