using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FoundingBonds
{
    // Adds the founding bond suffix to the relation label in the Social tab; runs for every row on every frame.
    [HarmonyPatch]
    internal static class Patch_SocialCardUtility_GetRelationsString
    {
        private static readonly AccessTools.FieldRef<object, Pawn>? OtherPawn = AccessTools.FieldRefAccess<Pawn>(
            AccessTools.Inner(typeof(SocialCardUtility), "CachedSocialTabEntry"),
            "otherPawn");

        private static string? suffix;

        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(SocialCardUtility), "GetRelationsString");
        }

        private static void Postfix(object __0, Pawn __1, ref string __result)
        {
            if (OtherPawn == null || !FoundingBondsUtility.IsProtectedSpousePair(__1, OtherPawn(__0)))
            {
                return;
            }

            suffix ??= "FoundingBonds_RelationSuffix".Translate();
            __result = __result.NullOrEmpty() ? suffix : __result + " " + suffix;
        }
    }
}
