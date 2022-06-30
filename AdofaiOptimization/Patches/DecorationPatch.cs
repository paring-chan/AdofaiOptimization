using HarmonyLib;

namespace AdofaiOptimization.Patches
{
    internal static class DecorationPatch
    {
        [HarmonyPatch(typeof(scrVisualDecoration), "UpdateHitbox")]
        private static class VisualDecorationUpdateHitbox
        {
            private static bool Prefix(scrVisualDecoration __instance) => __instance.canHitPlanets;
        }
    }
}