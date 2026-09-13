using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FoundingBonds
{
    internal static class FoundingBondsUtility
    {
        public static bool IsProtectedSpousePair(Pawn? first, Pawn? second)
        {
            return first != null
                && second != null
                && first.relations.DirectRelationExists(PawnRelationDefOf.Spouse, second)
                && FoundingBondsGameComponent.Protects(first, second);
        }
    }

    [HarmonyPatch(typeof(InteractionWorker_Breakup), nameof(InteractionWorker_Breakup.RandomSelectionWeight))]
    internal static class BreakupSelectionPatch
    {
        private static void Postfix(Pawn initiator, Pawn recipient, ref float __result)
        {
            if (FoundingBondsUtility.IsProtectedSpousePair(initiator, recipient))
            {
                __result = 0f;
            }
        }
    }

    [HarmonyPatch(typeof(InteractionWorker_Breakup), nameof(InteractionWorker_Breakup.Interacted))]
    internal static class BreakupExecutionPatch
    {
        private static bool Prefix(
            Pawn initiator,
            Pawn recipient,
            ref string? letterText,
            ref string? letterLabel,
            ref LetterDef? letterDef,
            ref LookTargets? lookTargets)
        {
            if (!FoundingBondsUtility.IsProtectedSpousePair(initiator, recipient))
            {
                return true;
            }

            letterText = null;
            letterLabel = null;
            letterDef = null;
            lookTargets = null;
            return false;
        }
    }

    [HarmonyPatch]
    internal static class SocialRelationLabelPatch
    {
        private static readonly FieldInfo? OtherPawnField = AccessTools.Field(
            AccessTools.Inner(typeof(SocialCardUtility), "CachedSocialTabEntry"),
            "otherPawn");

        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(SocialCardUtility), "GetRelationsString");
        }

        private static void Postfix(object __0, Pawn __1, ref string __result)
        {
            Pawn? otherPawn = OtherPawnField?.GetValue(__0) as Pawn;
            if (!FoundingBondsUtility.IsProtectedSpousePair(__1, otherPawn))
            {
                return;
            }

            string suffix = "FoundingBonds_RelationSuffix".Translate();
            __result = __result.NullOrEmpty() ? suffix : __result + " " + suffix;
        }
    }
}
