using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using RimWorld;
using Verse;

namespace FoundingBonds
{
    internal static class FoundingBondsDebugActions
    {
        [DebugAction(
            "Founding Bonds",
            "Restore founding marriage",
            actionType = DebugActionType.ToolMapForPawns,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void RestoreFoundingMarriage(Pawn pawn)
        {
            List<Pawn> candidates = pawn.relations.DirectRelations
                .Where(relation => relation.def == PawnRelationDefOf.Spouse
                    || relation.def == PawnRelationDefOf.ExSpouse)
                .Select(relation => relation.otherPawn)
                .Where(other => other != null
                    && !other.Dead
                    && !other.Destroyed
                    && !FoundingBondsGameComponent.Protects(pawn, other))
                .Distinct()
                .OrderBy(other => other.LabelShort)
                .ToList();

            if (candidates.Count == 0)
            {
                Messages.Message(
                    "FoundingBonds_NoRecoveryCandidate".Translate(pawn.Named("PAWN")),
                    pawn,
                    MessageTypeDefOf.RejectInput,
                    historical: false);
                return;
            }

            List<FloatMenuOption> options = new List<FloatMenuOption>();
            foreach (Pawn candidate in candidates)
            {
                Pawn partner = candidate;
                bool alreadyMarried = pawn.relations.DirectRelationExists(PawnRelationDefOf.Spouse, partner);
                string label = alreadyMarried
                    ? "FoundingBonds_ProtectMarriageOption".Translate(partner.Named("PAWN"))
                    : "FoundingBonds_RestoreMarriageOption".Translate(partner.Named("PAWN"));

                options.Add(new FloatMenuOption(label, delegate
                {
                    RestoreAndProtect(pawn, partner);
                }));
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }

        private static void RestoreAndProtect(Pawn first, Pawn second)
        {
            bool restored = !first.relations.DirectRelationExists(PawnRelationDefOf.Spouse, second);
            if (restored)
            {
                first.relations.TryRemoveDirectRelation(PawnRelationDefOf.ExSpouse, second);
                first.relations.TryRemoveDirectRelation(PawnRelationDefOf.Fiance, second);
                first.relations.TryRemoveDirectRelation(PawnRelationDefOf.Lover, second);
                first.relations.AddDirectRelation(PawnRelationDefOf.Spouse, second);
            }

            if (!FoundingBondsGameComponent.TryRecord(first, second))
            {
                Messages.Message(
                    "FoundingBonds_RecoveryFailed".Translate(),
                    first,
                    MessageTypeDefOf.RejectInput,
                    historical: false);
                return;
            }

            ClearDivorceMemory(first, second);
            ClearDivorceMemory(second, first);

            Messages.Message(
                (restored ? "FoundingBonds_MarriageRestored" : "FoundingBonds_MarriageProtected")
                    .Translate(first.Named("PAWN1"), second.Named("PAWN2")),
                new LookTargets(first, second),
                MessageTypeDefOf.PositiveEvent,
                historical: false);
            SocialCardUtility.ClearCaches();
        }

        private static void ClearDivorceMemory(Pawn pawn, Pawn formerSpouse)
        {
            if (pawn.needs.mood != null)
            {
                pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(
                    ThoughtDefOf.DivorcedMe,
                    formerSpouse);
            }
        }
    }
}
