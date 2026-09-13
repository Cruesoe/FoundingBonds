using HarmonyLib;
using Verse;

namespace FoundingBonds
{
    public sealed class FoundingBondsMod : Mod
    {
        public FoundingBondsMod(ModContentPack content) : base(content)
        {
            new Harmony("cruesoe.foundingbonds").PatchAll();
        }
    }
}
