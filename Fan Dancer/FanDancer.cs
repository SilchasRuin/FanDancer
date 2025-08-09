using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.TrueFeatDb;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.TrueFeatDb.Archetypes;
using Dawnsbury.Core.CharacterBuilder.Selections.Options;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.IO;
using Dawnsbury.Modding;
using Microsoft.Xna.Framework;

namespace Fan_Dancer;

public abstract class FanDancer
{
    public static IEnumerable<Feat> FanDancerFeats()
    {
        //Dedication feat
        Feat fanDancerDedication = ArchetypeFeats.CreateAgnosticArchetypeDedication(ModData.Traits.FanDancerArchetype,
            "You sweep across the battlefield, manifesting both the gentle spring breeze and the crisp autumn gusts.",
            "You become an expert in Performance. At 7th level you become a master in Performance, and at 15th level, you become legendary in Performance." +
            "\nWhenever you Feint while holding a fan, you give your Feint the air trait and may Stride 10 feet before or after as part of the same action.");
        CreateFanDancerLogic(fanDancerDedication);
        yield return fanDancerDedication;
        //Archetype feats
        TrueFeat soloDancer = new TrueFeat(ModManager.RegisterFeatName("SoloDancer", "Solo Dancer"),
            4, "You often dance alone with a grace exceeding that of most other performers, carrying yourself with a poise and confidence that draws the attention of those around you.", "You can always roll Performance for initiative, and during the first round of combat, creatures that act after you are off-guard to you.",[ModData.Traits.FanDancer]);
        if (PlayerProfile.Instance.IsBooleanOptionEnabled("TwirlThroughAndSoloDancerAsSkillFeats"))
        {
            soloDancer.Traits.Add(Trait.General);
            soloDancer.Traits.Add(Trait.Skill);
            soloDancer.WithPrerequisite(sheet => sheet.NumberOfFeatsForDedication.ContainsKey(ModData.Traits.FanDancerArchetype), "You must have the Fan Dancer dedication to take this feat.");
        }
        else soloDancer.WithAvailableAsArchetypeFeat(ModData.Traits.FanDancerArchetype);
        CreateSoloDancerLogic(soloDancer);
        yield return soloDancer;
        //Solo Dancer choice feats
        Feat performIni = new Feat(ModData.FeatNames.PerformIni,
            null, "You will roll Performance for initiative.", [], null);
        yield return performIni;
        Feat perceptionIni = new Feat(ModData.FeatNames.PerceptionIni,
            null, "You will roll Perception for initiative.", [], null);
        yield return perceptionIni;
        //Archetype feats continued
        TrueFeat twirlThrough = new TrueFeat(ModData.FeatNames.TwirlThrough,
            4, "You sweep across the battlefield in a fluttering of movement honed from years of coordinating perfectly spaced movements alongside fellow dancers.", 
            "When you attempt to Tumble Through an enemy's space, you can use Performance instead of Acrobatics.", [ModData.Traits.FanDancer]);
        if (PlayerProfile.Instance.IsBooleanOptionEnabled("TwirlThroughAndSoloDancerAsSkillFeats"))
        {
            twirlThrough.Traits.Add(Trait.General);
            twirlThrough.Traits.Add(Trait.Skill);
            twirlThrough.WithPrerequisite(sheet => sheet.NumberOfFeatsForDedication.ContainsKey(ModData.Traits.FanDancerArchetype), "You must have the Fan Dancer dedication to take this feat.");
        }
        else twirlThrough.WithAvailableAsArchetypeFeat(ModData.Traits.FanDancerArchetype);
        CreateTwirlThroughLogic(twirlThrough);
        yield return twirlThrough;
        TrueFeat sweepingFanBlock = new TrueFeat(ModManager.RegisterFeatName("SweepingFanBlock", "Sweeping Fan"),
            6,
            "You leap up on one leg, snapping your fans open alongside your head before sweeping them across your body.",
            "{b}Requirements{/b} You are wielding two fans and are the target of an attack using ammunition (such as arrows, bolts, sling bullets, and other objects of similar size)." +
            "\nYou whirl your fans to disrupt the incoming attack with gusts of air, gaining a +2 circumstance bonus to AC against the triggering attack.",
            [ModData.Traits.FanDancer, Trait.Air])
            .WithAvailableAsArchetypeFeat(ModData.Traits.FanDancerArchetype);
        CreateSweepingFanBlockLogic(sweepingFanBlock);
        yield return sweepingFanBlock;
        TrueFeat pushingWind = new TrueFeat(ModManager.RegisterFeatName("PushingWind", "Pushing Wind"),
            8,
            "As you spin and glide your fans alongside your allies, you kick up a mild wind that gently carries you all forward.",
            "So long as you’re holding a fan, you and allies who start their turn in a 30- foot aura emanating around you gain a +5-foot circumstance bonus to Speed for 1 round." +
            "\n\nAdditionally, the air impedes the movements of your foes. While holding a fan, the area in a 10-foot aura emanating around you is difficult terrain for all enemies.",
            [ModData.Traits.FanDancer, Trait.Air, Trait.Aura])
            .WithAvailableAsArchetypeFeat(ModData.Traits.FanDancerArchetype);
        CreatePushingWindLogic(pushingWind);
        yield return pushingWind;
        TrueFeat twirlingStrike = new TrueFeat(ModManager.RegisterFeatName("TwirlingStrike", "Twirling Strike"),
            8,
            "Your fans, one raised up alongside your head and the other alongside your hip, become a blur as you twirl across the battlefield.",
            "{b}Requirements{/b} You're wielding a fan and an enemy is close enough to tumble through.\nYou stride and may attempt to tumble through an enemy's space. If you successfully tumble through an enemy's space, you can make a melee Strike against that enemy with a fan you're wielding at any point during the movement. On a critical success, the enemy is off-guard against this attack.",
            [ModData.Traits.FanDancer])
            .WithAvailableAsArchetypeFeat(ModData.Traits.FanDancerArchetype); 
        CreateTwirlingStrikeLogic(twirlingStrike);
        yield return twirlingStrike;
    }
    //Logic functions
    private static void CreateFanDancerLogic(Feat fanDancer)
    {
        fanDancer.WithPrerequisite(values => values.HasFeat(FeatName.Performance), "You must be trained in Performance.")
                .WithOnSheet(values =>
                {
                    values.GrantFeat(FeatName.ExpertPerformance);
                    values.AddAtLevel(7, v7 => v7.GrantFeat(FeatName.MasterPerformance));
                    values.AddAtLevel(15, v7 => v7.GrantFeat(FeatName.LegendaryPerformance));
                })
                .WithPermanentQEffect("Whenever you Feint while holding a fan, you give your Feint the air trait and may Stride 10 feet before or after as part of the same action.",
                qf =>
                {
                    Creature self = qf.Owner;
                    if (self.HeldItems.Any(item => item.HasTrait(ModData.Traits.Fan)))
                    {
                        qf.YouBeginAction = async (_, action) =>
                        {
                            if (action.ActionId is ActionId.Feint)
                            {
                                action.Traits.Add(Trait.Air);
                                if (await self.Battle.AskForConfirmation(self, IllustrationName.FreeAction, "Stride 10 feet before you feint?", "Yes"))
                                {
                                    int speed = SetSpeed(self.Speed);
                                    self.AddQEffect(new QEffect
                                        {
                                            Id = ModData.QEffects.FanSpeed,
                                            BonusToAllSpeeds = (Func<QEffect, Bonus>)(_ =>
                                                new Bonus(2 - speed, BonusType.Untyped, "10 feet"))

                                        }
                                    );
                                    if (await self.StrideAsync("Stride 10 feet", allowCancel: true))
                                        self.AddQEffect(new QEffect { Id = ModData.QEffects.FanMove });
                                    if (self.FindQEffect(ModData.QEffects.FanSpeed) != null)
                                        self.FindQEffect(ModData.QEffects.FanSpeed)!.ExpiresAt = ExpirationCondition.Immediately;
                                }
                            }
                        };
                        qf.AfterYouTakeAction = async (_, action) =>
                        {
                            if (action.ActionId is ActionId.Feint && !self.HasEffect(ModData.QEffects.FanMove))
                            {
                                if (await self.Battle.AskForConfirmation(self, IllustrationName.FreeAction, "Stride 10 feet after you feint?", "Yes"))
                                {
                                    int speed = SetSpeed(self.Speed);
                                    self.AddQEffect(new QEffect
                                        {
                                        Id = ModData.QEffects.FanSpeed,
                                        BonusToAllSpeeds = (Func<QEffect, Bonus>)(_ => new Bonus(2 - speed, BonusType.Untyped, "10 feet"))
                                    }
                                        );
                                    await self.StrideAsync("Stride 10 feet", allowCancel: true);
                                    if (self.FindQEffect(ModData.QEffects.FanSpeed) != null)
                                        self.FindQEffect(ModData.QEffects.FanSpeed)!.ExpiresAt = ExpirationCondition.Immediately;
                                }
                            }
                            if (self.HasEffect(ModData.QEffects.FanMove) && action.ActionId is ActionId.Feint)
                            { 
                                QEffect? fanMove = self.FindQEffect(ModData.QEffects.FanMove);
                                    fanMove!.ExpiresAt = ExpirationCondition.EphemeralAtEndOfImmediateAction;
                            }
                        };
                    }
                });
    }
    private static void CreateSoloDancerLogic(TrueFeat dancer)
    {
        dancer.WithOnSheet(sheet =>
            {
                sheet.AddSelectionOption(new SingleFeatSelectionOption("SoloDancerInitiativeChoice",
                    "Solo Dancer Initiative", SelectionOption.PRECOMBAT_PREPARATIONS_LEVEL,
                    feat => feat.FeatName == ModData.FeatNames.PerformIni ||
                            feat.FeatName == ModData.FeatNames.PerceptionIni));
            })
            .WithOnCreature((self) =>
            {
                self.AddQEffect(Rogue.SurpriseAttackQEffect());
                self.AddQEffect(new QEffect("Solo Dancer",
                        "Can roll Performance as Initiative and creatures that act after you on the first round are off-guard to you.")
                    {
                        BonusToInitiative = (_) =>
                        {
                            if (self.HasFeat(ModData.FeatNames.PerformIni))
                            {
                                if (ModManager.TryParse("Virtuosic Performer", out FeatName virtuoso))
                                {
                                    if (self.HasFeat(virtuoso) && self.HasFeat(FeatName.IncredibleInitiative))
                                    {
                                        int performancePerception = self.Skills.Get(Skill.Performance) - self.Perception - (self.Proficiencies.Get(Trait.Performance) >= Proficiency.Master ? 2 : 1);
                                        if (performancePerception != 0)
                                            return new Bonus(performancePerception, BonusType.Untyped, "Solo Dancer"); 
                                    }
                                    else
                                    {
                                        int performanceMinusPerception = self.Skills.Get(Skill.Performance) - self.Perception;
                                        if (performanceMinusPerception != 0)
                                            return new Bonus(performanceMinusPerception, BonusType.Untyped, "Solo Dancer");
                                    }
                                }
                                else
                                {
                                    int performanceMinusPerception = self.Skills.Get(Skill.Performance) - self.Perception;
                                    if (performanceMinusPerception != 0)
                                        return new Bonus(performanceMinusPerception, BonusType.Untyped, "Solo Dancer");
                                }
                            }
                            return null;
                        }
                    }

                );
            });
    }
    private static void CreateTwirlThroughLogic(TrueFeat twirlThrough)
    {
        twirlThrough.WithOnCreature(cr =>
        {
            cr.AddQEffect(new QEffect()
                {
                    YouBeginAction = (_, action) =>
                    {
                        if (action.ActionId == ActionId.TumbleThrough)
                        {
                            action.WithActiveRollSpecification(new ActiveRollSpecification(TaggedChecks.SkillCheck(Skill.Performance), TaggedChecks.DefenseDC(Defense.Reflex)));
                            action.ActiveRollSpecification!.TaggedDetermineBonus.InvolvedSkill = Skill.Performance;
                        }
                        return Task.CompletedTask;
                    }
                }
            );
        });
    }
    private static void CreateSweepingFanBlockLogic(TrueFeat fanBlock)
    {
        fanBlock.WithActionCost(-2).WithPermanentQEffectAndSameRulesText(qf =>
            {
                qf.YouAreTargeted = async (qff, attack) =>
                {
                    Creature self = qff.Owner;
                    if (attack.HasTrait(Trait.Attack) 
                        && attack.HasTrait(Trait.Ranged) 
                        && attack.ProjectileKind == ProjectileKind.Arrow
                        && self.HeldItems.Count == 2
                        && self.HeldItems[0].HasTrait(ModData.Traits.Fan)
                        && self.HeldItems[1].HasTrait(ModData.Traits.Fan))
                    {
                        ActiveRollSpecification? rollSpecification = attack.ActiveRollSpecification;
                        int num8;
                        if (rollSpecification == null)
                        {
                            num8 = 0;
                        }
                        else
                        {
                            Defense? involvedDefense = rollSpecification.TaggedDetermineDC.InvolvedDefense;
                            Defense defense = Defense.AC;
                            num8 = involvedDefense.GetValueOrDefault() == defense & involvedDefense.HasValue ? 1 : 0;
                        }
                        if (num8 != 0)
                        {
                            if (!await self.Battle.AskToUseReaction(self,
                                    $"You're targeted by {attack.Owner.Name}'s {attack.Name}.\nUse Sweeping Fan Block to gain a +2 circumstance bonus to AC?"))
                                return;
                            self.AddQEffect(new QEffect()
                            {
                                ExpiresAt = ExpirationCondition.EphemeralAtEndOfImmediateAction,
                                BonusToDefenses =
                                    ((Func<QEffect, CombatAction, Defense, Bonus>)((_, _, defense) =>
                                        (defense != Defense.AC ? null : new Bonus(2, BonusType.Circumstance, "Sweeping Fan Block"))!))!
                            });
                        }
                    }
                };
            }
        );
    }
    private static void CreatePushingWindLogic(TrueFeat pushingWind)
    {
        pushingWind.WithOnCreature(self =>
        {
            if (!self.HeldItems.Any(item => item.HasTrait(ModData.Traits.Fan))) return;
            self.AnimationData.AddAuraAnimation(IllustrationName.KineticistAuraCircle, 6, Color.Blue);
            self.AnimationData.AddAuraAnimation(IllustrationName.KineticistAuraCircle, 2, Color.SkyBlue);
            QEffect terrain = new()
            {
                StartOfYourEveryTurn = (qf, innerSelf) =>
                {
                    qf.AddGrantingOfTechnical(cr => cr.FriendOf(innerSelf) && cr.DistanceTo(innerSelf) <= 6,
                        qfTech =>
                        {
                            qfTech.BonusToAllSpeeds = _ =>
                                new Bonus(1, BonusType.Circumstance, "Pushing Wind");
                            qfTech.ExpiresAt = ExpirationCondition.ExpiresAtStartOfSourcesTurn;
                        }
                    );
                    return Task.CompletedTask;
                }
            };
            terrain.AddGrantingOfTechnical(creature => creature.EnemyOf(self),
                qffTech =>
                {
                    qffTech.StartOfYourEveryTurn = (_, _) =>
                    {
                        foreach (Tile tile in self.Battle.Map.AllTiles.Where(tile => tile.DistanceTo(self.Occupies) <= 2))
                        {
                            tile.AddQEffect(new TileQEffect() 
                            {
                                TransformsTileIntoDifficultTerrain = true,
                                TileQEffectId = ModData.TileQEffects.GustTile
                            });
                        }

                        return Task.CompletedTask;
                    };
                    qffTech.EndOfYourTurnBeneficialEffect = (_, _) =>
                    {
                        foreach (Tile tile in self.Battle.Map.AllTiles.Where(tile => tile.HasEffect(ModData.TileQEffects.GustTile)))
                        {
                            tile.RemoveAllQEffects(tqf => tqf.TileQEffectId == ModData.TileQEffects.GustTile);
                        }

                        return Task.CompletedTask;
                    };
                });
            self.AddQEffect(terrain);
        });
    }
    private static void CreateTwirlingStrikeLogic(TrueFeat twirlingStrike)
    {
        twirlingStrike.WithActionCost(2)
            .WithPrerequisite(ModData.FeatNames.TwirlThrough, "Twirl Through")
            .WithPermanentQEffectAndSameRulesText(qfSelf =>
                {
                    qfSelf.ProvideMainAction = qfInner =>
                    {
                        Creature self = qfInner.Owner;
                        if (!self.HeldItems.Any(item => item.HasTrait(ModData.Traits.Fan)))
                            return null;
                        return new ActionPossibility(new CombatAction(self,
                                new SideBySideIllustration(ModData.Illustrations.FanItem,
                                    IllustrationName.BootsOfTumbling), "Twirling Strike", [Trait.Move],
                                "You stride and may attempt to tumble through an enemy's space. If you successfully tumble through an enemy's space, you can make a melee Strike against that enemy with a fan you're wielding at any point during the movement. On a critical success, the enemy is off-guard against this attack.",
                                Target.Self()
                                .WithAdditionalRestriction(cr =>
                                {
                                    return cr.Battle.AllCreatures.Where(enemy => enemy.EnemyOf(self)).Any(cr2 => self.DistanceTo(cr2) <= self.Speed - 2) ? null : "An enemy must be in range to tumble through.";
                                })
                            )
                            .WithActionId(ModData.ActionIds.TwirlStrike)
                            .WithActionCost(2)
                            .WithEffectOnChosenTargets(async (action, innerSelf, _) =>
                                {
                                    CombatAction? moveAction = Possibilities.Create(self)
                                        .Filter(ap =>
                                        {
                                            if (ap.CombatAction.ActionId != ActionId.StepByStepStride)
                                                return false;
                                            ap.CombatAction.ActionCost = 0;
                                            ap.RecalculateUsability();
                                            return true;
                                        }).CreateActions(true).FirstOrDefault(pw =>
                                            pw.Action.ActionId == ActionId.StepByStepStride) as CombatAction;
                                    QEffect strikeQf = new QEffect()
                                    {
                                        PreventTakingAction = strike =>
                                        {
                                            if (strike.HasTrait(Trait.Attack) && !strike.HasTrait(ModData.Traits.Fan))
                                                return "Must make strikes with a fan.";
                                            return null;
                                        },
                                        AfterYouTakeAction = async (effect, combatAction) =>
                                        {
                                            if (combatAction.ActionId == ActionId.TumbleThrough)
                                            {
                                                switch (combatAction.CheckResult)
                                                {
                                                    case CheckResult.Success:
                                                    case CheckResult.CriticalSuccess:
                                                        QEffect flatFoot = QEffect.FlatFooted("Twirling Strike");
                                                        if (combatAction.CheckResult == CheckResult.CriticalSuccess)
                                                            combatAction.ChosenTargets.ChosenCreature!.AddQEffect(flatFoot);
                                                        if (!await CommonCombatActions.StrikeAnyCreature(self,
                                                                cr => cr == combatAction.ChosenTargets.ChosenCreature,
                                                                true))
                                                            self.AddQEffect(new QEffect()
                                                                {
                                                                    Tag = combatAction.ChosenTargets.ChosenCreature,
                                                                    StateCheckWithVisibleChanges = async qff =>
                                                                    {
                                                                        if (qff.Tag is Creature enemy && self.IsAdjacentTo(enemy))
                                                                        {
                                                                            qff.ExpiresAt = ExpirationCondition.Ephemeral;
                                                                            await CommonCombatActions
                                                                                .StrikeAnyCreature(self,
                                                                                    cr => cr == enemy, true);
                                                                            flatFoot.ExpiresAt = ExpirationCondition.EphemeralAtEndOfImmediateAction;
                                                                        }
                                                                    },
                                                                    AfterYouTakeAction = (inner, tumble) =>
                                                                    {
                                                                        if (tumble.ActionId ==
                                                                            ModData.ActionIds.TwirlStrike)
                                                                            inner.ExpiresAt =
                                                                                ExpirationCondition.Immediately;
                                                                        effect.ExpiresAt =
                                                                            ExpirationCondition.Immediately;
                                                                        return Task.CompletedTask;
                                                                    }
                                                                }
                                                            );
                                                        else
                                                        {
                                                            effect.ExpiresAt = ExpirationCondition.Immediately;
                                                            flatFoot.ExpiresAt = ExpirationCondition.EphemeralAtEndOfImmediateAction;
                                                        }
                                                        break;
                                                    case CheckResult.CriticalFailure:
                                                    case CheckResult.Failure:
                                                        effect.ExpiresAt = ExpirationCondition.Immediately;
                                                        break;
                                                }
                                            }
                                        }
                                    };
                                    innerSelf.AddQEffect(strikeQf);
                                    if (moveAction != null)
                                    {
                                        if (!await innerSelf.Battle.GameLoop.FullCast(moveAction))
                                            action.RevertRequested = true;
                                    }
                                    strikeQf.ExpiresAt = ExpirationCondition.Immediately;
                                }

                            )
                        ).WithPossibilityGroup("Abilities");
                    };
                }
                );
    }
    //Utility functions
    private static int SetSpeed(int speed)
    {
        var ints = new List<int>();
        if (ints == null) throw new ArgumentNullException(nameof(ints));
        ints.Add(speed);
        return ints.FirstOrDefault();
    }
}