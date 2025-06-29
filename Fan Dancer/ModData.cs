using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Modding;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Display.Illustrations;

namespace Fan_Dancer
{
    public class ModData
    {
        public static class Traits
        {
            public static readonly Trait FanDancerArchetype = ModManager.RegisterTrait("FanDancer", new TraitProperties("Fan Dancer", true));
            public static readonly Trait FanDancer = ModManager.RegisterTrait("FanDancer", new TraitProperties("Fan Dancer", true));
            public static readonly Trait Fan = ModManager.RegisterTrait("Fan", new TraitProperties("Fan", true));
        }
        internal static class QEffects
        {
            internal static QEffectId FanMove { get; } = ModManager.RegisterEnumMember<QEffectId>("FanMove");
            internal static QEffectId FanSpeed { get; } = ModManager.RegisterEnumMember<QEffectId>("FanSpeed");
        }

        internal static class TileQEffects
        {
            internal static TileQEffectId GustTile { get; } = ModManager.RegisterEnumMember<TileQEffectId>("GustTile");
        }

        public static class FeatNames
        {
            public static readonly FeatName PerformIni = ModManager.RegisterFeatName("PerformInitiative", "Performance for Initiative");
            public static readonly FeatName PerceptionIni = ModManager.RegisterFeatName("PerceptionInitiative", "Perception for Initiative");
            public static readonly FeatName TwirlThrough = ModManager.RegisterFeatName("TwirlThrough", "Twirl Through");
        }

        internal static class Illustrations
        {
            internal static Illustration FanItem = new ModdedIllustration("FDAssets/Fan.png");
        }

        internal static class ActionIds
        {
            internal static ActionId TwirlStrike { get; } = ModManager.RegisterEnumMember<ActionId>("TwirlStrike");
        }
    }
}
