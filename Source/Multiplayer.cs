using Harmony;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Verse;

namespace Replimat
{
    public class MP_Field : Attribute { }
    public class MP : Attribute
    {
        //... dank
    }

    [StaticConstructorOnStartup]
    public static class MP_Util
    {
        public static bool Active = false;
        public static void Bootup(HarmonyInstance harmony)
        {
            var asshai = new HarmonyMethod(typeof(MP_Util).GetMethod(nameof(MP_Util.asshai)));
            Type SyncHandlers = null;
            Type Sync = null;
            Type SyncMethod = null;
            Type SyncField = null;

            var mp = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(assembly => assembly.GetName().Name == "Multiplayer");

            if (mp == null) return;

            foreach (var type in mp.GetTypes())
            {
                if (type.Name == "SyncHandlers")
                {
                    SyncHandlers = type;
                }
                if (type.Name == "Sync")
                {
                    Sync = type;
                }
                if (type.Name == "SyncMethod")
                {
                    SyncMethod = type;
                }
                if (type.Name == "SyncField")
                {
                    SyncField = type;
                }
            }

            if (SyncHandlers == null) return;
            if (Sync == null) return;
            if (SyncMethod == null) return;
            if (SyncField == null) return;

            var qarth = AccessTools.Method(SyncHandlers, "Init");
            RegisterRef = AccessTools.Method(SyncMethod, "Register");
            FieldWatchPrefixRef = AccessTools.Method(Sync, "FieldWatchPrefix");
            FieldWatchPostfixRef = AccessTools.Method(Sync, "FieldWatchPostfix");
            WatchRef = AccessTools.Method(Sync, "Watch");
            FieldRef = AccessTools.Method(Sync, "Field", new Type[] { typeof(Type), typeof(string) });
            SetBufferChangesRef = AccessTools.Method(SyncField, "SetBufferChanges");
            harmony.Patch(qarth, asshai);
        }

        static MethodInfo RegisterRef;
        public static void Register(Type t, string n)
        {
            if (Active) RegisterRef.Invoke(null, new object[] { t, n, null });
        }

        static MethodInfo FieldWatchPrefixRef;
        public static void FieldWatchPrefix()
        {
            if (Active) FieldWatchPrefixRef.Invoke(null, null);
        }


        static MethodInfo FieldWatchPostfixRef;
        public static void FieldWatchPostfix()
        {
            if (Active) FieldWatchPostfixRef.Invoke(null, null);
        }


        static MethodInfo WatchRef;
        public static void Watch(string name, object target)
        {          
            var st = target.GetType().Name + name;
            if (Active) WatchRef.Invoke(null, new[] { SyncFields[st], target, null });
        }

        static MethodInfo FieldRef;
        static MethodInfo SetBufferChangesRef;
        public static Dictionary<string, object> SyncFields = new Dictionary<string, object>();
        public static void SyncField(Type field, string st)
        {
            if (!Active) return;
            var meth = FieldRef.Invoke(null, new[] { (object)field, st });
            SyncFields.Add(field.Name + st, SetBufferChangesRef.Invoke(meth, null));
        }

        public static void asshai()
        {
            Active = true;

            foreach (var method in Assembly.GetExecutingAssembly().GetTypes().SelectMany(t => t.GetMethods(AccessTools.all)))
            {
                if (method.GetCustomAttributes(typeof(MP), false).Length > 0)
                {
                    Register(method.DeclaringType, method.Name);
                }
            }

            foreach (var field in Assembly.GetExecutingAssembly().GetTypes().SelectMany(t => t.GetFields(AccessTools.all)))
            {
                if (field.GetCustomAttributes(typeof(MP_Field), false).Length > 0)
                {
                    SyncField(field.ReflectedType, field.Name);
                }
            }

            Log.Message($"Patching Multiplayer for {Assembly.GetExecutingAssembly().GetName()}...OK");
        }
    }
}