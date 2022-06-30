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
                        Object.Destroy(source.gameObject);

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

        [HarmonyPatch(typeof(AudioManager), "MakeSource")]
        private static class MakeSourcePatch
        {
            private static AudioSource template;

            private static bool Prefix(out AudioSource __result, AudioManager __instance, AudioClip customClip,
                string clipName, GameObject ___audioSourceContainer)
            {
                if (!template)
                {
                    template = __instance.audioSourcePrefab.GetComponent<AudioSource>();
                }

                var source = Object.Instantiate(template, ___audioSourceContainer.transform);

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