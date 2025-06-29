using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Modding;

namespace Fan_Dancer
{
    public class ModLoader
    {
        [DawnsburyDaysModMainMethod]
        public static void LoadMod()
        {
            foreach (Feat feat in FanDancer.FanDancerFeats())
            {
                ModManager.AddFeat(feat);
            }
            FanItems.AddFans();
            ModManager.RegisterBooleanSettingsOption("TwirlThroughAndSoloDancerAsSkillFeats",
                "Fan Dancer: Twirl Through and Solo Dancer as general feats",
                "Enabling this option will allow you to select Twirl Through and Solo Dancer as general feats, or as skill feats, instead of as archetype feats." +
                "\n{b}NOTE{/b}: You must reload to use this option.",
                false);
        }
    }
}
