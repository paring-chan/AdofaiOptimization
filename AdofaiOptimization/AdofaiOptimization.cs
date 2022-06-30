#if ML
using MelonLoader;
#endif
#if UMM
using System.Reflection;
using UnityModManagerNet;
#endif

namespace AdofaiOptimization
{
    #if ML
    public class AdofaiOptimization : MelonMod {}
    #endif
    
    #if UMM
    // ReSharper disable once UnusedType.Global
    internal static class AdofaiOptimization
    {
        private static HarmonyLib.Harmony _harmony;
        
        private static void Load(UnityModManager.ModEntry entry)
        {
            _harmony = new HarmonyLib.Harmony(entry.Info.Id);
            
            entry.OnToggle += (modEntry, b) =>
            {
                if (b)
                {
                    _harmony.PatchAll(Assembly.GetExecutingAssembly());
                }
                else
                {
                    _harmony.UnpatchAll(_harmony.Id);
                }

                return true;
            };
        }
    }
    #endif
}