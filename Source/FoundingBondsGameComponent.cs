using System.Collections.Generic;
using RimWorld;
using Verse;

namespace FoundingBonds
{
    internal sealed class FoundingBondsGameComponent : GameComponent
    {
        private List<FoundingMarriage> foundingMarriages = new List<FoundingMarriage>();

        public FoundingBondsGameComponent(Game game)
        {
        }

        public static bool Protects(Pawn? first, Pawn? second)
        {
            if (first == null || second == null || first == second || Current.Game == null)
            {
                return false;
            }

            FoundingBondsGameComponent? component = Current.Game.GetComponent<FoundingBondsGameComponent>();
            return component != null && component.Contains(first, second);
        }

        public static bool TryRecord(Pawn? first, Pawn? second)
        {
            if (first == null || second == null || first == second || Current.Game == null)
            {
                return false;
            }

            FoundingBondsGameComponent? component = Current.Game.GetComponent<FoundingBondsGameComponent>();
            if (component == null)
            {
                return false;
            }

            if (!component.Contains(first, second))
            {
                component.foundingMarriages.Add(new FoundingMarriage(first, second));
            }

            return true;
        }

        public override void StartedNewGame()
        {
            base.StartedNewGame();
            foundingMarriages.Clear();

            List<Pawn>? startingPawns = Find.GameInitData?.startingAndOptionalPawns;
            if (startingPawns == null)
            {
                return;
            }

            for (int firstIndex = 0; firstIndex < startingPawns.Count; firstIndex++)
            {
                Pawn first = startingPawns[firstIndex];
                for (int secondIndex = firstIndex + 1; secondIndex < startingPawns.Count; secondIndex++)
                {
                    Pawn second = startingPawns[secondIndex];
                    if (first.relations.DirectRelationExists(PawnRelationDefOf.Spouse, second))
                    {
                        foundingMarriages.Add(new FoundingMarriage(first, second));
                    }
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref foundingMarriages, "foundingMarriages", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                foundingMarriages ??= new List<FoundingMarriage>();
                RemoveInvalidAndDuplicateRecords();
            }
        }

        private bool Contains(Pawn first, Pawn second)
        {
            for (int index = 0; index < foundingMarriages.Count; index++)
            {
                if (foundingMarriages[index].Matches(first, second))
                {
                    return true;
                }
            }

            return false;
        }

        private void RemoveInvalidAndDuplicateRecords()
        {
            for (int index = foundingMarriages.Count - 1; index >= 0; index--)
            {
                FoundingMarriage record = foundingMarriages[index];
                if (!record.IsValid)
                {
                    foundingMarriages.RemoveAt(index);
                    continue;
                }

                for (int earlierIndex = 0; earlierIndex < index; earlierIndex++)
                {
                    FoundingMarriage earlier = foundingMarriages[earlierIndex];
                    if (earlier.IsValid && RecordsMatch(earlier, record))
                    {
                        foundingMarriages.RemoveAt(index);
                        break;
                    }
                }
            }
        }

        private static bool RecordsMatch(FoundingMarriage first, FoundingMarriage second)
        {
            return first.SamePairAs(second);
        }
    }
}
