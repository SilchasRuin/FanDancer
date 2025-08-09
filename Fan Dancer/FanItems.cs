using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Modding;

namespace Fan_Dancer;

public abstract class FanItems
{
    public static void AddFans()
    {
        ModManager.RegisterNewItemIntoTheShop("ImprovisedFan", itemName =>
        {
            Item item = new Item(itemName, ModData.Illustrations.FanItem, "Improvised Fan", 0, 0,
                    Trait.Club, Trait.Finesse, Trait.Simple, Trait.Agile, ModData.Traits.Fan)
                .WithWeaponProperties(new WeaponProperties("1d4", DamageKind.Bludgeoning))
                .WithAdditionalWeaponProperties(properties => { properties.ItemBonus = -2; })
                .WithDescription("You take a –2 item penalty to attack rolls with an improvised weapon.");
            return item;
        });
        ModManager.RegisterActionOnEachItem(item =>
            {
                if (ModManager.TryParse("RL_FightingFan", out ItemName itemName))
                    if(item.ItemName == itemName)
                        item.Traits.Add(ModData.Traits.Fan);
                return item;
            });
    }
}