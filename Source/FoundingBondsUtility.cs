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
}
