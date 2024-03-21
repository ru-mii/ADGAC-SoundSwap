using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using System.Reflection;
using System.IO;
using UnityEngine.Networking;
using System.Collections;
using static SoundSwap.SoundSwap.Toolkit;
using System.Globalization;

namespace SoundSwap
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class SoundSwap : BaseUnityPlugin
    {

        #region Variables

        public const string pluginGuid = "04a39ea8969445baa93b8fb46dd4fbd7";
        public const string pluginName = "SoundSwap";
        public const string pluginVersion = "1.3";

        public static string pluginPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public static BepInEx.Logging.ManualLogSource pLogger;
        Harmony harmony = new Harmony(pluginGuid);

        List<MethodInfo> original = new List<MethodInfo>();
        List<MethodInfo> patch = new List<MethodInfo>();

        public static Sound[] SoundManager_Copy;
        public static Sound[] PlayerSoundManager_Copy;
        public static Sound[] PlayerBodySoundManager_Copy;

        public static SoundManager soundManager_Instance;
        public static PlayerSoundManager playerSoundManager_Instance;
        public static PlayerBodySoundManager playerBodySoundManager_Instance;

        public static bool shouldReload = true;

        #endregion Variables

        // ---------------------------------------

        #region LocalFunctions

        private void Awake()
        {
            // static logger
            pLogger = Logger;

            // SoundManager -> PlaySound()
            original.Add(AccessTools.Method(typeof(SoundManager), "PlaySound"));
            patch.Add(AccessTools.Method(typeof(SoundManager_PlaySound), "Prefix"));

            // SoundManager -> Awake()
            original.Add(AccessTools.Method(typeof(SoundManager), "Awake"));
            patch.Add(AccessTools.Method(typeof(SoundManager_Awake), "Prefix"));

            // PlayerSoundManager -> Awake()
            original.Add(AccessTools.Method(typeof(PlayerSoundManager), "Awake"));
            patch.Add(AccessTools.Method(typeof(PlayerSoundManager_Awake), "Prefix"));

            // PlayerSoundManager -> Awake()
            original.Add(AccessTools.Method(typeof(PlayerBodySoundManager), "Awake"));
            patch.Add(AccessTools.Method(typeof(PlayerBodySoundManager_Awake), "Prefix"));

            // patch all
            for (int i = 0; i < original.Count; i++)
            {
                if (patch[i].Name == "Prefix") harmony.Patch(original[i], new HarmonyMethod(patch[i]));
                else if (patch[i].Name == "Postfix") harmony.Patch(original[i], null, new HarmonyMethod(patch[i]));
            }
        }

        private void Update()
        {
            if (shouldReload && SoundManager_Copy != null && PlayerSoundManager_Copy != null && PlayerBodySoundManager_Copy != null)
            {
                StartCoroutine(LoadAudioFiles());
                shouldReload = false;
            }

            if (Input.GetKey(KeyCode.M) && Input.GetKeyDown(KeyCode.F5)) shouldReload = true;
        }

        #endregion LocalFunctions

        // ---------------------------------------

        #region SoundManager

        public static class SoundManager_PlaySound
        {
            public static bool Prefix(SoundManager __instance, string name, float volume)
            {
                Sound sound = Array.Find(__instance.sounds, (Sound x) => x.name == name);
                if (sound == null) return false;

                if (!sound.source.gameObject.activeInHierarchy) sound.source.gameObject.SetActive(true);
                if (!sound.source.enabled) sound.source.enabled = true;

                sound.source.volume = (UnityEngine.Random.Range(-sound.randomVolume, sound.randomVolume) + 1f) * sound.volume;
                sound.source.pitch = (UnityEngine.Random.Range(-sound.randomPitch, sound.randomPitch) + 1f) * sound.pitch;
                sound.source.volume *= volume;
                sound.source.Play();

                return false;
            }
        }

        public static class SoundManager_Awake
        {
            public static bool Prefix(SoundManager __instance)
            {
                Sound[] copyVar = SoundManager_Copy;

                if (SoundManager_Copy == null)
                {
                    List<Sound> tSounds = new List<Sound>();
                    foreach (Sound sound in __instance.sounds)
                    {
                        Sound tSound = new Sound();
                        tSound.clip = sound.clip;
                        tSound.loop = sound.loop;
                        tSound.name = sound.name;
                        tSound.pitch = sound.pitch;
                        tSound.randomPitch = sound.randomPitch;
                        tSound.randomVolume = sound.randomVolume;
                        tSound.source = sound.source;
                        tSound.volume = sound.volume;
                        tSounds.Add(tSound);
                    }

                    SoundManager_Copy = tSounds.ToArray();
                }

                soundManager_Instance = __instance;
                SwapSounds(SoundManager_Copy, __instance);
                return false;
            }
        }

        #endregion SoundManager

        #region PlayerSoundManager

        public static class PlayerSoundManager_Awake
        {
            public static bool Prefix(PlayerSoundManager __instance)
            {
                Sound[] copyVar = PlayerSoundManager_Copy;

                if (PlayerSoundManager_Copy == null)
                {
                    List<Sound> tSounds = new List<Sound>();
                    foreach (Sound sound in __instance.sounds)
                    {
                        Sound tSound = new Sound();
                        tSound.clip = sound.clip;
                        tSound.loop = sound.loop;
                        tSound.name = sound.name;
                        tSound.pitch = sound.pitch;
                        tSound.randomPitch = sound.randomPitch;
                        tSound.randomVolume = sound.randomVolume;
                        tSound.source = sound.source;
                        tSound.volume = sound.volume;
                        tSounds.Add(tSound);
                    }

                    PlayerSoundManager_Copy = tSounds.ToArray();
                }

                playerSoundManager_Instance = __instance;
                SwapSounds(PlayerSoundManager_Copy, __instance);
                return false;
            }
        }

        #endregion PlayerSoundManager

        #region PlayerBodySoundManager

        public static class PlayerBodySoundManager_Awake
        {
            public static bool Prefix(PlayerBodySoundManager __instance)
            {
                Sound[] copyVar = PlayerBodySoundManager_Copy;

                if (PlayerBodySoundManager_Copy == null)
                {
                    List<Sound> tSounds = new List<Sound>();
                    foreach (Sound sound in __instance.sounds)
                    {
                        Sound tSound = new Sound();
                        tSound.clip = sound.clip;
                        tSound.loop = sound.loop;
                        tSound.name = sound.name;
                        tSound.pitch = sound.pitch;
                        tSound.randomPitch = sound.randomPitch;
                        tSound.randomVolume = sound.randomVolume;
                        tSound.source = sound.source;
                        tSound.volume = sound.volume;
                        tSounds.Add(tSound);
                    }

                    PlayerBodySoundManager_Copy = tSounds.ToArray();
                }

                playerBodySoundManager_Instance = __instance;
                SwapSounds(PlayerBodySoundManager_Copy, __instance);
                return false;
            }
        }

        #endregion PlayerBodySoundManager

        // ---------------------------------------

        #region InternalTools

        public static class Toolkit
        {
            public static void SwapSounds(Sound[] sounds, dynamic instance)
            {
                if (instance is SoundManager instance1)
                {
                    DoLoop(sounds, instance1.gameObject);
                    instance1.sounds = sounds;
                }

                else if (instance is PlayerSoundManager instance2)
                {
                    DoLoop(sounds, instance2.gameObject);
                    instance2.sounds = sounds;
                }

                else if (instance is PlayerBodySoundManager instance3)
                {
                    DoLoop(sounds, instance3.gameObject);
                    instance3.sounds = sounds;
                }

                void DoLoop(Sound[] sounds2, GameObject gObject)
                {
                    foreach (Sound sound in sounds2)
                    {
                        AudioSource newSource = gObject.AddComponent<AudioSource>();
                        newSource.clip = sound.clip;
                        newSource.volume = sound.volume;
                        newSource.pitch = sound.pitch;
                        newSource.loop = sound.loop;
                        newSource.playOnAwake = false;
                        newSource.enabled = true;
                        sound.source = newSource;
                        sound.source.playOnAwake = false;
                    }
                }
            }

            public static IEnumerator LoadAudioFiles()
            {
                string[] soundGroups = { "SoundManager", "PlayerSoundManager", "PlayerBodySoundManager" };

                foreach (string soundGroup in soundGroups)
                {
                    string soundsPath = Path.Combine(pluginPath, "Sounds", soundGroup);

                    if (Directory.Exists(soundsPath))
                    {
                        Sound[] toGoThrough = new Sound[0];
                        if (soundGroup == "SoundManager") toGoThrough = SoundManager_Copy;
                        else if (soundGroup == "PlayerSoundManager") toGoThrough = PlayerSoundManager_Copy;
                        else if (soundGroup == "PlayerBodySoundManager") toGoThrough = PlayerBodySoundManager_Copy;

                        foreach (Sound sound in toGoThrough)
                        {
                            string assumedAudioPath = Path.Combine(soundsPath, sound.name);

                            if (Directory.Exists(assumedAudioPath))
                            {
                                string[] filesPath = Directory.GetFiles(assumedAudioPath);
                                string audioPath = ""; string configPath = "";

                                foreach (string filePath in filesPath)
                                {
                                    if (Path.GetFileNameWithoutExtension(filePath).ToLower() == "config") configPath = filePath;
                                    else if (GetAudioType(filePath) != AudioType.UNKNOWN) audioPath = filePath;
                                }

                                // loading clip
                                if (audioPath != "")
                                {
                                    var downloadHandler = new DownloadHandlerAudioClip($"file://{audioPath}", GetAudioType(audioPath));
                                    downloadHandler.compressed = true;

                                    using (UnityWebRequest webRequest = new UnityWebRequest($"file://{audioPath}", "GET", downloadHandler, null))
                                    {
                                        yield return webRequest.SendWebRequest();
                                        if (webRequest.responseCode == 200)
                                        {
                                            sound.clip = downloadHandler.audioClip;
                                            pLogger.LogInfo("\"" + Path.GetFileName(assumedAudioPath) + "\"" + " loaded as " + "\"" + Path.GetFileNameWithoutExtension(audioPath) + "\"");
                                        }
                                        else pLogger.LogError("FILE COULDN'T BE ACCESSED: " + audioPath);
                                    }
                                }

                                // loading config
                                if (configPath != "")
                                {
                                    try
                                    {
                                        sound.volume = float.Parse(GetConfigData(configPath, "volume").Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture);
                                        sound.pitch = float.Parse(GetConfigData(configPath, "pitch").Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture);
                                        sound.loop = bool.Parse(GetConfigData(configPath, "loop"));
                                        sound.randomPitch = float.Parse(GetConfigData(configPath, "random pitch").Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture);
                                        sound.randomVolume = float.Parse(GetConfigData(configPath, "random volume").Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture);
                                    }
                                    catch
                                    {
                                        pLogger.LogError("COULDN'T PARSE ONE OF THE VALUES IN CONFIG AT: " + configPath);
                                        Application.Quit();
                                    }
                                }
                            }
                        }
                    }
                }

                SwapSounds(SoundManager_Copy, soundManager_Instance);
                SwapSounds(PlayerSoundManager_Copy, playerSoundManager_Instance);
                SwapSounds(PlayerBodySoundManager_Copy, playerBodySoundManager_Instance);
            }

            public static string GetConfigData(string path, string type)
            {
                if (File.Exists(path))
                {
                    string[] lines = File.ReadAllLines(path);

                    foreach (string line in lines)
                        if (line.StartsWith(type)) return line.Replace(type, "").Replace(":", " ").Replace(" ", "");
                }

                return "";
            }

            public static AudioType GetAudioType(string path)
            {
                string extension = Path.GetExtension(path).ToLowerInvariant();

                switch (extension)
                {
                    case ".ogg": return AudioType.OGGVORBIS;
                    case ".wav": return AudioType.WAV;
                    case ".mp3": return AudioType.MPEG;
                    case ".aiff": return AudioType.AIFF;
                    case ".aif": return AudioType.AIFF;
                    case ".xm": return AudioType.XM;
                    case ".mod": return AudioType.MOD;
                    case ".s3m": return AudioType.S3M;
                    case ".it": return AudioType.IT;
                    default:
                        {
                            pLogger.LogError("UNSUPPORTED AUDIO FORMAT IN: " + path);
                            return AudioType.UNKNOWN;
                        }
                }
            }
        }

        #endregion InternalTools
    }
}
