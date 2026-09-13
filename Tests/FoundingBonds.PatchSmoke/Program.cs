using System;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace FoundingBonds.PatchSmoke
{
    internal static class Program
    {
        private const string HarmonyId = "cruesoe.foundingbonds.patch-smoke";

        private static int Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.Error.WriteLine("Usage: PatchSmoke <RimWorld managed directory> <Harmony DLL> <FoundingBonds DLL>");
                return 2;
            }

            string managedDirectory = Path.GetFullPath(args[0]);
            AppDomain.CurrentDomain.AssemblyResolve += (_, eventArgs) =>
            {
                string? assemblyName = new AssemblyName(eventArgs.Name).Name;
                string candidate = Path.Combine(managedDirectory, assemblyName + ".dll");
                return File.Exists(candidate) ? Assembly.LoadFrom(candidate) : null;
            };

            Assembly.LoadFrom(Path.GetFullPath(args[1]));
            Assembly.LoadFrom(Path.Combine(managedDirectory, "Assembly-CSharp.dll"));
            Assembly modAssembly = Assembly.LoadFrom(Path.GetFullPath(args[2]));

            Harmony harmony = new Harmony(HarmonyId);
            try
            {
                harmony.PatchAll(modAssembly);
                AssertPatched("RimWorld.InteractionWorker_Breakup", "RandomSelectionWeight");
                AssertPatched("RimWorld.InteractionWorker_Breakup", "Interacted");
                AssertPatched("RimWorld.SocialCardUtility", "GetRelationsString");
                AssertRecoveryAction(modAssembly);
                Console.WriteLine("Harmony patch smoke test: passed");
                return 0;
            }
            finally
            {
                harmony.UnpatchAll(HarmonyId);
            }
        }

        private static void AssertPatched(string typeName, string methodName)
        {
            Type type = AccessTools.TypeByName(typeName)
                ?? throw new InvalidOperationException("Could not resolve " + typeName);
            MethodInfo method = AccessTools.Method(type, methodName)
                ?? throw new InvalidOperationException("Could not resolve " + typeName + "." + methodName);
            Patches patches = Harmony.GetPatchInfo(method)
                ?? throw new InvalidOperationException("No patches applied to " + typeName + "." + methodName);

            bool owned = patches.Owners.Contains(HarmonyId);
            if (!owned)
            {
                throw new InvalidOperationException("Smoke-test patch owner missing from " + typeName + "." + methodName);
            }
        }

        private static void AssertRecoveryAction(Assembly modAssembly)
        {
            Type type = modAssembly.GetType("FoundingBonds.FoundingBondsDebugActions")
                ?? throw new InvalidOperationException("Recovery action type is missing");
            MethodInfo method = AccessTools.Method(type, "RestoreFoundingMarriage")
                ?? throw new InvalidOperationException("Recovery action method is missing");

            bool hasDebugAction = method.GetCustomAttributes(inherit: false)
                .Any(attribute => attribute.GetType().FullName == "LudeonTK.DebugActionAttribute");
            if (!hasDebugAction)
            {
                throw new InvalidOperationException("Recovery action is not registered as a debug action");
            }
        }
    }
}
