﻿using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.CustomValidators;
using SolastaUnfinishedBusiness.Models;
using SolastaUnfinishedBusiness.Properties;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.CharacterRaceDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionSenses;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionMoveModes;
using static FeatureDefinitionAttributeModifier;

namespace SolastaUnfinishedBusiness.Races;

internal static class FairyRaceBuilder
{
    private const string Name = "Fairy";
    internal static CharacterRaceDefinition RaceFairy { get; } = BuildFairy();

    private static bool IsFlightValid(RulesetCharacter character)
    {
        return !character.IsWearingMediumArmor() && !character.IsWearingHeavyArmor();
    }

    internal static void OnItemEquipped([NotNull] RulesetCharacter character)
    {
        if (IsFlightValid(character))
        {
            return;
        }

        var rulesetCondition = character.AllConditions.FirstOrDefault(x =>
            x.ConditionDefinition == ConditionDefinitions.ConditionFlyingAdaptive);

        if (rulesetCondition != null)
        {
            character.RemoveCondition(rulesetCondition);
        }
    }

    [NotNull]
    private static CharacterRaceDefinition BuildFairy()
    {
        // Spells

        var castSpellFairy = FeatureDefinitionCastSpellBuilder
            .Create($"CastSpell{Name}")
            .SetGuiPresentation(Category.Feature)
            .SetSpellCastingOrigin(FeatureDefinitionCastSpell.CastingOrigin.Race)
            .SetSpellCastingAbility(AttributeDefinitions.Charisma)
            .SetSpellKnowledge(SpellKnowledge.FixedList)
            .SetSpellReadyness(SpellReadyness.AllKnown)
            .SetSlotsRecharge(RechargeRate.LongRest)
            .SetSlotsPerLevel(SharedSpellsContext.RaceCastingSlots)
            .SetKnownCantrips(1, 1, FeatureDefinitionCastSpellBuilder.CasterProgression.Flat)
            .SetSpellList(SpellListDefinitionBuilder
                .Create($"SpellList{Name}")
                .SetGuiPresentationNoContent(true)
                .ClearSpells()
                .SetSpellsAtLevel(0, SpellsContext.BurstOfRadiance)
                .SetSpellsAtLevel(1, SpellDefinitions.FaerieFire)
                .SetSpellsAtLevel(2, SpellsContext.ColorBurst)
                .FinalizeSpells()
                .AddToDB())
            .AddToDB();

        // Languages

        var proficiencyFairyLanguages = FeatureDefinitionProficiencyBuilder
            .Create($"Proficiency{Name}Languages")
            .SetGuiPresentation(Category.Feature)
            .SetProficiencies(ProficiencyType.Language, "Language_Common", "Language_Elvish")
            .AddToDB();

        // Ability Scores

        var attributeModifierAbilityScore = FeatureDefinitionAttributeModifierBuilder
            .Create($"AttributeModifier{Name}AbilityScore")
            .SetGuiPresentationNoContent(true)
            .SetModifier(AttributeModifierOperation.Additive, AttributeDefinitions.Dexterity, 2)
            .AddToDB();

        var pointPoolAbilityScore = FeatureDefinitionPointPoolBuilder
            .Create($"PointPool{Name}AbilityScore")
            .SetGuiPresentationNoContent(true)
            .SetPool(HeroDefinitions.PointsPoolType.AbilityScore, 1)
            .RestrictChoices(
                AttributeDefinitions.Intelligence,
                AttributeDefinitions.Wisdom,
                AttributeDefinitions.Charisma)
            .AddToDB();

        var featureSetFairyAbilityScoreIncrease = FeatureDefinitionFeatureSetBuilder
            .Create($"FeatureSet{Name}AbilityScoreIncrease")
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(attributeModifierAbilityScore, pointPoolAbilityScore)
            .AddToDB();

        // Flight

        var powerAngelicFormSprout = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}FlightSprout")
            .SetGuiPresentation(Category.Feature,
                Sprites.GetSprite("FlightSprout", Resources.PowerAngelicFormSprout, 256, 128))
            .SetUsesFixed(ActivationTime.Action)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetDurationData(DurationType.Permanent)
                    .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetConditionForm(
                                ConditionDefinitions.ConditionFlyingAdaptive,
                                ConditionForm.ConditionOperation.Add)
                            .Build())
                    .Build())
            .SetCustomSubFeatures(new ValidatorsPowerUse(IsFlightValid,
                ValidatorsCharacter.HasNoneOfConditions(ConditionFlyingAdaptive)))
            .AddToDB();

        var powerAngelicFormDismiss = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}FlightDismiss")
            .SetGuiPresentation(Category.Feature,
                Sprites.GetSprite("FlightDismiss", Resources.PowerAngelicFormDismiss, 256, 128))
            .SetUsesFixed(ActivationTime.BonusAction)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetDurationData(DurationType.Instantaneous)
                    .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetConditionForm(
                                ConditionDefinitions.ConditionFlyingAdaptive,
                                ConditionForm.ConditionOperation.Remove)
                            .Build())
                    .Build())
            .SetCustomSubFeatures(
                new ValidatorsPowerUse(ValidatorsCharacter.HasAnyOfConditions(ConditionFlyingAdaptive)))
            .AddToDB();

        var featureSetFairyFlight = FeatureDefinitionFeatureSetBuilder
            .Create($"FeatureSet{Name}Flight")
            .SetGuiPresentation($"Power{Name}FlightSprout", Category.Feature)
            .AddFeatureSet(powerAngelicFormSprout, powerAngelicFormDismiss)
            .AddToDB();

        var raceFairy = CharacterRaceDefinitionBuilder
            .Create(Elf, "RaceFairy")
            .SetGuiPresentation(Category.Race, Sprites.GetSprite(Name, Resources.Fairy, 1024, 512))
            .SetSizeDefinition(CharacterSizeDefinitions.Small)
            .SetBaseWeight(35)
            .SetBaseHeight(1)
            .SetMinimalAge(6)
            .SetMaximalAge(120)
            .SetFeaturesAtLevel(1,
                castSpellFairy,
                proficiencyFairyLanguages,
                featureSetFairyAbilityScoreIncrease,
                featureSetFairyFlight,
                MoveModeMove6,
                SenseNormalVision,
                SenseDarkvision)
            .AddToDB();

        raceFairy.subRaces = new List<CharacterRaceDefinition>();
        RacesContext.RaceScaleMap[raceFairy] = 6f / 9.4f;

        return raceFairy;
    }
}
