﻿using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Properties;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Subclasses.CommonBuilders;

namespace SolastaUnfinishedBusiness.Subclasses;

internal sealed class WizardArcaneFighter : AbstractSubclass
{
    internal WizardArcaneFighter()
    {
        var magicAffinityArcaneFighterConcentrationAdvantage = FeatureDefinitionMagicAffinityBuilder
            .Create("MagicAffinityArcaneFighterConcentrationAdvantage")
            .SetGuiPresentation(Category.Feature)
            .SetConcentrationModifiers(ConcentrationAffinity.Advantage)
            .AddToDB();

        // LEFT AS A POWER FOR BACKWARD COMPATIBILITY
        var powerArcaneFighterEnchantWeapon = FeatureDefinitionPowerBuilder
            .Create("PowerArcaneFighterEnchantWeapon")
            .SetGuiPresentation(Category.Feature)
            .SetUsesFixed(ActivationTime.Action)
            .SetCustomSubFeatures(
                PowerVisibilityModifier.Hidden,
                new CanUseAttribute(AttributeDefinitions.Intelligence, CanWeaponBeEnchanted))
            .AddToDB();

        var additionalActionArcaneFighter = FeatureDefinitionAdditionalActionBuilder
            .Create("AdditionalActionArcaneFighter")
            .SetGuiPresentation(Category.Feature)
            .SetActionType(ActionDefinitions.ActionType.Main)
            .SetRestrictedActions(ActionDefinitions.Id.CastMain)
            .SetTriggerCondition(AdditionalActionTriggerCondition.HasDownedAnEnemy)
            .AddToDB();

        var additionalDamageArcaneFighterBonusWeapon = FeatureDefinitionAdditionalDamageBuilder
            .Create("AdditionalDamageArcaneFighterBonusWeapon")
            .SetGuiPresentation(Category.Feature)
            .SetNotificationTag("ArcaneFighter")
            .SetFrequencyLimit(FeatureLimitedUsage.OncePerTurn)
            .SetDamageDice(DieType.D8, 1)
            .SetAdditionalDamageType(AdditionalDamageType.SameAsBaseDamage)
            .AddToDB();

        Subclass = CharacterSubclassDefinitionBuilder
            .Create("WizardArcaneFighter")
            .SetGuiPresentation(Category.Subclass,
                Sprites.GetSprite("WizardArcaneFighter", Resources.WizardArcaneFighter, 256))
            .AddFeaturesAtLevel(2,
                FeatureSetCasterFightingProficiency,
                magicAffinityArcaneFighterConcentrationAdvantage,
                powerArcaneFighterEnchantWeapon)
            .AddFeaturesAtLevel(6,
                AttributeModifierCasterFightingExtraAttack,
                AttackReplaceWithCantripCasterFighting)
            .AddFeaturesAtLevel(10,
                additionalActionArcaneFighter)
            .AddFeaturesAtLevel(14,
                additionalDamageArcaneFighterBonusWeapon)
            .AddToDB();
    }

    internal override CharacterSubclassDefinition Subclass { get; }

    internal override FeatureDefinitionSubclassChoice SubclassChoice =>
        FeatureDefinitionSubclassChoices.SubclassChoiceWizardArcaneTraditions;

    // ReSharper disable once UnassignedGetOnlyAutoProperty
    internal override DeityDefinition DeityDefinition { get; }
}
