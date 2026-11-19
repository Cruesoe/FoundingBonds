using HarmonyLib;
using RimWorld;
using Verse;

namespace FoundingBonds
{
    // Skips a breakup between founding spouses that is started some other way, with no letter.
    [HarmonyPatch(typeof(InteractionWorker_Breakup), nameof(InteractionWorker_Breakup.Interacted))]
    internal static class Patch_InteractionWorker_Breakup_Interacted
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
}
