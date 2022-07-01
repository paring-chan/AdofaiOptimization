using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace AdofaiOptimization.Patches
{
    internal class AudioPatch
    {
        [HarmonyPatch(typeof(AudioManager), "Update")]
        private static class UpdatePatch
        {
            private static bool Prefix(AudioManager __instance)
            {
                __instance.liveSources.RemoveAll(source =>
                {
                    if (!source) return true;

                    if (!source.clip || !source.isPlaying)
                    {
                        source.Stop();
                        source.clip = null;
                        source.loop = false;

                        source.gameObject.SetActive(false);
                        
                        _sources.Enqueue(source);

                        return true;
                    }

                    return false;
                });

                for (int index = 0; index < __instance.musicTracks.Count; ++index)
                {
                    if ((object)__instance.musicTracks[index] == null)
                        __instance.musicTracks.RemoveAt(index);
                    else if (!__instance.musicTracks[index].isPlaying)
                    {
                        Object.Destroy(__instance.musicTracks[index].gameObject, 3.5f);
                        __instance.tracksActive.RemoveAt(index);
                        __instance.musicTracks.RemoveAt(index);
                    }
                }

                for (int index = 0; index < __instance.musicTracks.Count; ++index)
                    __instance.CrossFadeAudioSource(__instance.musicTracks[index], __instance.tracksActive[index]);

                return false;
            }
        }

        private static Queue<AudioSource> _sources;

        private static AudioSource template;

        [HarmonyPatch(typeof(AudioManager), "Awake")]
        private static class AudioManagerAwake
        {
            private static void Postfix(AudioManager __instance, GameObject ___audioSourceContainer)
            {
                if (!template)
                {
                    template = __instance.audioSourcePrefab.GetComponent<AudioSource>();
                }

                _sources = new();
                
                for (int i = 0; i < 100; i++)
                {
                    var item = Object.Instantiate(template, ___audioSourceContainer.transform);
                    item.gameObject.SetActive(false);
                    _sources.Enqueue(item);
                }
            }
        }

        [HarmonyPatch(typeof(AudioManager), "StopAllSounds")]
        private static class StopAllSoundsPatch
        {
            private static bool Prefix(AudioManager __instance)
            {
                foreach (var source in __instance.liveSources)
                {
                    source.Stop();
                    source.clip = null;
                    source.loop = false;
                    _sources.Enqueue(source);
                }

                __instance.liveSources = new();

                return false;
            }
        }

        [HarmonyPatch(typeof(AudioManager), "MakeSource")]
        private static class MakeSourcePatch
        {
            private static bool Prefix(out AudioSource __result, AudioManager __instance, AudioClip customClip,
                string clipName, GameObject ___audioSourceContainer)
            {
                AudioSource source;

                if (_sources.Count == 0)
                {
                    source = Object.Instantiate(template, ___audioSourceContainer.transform);
                }
                else
                {
                    source = _sources.Dequeue();
                    source.gameObject.SetActive(true);
                }
                
                if ((object)customClip == null)
                {
                    customClip = __instance.FindOrLoadAudioClip(clipName);
                }

                source.clip = customClip;

                __instance.liveSources.Add(source);

                __result = source;

                return false;
            }
        }
    }
}