using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony;
using Verse;

namespace Replimat
{
    public class MP : Attribute
    {
        //... dank
    }

    [StaticConstructorOnStartup]
    public class MultiplayerFixUtil
    {
        public static void Bootup()
        {
            var harmony = HarmonyInstance.Create("Dubwise.DubsMultiplayerPatches");

           var Multiplayer = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(assembly => assembly.GetName().Name == "Multiplayer");
            if (Multiplayer != null)
            {
                HarmonyMethod asshai = new HarmonyMethod(typeof(MultiplayerFixUtil), "asshai");
                MethodInfo qarth = AccessTools.Method("Multiplayer.Client.SyncHandlers:Init");
                harmony.Patch(qarth, asshai);
            }
        }

        public static void asshai()
        {
            var methheads = Assembly.GetExecutingAssembly().GetTypes()
                      .SelectMany(t => t.GetMethods(AccessTools.all))
                      .Where(m => m.GetCustomAttributes(typeof(MP), false).Length > 0)
                      .ToArray();

            foreach (var head in methheads)
            {
                AccessTools.Method("Multiplayer.Client.SyncMethod:Register").Invoke(null, new object[] { head.DeclaringType, head.Name, null });
            }

            Log.Message($"Patching Multiplayer for {Assembly.GetExecutingAssembly().GetName().Name}...OK");
        }
    }
}