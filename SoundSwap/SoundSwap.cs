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
using System.Runtime.InteropServices;

namespace SoundSwap
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class SoundSwap : BaseUnityPlugin
    {
        #region Variables

        public const string pluginGuid = "04a39ea8969445baa93b8fb46dd4fbd7";
        public const string pluginName = "SoundSwap";
        public const string pluginVersion = "5.0.0";

        public static string pluginPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public static BepInEx.Logging.ManualLogSource pLogger;
        Harmony harmony = new Harmony(pluginGuid);

        public static Sound[] SoundManager_Copy;
        public static Sound[] PlayerSoundManager_Copy;
        public static Sound[] PlayerBodySoundManager_Copy;

        public static SoundManager soundManager_Instance;
        public static PlayerSoundManager playerSoundManager_Instance;
        public static PlayerBodySoundManager playerBodySoundManager_Instance;

        public static string[] globalNames = { "ConfettiPop", "CreakLoop", "MetalSqueak01", "NewGameWater01",
                    "PlanksBreaking", "PulleyHit", "SqueakyWheel", "RopeStretch", "TwigSnap01",
                    "WaterSoundEnding", "WindFalling01" };

        public static AudioClip[] globalClips_Copy = new AudioClip[globalNames.Length];
        public static AudioClip[] globalClips = new AudioClip[globalNames.Length];

        public static bool shouldReload = true;
        public static bool shouldUnlockCursor = false;

        #endregion Variables

        #region Patching

        public static class Patches
        {
            public static List<MethodInfo> original = new List<MethodInfo>();
            public static List<MethodInfo> patch = new List<MethodInfo>();
            public static Harmony harmony = new Harmony(pluginGuid);

            public static void PatchAll()
            {
                original.Add(AccessTools.Method(typeof(SoundManager), "PlaySound"));
                patch.Add(AccessTools.Method(typeof(SoundManager_PlaySound), "Prefix"));

                original.Add(AccessTools.Method(typeof(SoundManager), "Awake"));
                patch.Add(AccessTools.Method(typeof(SoundManager_Awake), "Prefix"));

                original.Add(AccessTools.Method(typeof(PlayerSoundManager), "Awake"));
                patch.Add(AccessTools.Method(typeof(PlayerSoundManager_Awake), "Prefix"));

                original.Add(AccessTools.Method(typeof(PlayerBodySoundManager), "Awake"));
                patch.Add(AccessTools.Method(typeof(PlayerBodySoundManager_Awake), "Prefix"));

                original.Add(AccessTools.Method(typeof(ConfettiHandleScript), "ClaimGift"));
                patch.Add(AccessTools.Method(typeof(ConfettiHandleScript_ClaimGift), "Prefix"));

                original.Add(AccessTools.Method(typeof(TheScaleScript), "FixedUpdate"));
                patch.Add(AccessTools.Method(typeof(TheScaleScript_FixedUpdate), "Prefix"));

                original.Add(AccessTools.Method(typeof(HingeJointMotorTargetScript), "FixedUpdate"));
                patch.Add(AccessTools.Method(typeof(HingeJointMotorTargetScript_FixedUpdate), "Prefix"));

                original.Add(AccessTools.Method(typeof(PlayerSpawn), "NewGame"));
                patch.Add(AccessTools.Method(typeof(PlayerSpawn_NewGame), "Prefix"));

                original.Add(AccessTools.Method(typeof(SuspendedHingeJointScript), "FixedUpdate"));
                patch.Add(AccessTools.Method(typeof(SuspendedHingeJointScript_FixedUpdate), "Prefix"));

                original.Add(AccessTools.Method(typeof(PendulumConstructionScript), "FixedUpdate"));
                patch.Add(AccessTools.Method(typeof(PendulumConstructionScript_FixedUpdate), "Prefix"));

                original.Add(AccessTools.Method(typeof(LanternRopeScript), "FixedUpdate"));
                patch.Add(AccessTools.Method(typeof(LanternRopeScript_FixedUpdate), "Prefix"));

                original.Add(AccessTools.Method(typeof(SnappingHingeJointScript), "FixedUpdate"));
                patch.Add(AccessTools.Method(typeof(SnappingHingeJointScript_FixedUpdate), "Prefix"));

                original.Add(AccessTools.Method(typeof(PlaySoundTriggerScript), "OnTriggerEnter2D"));
                patch.Add(AccessTools.Method(typeof(PlaySoundTriggerScript_FixedUpdate), "Prefix"));

                original.Add(AccessTools.Method(typeof(CameraScript), "PlayWindSound"));
                patch.Add(AccessTools.Method(typeof(CameraScript_PlayWindSound), "Prefix"));

                // patch all
                for (int i = 0; i < original.Count; i++)
                {
                    if (patch[i].Name == "Prefix") harmony.Patch(original[i], new HarmonyMethod(patch[i]));
                    else if (patch[i].Name == "Postfix") harmony.Patch(original[i], null, new HarmonyMethod(patch[i]));
                }
            }
        }

        #endregion Patching

        // ---------------------------------------

        #region LocalFunctions

        private void Awake()
        {
            // static logger
            pLogger = Logger;

            Patches.PatchAll();

            // move bepinex console
            //IntPtr consoleWindowHandle = FindWindow(null, "BepInEx 5.4.22.0 - A Difficult Game About Climbing");
            //if (consoleWindowHandle != IntPtr.Zero) SetWindowPos(consoleWindowHandle, IntPtr.Zero, -1200, 350, 0, 0, 5);
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.M) && Input.GetKeyDown(KeyCode.F5)) shouldReload = true;

            if (shouldReload && SoundManager_Copy != null && PlayerSoundManager_Copy != null && PlayerBodySoundManager_Copy != null)
            {
                StartCoroutine(LoadAudioFiles());
                shouldReload = false;
            }

            if (shouldUnlockCursor)
            {
                Cursor.lockState = CursorLockMode.None;
                shouldUnlockCursor = false;
            }
        }

        #endregion LocalFunctions

        // ---------------------------------------

        #region CameraScript

        public static class CameraScript_PlayWindSound
        {
            public static bool Prefix(CameraScript __instance)
            {
                var windFallingSoundField = AccessTools.Field(__instance.GetType(), "windFallingSound");
                AudioSource windFallingSound = (AudioSource)windFallingSoundField.GetValue(__instance);

                if (windFallingSound != null && windFallingSound.clip.name == "WindFalling01")
                {
                    if (globalClips_Copy[10] == null) globalClips_Copy[10] = windFallingSound.clip;
                    if (globalClips[10] == null) windFallingSound.clip = globalClips_Copy[10];
                    else windFallingSound.clip = globalClips[10];
                }

                return true;
            }
        }

        #endregion CameraScript

        #region PlaySoundTriggerScript

        public static class PlaySoundTriggerScript_FixedUpdate
        {
            public static bool Prefix(PlaySoundTriggerScript __instance, Collider2D collision)
            {
                if (collision.name == "Climber_Hero_Body_Prefab")
                {
                    AudioSource audComp = __instance.gameObject.GetComponent<AudioSource>();

                    if (audComp != null && audComp.clip.name == "WaterSoundEnding")
                    {
                        if (globalClips_Copy[9] == null) globalClips_Copy[9] = audComp.clip;
                        if (globalClips[9] == null) audComp.clip = globalClips_Copy[9];
                        else audComp.clip = globalClips[9];
                    }
                }

                return true;
            }
        }

        #endregion PlaySoundTriggerScript

        #region SnappingHingeJointScript

        public static class SnappingHingeJointScript_FixedUpdate
        {
            public static bool Prefix(SnappingHingeJointScript __instance)
            {
                var audioSourceField = AccessTools.Field(__instance.GetType(), "audioSource");
                AudioSource audioSource = (AudioSource)audioSourceField.GetValue(__instance);

                if (audioSource != null && audioSource.clip.name == "TwigSnap01")
                {
                    if (globalClips_Copy[8] == null) globalClips_Copy[8] = audioSource.clip;
                    if (globalClips[8] == null) audioSource.clip = globalClips_Copy[8];
                    else audioSource.clip = globalClips[8];
                }

                return true;
            }
        }

        #endregion SnappingHingeJointScript

        #region LanternRopeScript

        public static class LanternRopeScript_FixedUpdate
        {
            public static bool Prefix(LanternRopeScript __instance)
            {
                var audioSourceField = AccessTools.Field(__instance.GetType(), "audioSource");
                AudioSource audioSource = (AudioSource)audioSourceField.GetValue(__instance);

                if (audioSource != null && audioSource.clip.name == "RopeStretch")
                {
                    if (globalClips_Copy[7] == null) globalClips_Copy[7] = audioSource.clip;
                    if (globalClips[7] == null) audioSource.clip = globalClips_Copy[7];
                    else audioSource.clip = globalClips[7];
                }

                return true;
            }
        }

        #endregion LanternRopeScript

        #region PendulumConstructionScript

        public static class PendulumConstructionScript_FixedUpdate
        {
            public static bool Prefix(PendulumConstructionScript __instance)
            {
                var audioStopField = AccessTools.Field(__instance.GetType(), "audioStop");
                AudioSource audioStop = (AudioSource)audioStopField.GetValue(__instance);

                var audioWheelField = AccessTools.Field(__instance.GetType(), "audioWheel");
                AudioSource audioWheel = (AudioSource)audioWheelField.GetValue(__instance);

                if (audioStop != null && audioStop.clip.name == "PulleyHit")
                {
                    if (globalClips_Copy[5] == null) globalClips_Copy[5] = audioStop.clip;
                    if (globalClips[5] == null) audioStop.clip = globalClips_Copy[5];
                    else audioStop.clip = globalClips[5];
                }

                if (audioWheel != null && audioWheel.clip.name == "SqueakyWheel")
                {
                    if (globalClips_Copy[6] == null) globalClips_Copy[6] = audioWheel.clip;
                    if (globalClips[6] == null) audioWheel.clip = globalClips_Copy[6];
                    else audioWheel.clip = globalClips[6];
                }

                return true;
            }
        }

        #endregion PendulumConstructionScript

        #region SuspendedHingeJointScript

        public static class SuspendedHingeJointScript_FixedUpdate
        {
            public static bool Prefix(SuspendedHingeJointScript __instance)
            {
                var breakSoundField = AccessTools.Field(__instance.GetType(), "breakSound");
                AudioSource breakSound = (AudioSource)breakSoundField.GetValue(__instance);

                if (breakSound != null && breakSound.clip.name == "PlanksBreaking")
                {
                    if (globalClips_Copy[4] == null) globalClips_Copy[4] = breakSound.clip;
                    if (globalClips[4] == null) breakSound.clip = globalClips_Copy[4];
                    else breakSound.clip = globalClips[4];
                }

                return true;
            }
        }

        #endregion SuspendedHingeJointScript

        #region PlayerSpawn

        public static class PlayerSpawn_NewGame
        {
            public static bool Prefix(PlayerSpawn __instance)
            {
                AudioSource audComp = __instance.gameObject.GetComponent<AudioSource>();

                if (audComp != null && audComp.clip.name == "NewGameWater01")
                {
                    if (globalClips_Copy[3] == null) globalClips_Copy[3] = audComp.clip;
                    if (globalClips[3] == null) audComp.clip = globalClips_Copy[3];
                    else audComp.clip = globalClips[3];
                }

                return true;
            }
        }

        #endregion PlayerSpawn

        #region HingeJointMotorTargetScript

        public static class HingeJointMotorTargetScript_FixedUpdate
        {
            public static bool Prefix(HingeJointMotorTargetScript __instance)
            {
                var audioSourceField = AccessTools.Field(__instance.GetType(), "audioSource");
                AudioSource audioSource = (AudioSource)audioSourceField.GetValue(__instance);

                if (audioSource != null && audioSource.clip.name == "MetalSqueak01")
                {
                    if (globalClips_Copy[2] == null) globalClips_Copy[2] = audioSource.clip;
                    if (globalClips[2] == null) audioSource.clip = globalClips_Copy[2];
                    else audioSource.clip = globalClips[2];
                }

                return true;
            }
        }

        #endregion HingeJointMotorTargetScript

        #region TheScaleScript

        public static class TheScaleScript_FixedUpdate
        {
            public static bool Prefix(TheScaleScript __instance)
            {
                var audioSourceField = AccessTools.Field(__instance.GetType(), "audioSource");
                AudioSource audioSource = (AudioSource)audioSourceField.GetValue(__instance);

                if (audioSource != null && audioSource.clip.name == "CreakLoop")
                {
                    if (globalClips_Copy[1] == null) globalClips_Copy[1] = audioSource.clip;
                    if (globalClips[1] == null) audioSource.clip = globalClips_Copy[1];
                    else audioSource.clip = globalClips[1];
                }

                return true;
            }
        }

        #endregion TheScaleScript

        #region ConfettiHandleScript

        public static class ConfettiHandleScript_ClaimGift
        {
            public static bool Prefix(ConfettiHandleScript __instance)
            {
                AudioSource audComp = __instance.gameObject.GetComponent<AudioSource>();

                if (audComp != null && audComp.clip.name == "ConfettiPop")
                {
                    if (globalClips_Copy[0] == null) globalClips_Copy[0] = audComp.clip;
                    if (globalClips[0] == null) audComp.clip = globalClips_Copy[0];
                    else audComp.clip = globalClips[0];
                }

                return true;
            }
        }

        #endregion ConfettiHandleScript

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
                                            pLogger.LogInfo(Path.GetFileName(audioPath)  + " loaded");
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

                string soundsGlobalPath = Path.Combine(pluginPath, "Sounds", "Global");

                if (Directory.Exists(soundsGlobalPath))
                {
                    for (int i = 0; i < globalNames.Length; i++)
                    {
                        string assumedAudioPath = Path.Combine(soundsGlobalPath, globalNames[i]);

                        if (Directory.Exists(assumedAudioPath))
                        {
                            string[] filesPath = Directory.GetFiles(assumedAudioPath);
                            string audioPath = ""; string configPath = "";

                            foreach (string filePath in filesPath)
                            {
                                if (Path.GetFileNameWithoutExtension(filePath).ToLower() == "config") configPath = filePath;
                                else if (GetAudioType(filePath) != AudioType.UNKNOWN) audioPath = filePath;
                            }

                            if (audioPath != "")
                            {
                                var downloadHandler = new DownloadHandlerAudioClip($"file://{audioPath}", GetAudioType(audioPath));
                                downloadHandler.compressed = true;

                                using (UnityWebRequest webRequest = new UnityWebRequest($"file://{audioPath}", "GET", downloadHandler, null))
                                {
                                    yield return webRequest.SendWebRequest();
                                    if (webRequest.responseCode == 200)
                                    {
                                        globalClips[i] = downloadHandler.audioClip;
                                        pLogger.LogInfo(Path.GetFileName(audioPath) + " loaded");
                                    }
                                    else pLogger.LogError("FILE COULDN'T BE ACCESSED: " + audioPath);
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

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        #endregion InternalTools
    }
}
