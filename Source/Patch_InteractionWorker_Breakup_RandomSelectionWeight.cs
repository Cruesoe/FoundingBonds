using HarmonyLib;
using RimWorld;
using Verse;

namespace FoundingBonds
{
    // Founding spouses never pick the breakup interaction.
    [HarmonyPatch(typeof(InteractionWorker_Breakup), nameof(InteractionWorker_Breakup.RandomSelectionWeight))]
    internal static class Patch_InteractionWorker_Breakup_RandomSelectionWeight
    {
        private static void Postfix(Pawn initiator, Pawn recipient, ref float __result)
        {
            if (FoundingBondsUtility.IsProtectedSpousePair(initiator, recipient))
            {
                __result = 0f;
            }
        }
    }
}
