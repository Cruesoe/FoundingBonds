using Verse;

namespace FoundingBonds
{
    internal sealed class FoundingMarriage : IExposable
    {
        private Pawn? first;
        private Pawn? second;

        public FoundingMarriage()
        {
        }

        public FoundingMarriage(Pawn first, Pawn second)
        {
            if (first.thingIDNumber <= second.thingIDNumber)
            {
                this.first = first;
                this.second = second;
            }
            else
            {
                this.first = second;
                this.second = first;
            }
        }

        public bool IsValid => first != null && second != null && first != second;

        public bool Matches(Pawn firstPawn, Pawn secondPawn)
        {
            return (first == firstPawn && second == secondPawn)
                || (first == secondPawn && second == firstPawn);
        }

        public bool SamePairAs(FoundingMarriage other)
        {
            return first != null && second != null && other.Matches(first, second);
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref first, "first");
            Scribe_References.Look(ref second, "second");
        }
    }
}
