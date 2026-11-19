using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace FoundingBonds
{
    internal static class FoundingMarriageRepair
    {
        // Relations cleared before a broken founding marriage is restored; none of their workers react to removal.
        private static readonly PawnRelationDef[] ClearedOnRestore =
        {
            PawnRelationDefOf.Spouse,
            PawnRelationDefOf.ExSpouse,
            PawnRelationDefOf.Fiance,
            PawnRelationDefOf.Lover,
            PawnRelationDefOf.ExLover,
        };

        // True when each pawn holds exactly one spouse entry for the other and neither is an ex-spouse.
        public static bool IsIntact(Pawn first, Pawn second)
        {
            return CountEntries(first, second, PawnRelationDefOf.Spouse) == 1
                && CountEntries(second, first, PawnRelationDefOf.Spouse) == 1
                && CountEntries(first, second, PawnRelationDefOf.ExSpouse) == 0
                && CountEntries(second, first, PawnRelationDefOf.ExSpouse) == 0;
        }

        // Drops extra spouse entries left by pawn editors, which make the game count one spouse as several.
        public static bool RemoveDuplicateSpouseEntries(Pawn first, Pawn second)
        {
            if (CountEntries(first, second, PawnRelationDefOf.Spouse) == 0
                || CountEntries(second, first, PawnRelationDefOf.Spouse) == 0)
            {
                return false;
            }

            bool removedFirst = RemoveExtraEntries(first, second, PawnRelationDefOf.Spouse);
            bool removedSecond = RemoveExtraEntries(second, first, PawnRelationDefOf.Spouse);
            if (removedFirst)
            {
                MarkThoughtsDirty(first);
            }

            if (removedSecond)
            {
                MarkThoughtsDirty(second);
            }

            return removedFirst || removedSecond;
        }

        // Clears every romantic entry between the pair on both sides, then adds one clean marriage.
        public static void Remarry(Pawn first, Pawn second)
        {
            RemoveEntries(first, second);
            RemoveEntries(second, first);
            first.relations.AddDirectRelation(PawnRelationDefOf.Spouse, second);
        }

        private static void MarkThoughtsDirty(Pawn pawn)
        {
            if (Current.ProgramState == ProgramState.Playing && !pawn.Dead)
            {
                pawn.needs?.mood?.thoughts.situational.Notify_SituationalThoughtsDirty();
            }
        }

        private static int CountEntries(Pawn pawn, Pawn other, PawnRelationDef def)
        {
            List<DirectPawnRelation> relations = pawn.relations.DirectRelations;
            int count = 0;
            for (int index = 0; index < relations.Count; index++)
            {
                if (relations[index].def == def && relations[index].otherPawn == other)
                {
                    count++;
                }
            }

            return count;
        }

        private static bool RemoveExtraEntries(Pawn pawn, Pawn other, PawnRelationDef def)
        {
            List<DirectPawnRelation> relations = pawn.relations.DirectRelations;
            bool kept = false;
            bool removed = false;
            for (int index = 0; index < relations.Count; index++)
            {
                if (relations[index].def != def || relations[index].otherPawn != other)
                {
                    continue;
                }

                if (!kept)
                {
                    kept = true;
                    continue;
                }

                relations.RemoveAt(index);
                index--;
                removed = true;
            }

            return removed;
        }

        private static void RemoveEntries(Pawn pawn, Pawn other)
        {
            List<DirectPawnRelation> relations = pawn.relations.DirectRelations;
            for (int index = relations.Count - 1; index >= 0; index--)
            {
                DirectPawnRelation relation = relations[index];
                if (relation.otherPawn == other && Array.IndexOf(ClearedOnRestore, relation.def) >= 0)
                {
                    relations.RemoveAt(index);
                }
            }
        }
    }
}
