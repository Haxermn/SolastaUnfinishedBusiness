﻿using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using SolastaUnfinishedBusiness.Api;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Models;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.WeaponTypeDefinitions;

namespace SolastaUnfinishedBusiness.CustomValidators;

internal delegate bool IsCharacterValidHandler(RulesetCharacter character);

internal static class ValidatorsCharacter
{
    internal static readonly IsCharacterValidHandler HasAvailableBonusAction = character =>
    {
        var locationCharacter = GameLocationCharacter.GetFromActor(character);

        if (locationCharacter == null)
        {
            return false;
        }

        return locationCharacter.CurrentActionRankByType[ActionDefinitions.ActionType.Bonus] == 0;
    };

    internal static readonly IsCharacterValidHandler HasAttacked = character => character.ExecutedAttacks > 0;

    internal static readonly IsCharacterValidHandler HasNoArmor = character => !character.IsWearingArmor();

    internal static readonly IsCharacterValidHandler HasNoShield = character => !character.IsWearingShield();

    internal static readonly IsCharacterValidHandler HasShield = character => character.IsWearingShield();

    internal static readonly IsCharacterValidHandler HasLightArmor = character =>
        HasArmorCategory(character, EquipmentDefinitions.LightArmorCategory);

    internal static readonly IsCharacterValidHandler HasHeavyArmor = character =>
        HasArmorCategory(character, EquipmentDefinitions.HeavyArmorCategory);

    internal static readonly IsCharacterValidHandler DoesNotHaveHeavyArmor = character =>
        !HasArmorCategory(character, EquipmentDefinitions.HeavyArmorCategory);

    internal static readonly IsCharacterValidHandler HasLightSourceOffHand = character =>
        character is RulesetCharacterHero && character.GetOffhandWeapon()?.ItemDefinition.IsLightSourceItem == true;

    internal static readonly IsCharacterValidHandler HasFreeHand = character =>
        character.HasFreeHandSlot() &&
        !ValidatorsWeapon.HasAnyWeaponTag(character.GetMainWeapon(), TagsDefinitions.WeaponTagTwoHanded);

    internal static readonly IsCharacterValidHandler HasTwoHandedQuarterstaff = character =>
        ValidatorsWeapon.IsWeaponType(character.GetMainWeapon(), QuarterstaffType) && IsFreeOffhand(character);

    internal static readonly IsCharacterValidHandler HasLongbow = character =>
        ValidatorsWeapon.IsWeaponType(character.GetMainWeapon(), LongbowType);

    internal static readonly IsCharacterValidHandler HasTwoHandedRangedWeapon = character =>
        ValidatorsWeapon.IsWeaponType(character.GetMainWeapon(),
            LongbowType, ShortbowType, HeavyCrossbowType, LightCrossbowType);

    internal static readonly IsCharacterValidHandler HasTwoHandedVersatileWeapon = character =>
        ValidatorsWeapon.HasAnyWeaponTag(character.GetMainWeapon(), TagsDefinitions.WeaponTagVersatile) &&
        IsFreeOffhand(character);

    internal static readonly IsCharacterValidHandler HasMeleeWeaponInMainHand = character =>
        ValidatorsWeapon.IsMelee(character.GetMainWeapon());

    internal static readonly IsCharacterValidHandler HasMeleeWeaponInOffHand = character =>
        ValidatorsWeapon.IsMelee(character.GetOffhandWeapon());

    internal static readonly IsCharacterValidHandler HasMeleeWeaponInMainAndOffhand = character =>
        HasMeleeWeaponInMainHand(character) && HasMeleeWeaponInOffHand(character);

    internal static readonly IsCharacterValidHandler IsUnarmedInMainHand = character =>
        ValidatorsWeapon.IsUnarmed(character.GetMainWeapon()?.ItemDefinition, null);

    internal static readonly IsCharacterValidHandler IsNotInBrightLight = character =>
        HasAnyOfLightingStates(
            LocationDefinitions.LightingState.Darkness,
            LocationDefinitions.LightingState.Unlit,
            LocationDefinitions.LightingState.Dim)(character);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static IsCharacterValidHandler HasAnyOfConditions(params string[] conditions)
    {
        return character => conditions.Any(character.HasConditionOfType);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static IsCharacterValidHandler HasNoneOfConditions(params string[] conditions)
    {
        return character => !conditions.Any(character.HasConditionOfType);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IsCharacterValidHandler HasAnyOfLightingStates(
        params LocationDefinitions.LightingState[] lightingStates)
    {
        return character =>
        {
            var gameLocationCharacter = GameLocationCharacter.GetFromActor(character);

            return gameLocationCharacter != null && lightingStates.Contains(gameLocationCharacter.LightingState);
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static IsCharacterValidHandler HasMainHandWeaponType(params WeaponTypeDefinition[] weaponTypeDefinition)
    {
        return character => ValidatorsWeapon.IsWeaponType(character.GetMainWeapon(), weaponTypeDefinition);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static IsCharacterValidHandler HasOffhandWeaponType(params WeaponTypeDefinition[] weaponTypeDefinition)
    {
        return character => ValidatorsWeapon.IsWeaponType(character.GetOffhandWeapon(), weaponTypeDefinition);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static IsCharacterValidHandler HasWeaponType(params WeaponTypeDefinition[] weaponTypeDefinition)
    {
        return character =>
            ValidatorsWeapon.IsWeaponType(character.GetMainWeapon(), weaponTypeDefinition) ||
            ValidatorsWeapon.IsWeaponType(character.GetOffhandWeapon(), weaponTypeDefinition);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static IsCharacterValidHandler HasUsedWeaponType(WeaponTypeDefinition weaponTypeDefinition)
    {
        return character =>
        {
            var gameLocationCharacter = GameLocationCharacter.GetFromActor(character);

            return gameLocationCharacter != null &&
                   gameLocationCharacter.UsedSpecialFeatures.ContainsKey(weaponTypeDefinition.Name);
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void RegisterWeaponTypeUsed(
        GameLocationCharacter gameLocationCharacter,
        RulesetAttackMode attackMode)
    {
        if (attackMode?.SourceDefinition is not ItemDefinition itemDefinition)
        {
            return;
        }

        var type = itemDefinition.IsWeapon
            ? itemDefinition.WeaponDescription.WeaponType
            : itemDefinition.ArmorDescription.ArmorType;

        gameLocationCharacter.UsedSpecialFeatures.TryAdd(type, 0);
        gameLocationCharacter.UsedSpecialFeatures[type]++;
    }

    //
    // BOOL VALIDATORS
    //

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsMonkWeapon(this RulesetActor character, WeaponDescription weaponDescription)
    {
        var monkWeaponSpecializations = character.GetSubFeaturesByType<CharacterContext.MonkWeaponSpecialization>();

        return weaponDescription == null || weaponDescription.IsMonkWeaponOrUnarmed() ||
               monkWeaponSpecializations.Exists(x => x.WeaponType == weaponDescription.WeaponTypeDefinition);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsMonkWeapon(this RulesetCharacter character, ItemDefinition itemDefinition)
    {
        return itemDefinition != null && itemDefinition.IsWeapon &&
               character.IsMonkWeapon(itemDefinition.WeaponDescription);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsFreeOffhandVanilla(RulesetCharacter character)
    {
        var offHand = character.GetOffhandWeapon();

        // does character has free offhand in TA's terms as used in RefreshAttackModes for Monk bonus unarmed attack?
        return offHand == null || !offHand.ItemDefinition.IsWeapon;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsFreeOffhand(RulesetCharacter character)
    {
        return character.GetOffhandWeapon() == null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool HasConditionWithSubFeatureOfType<T>(this RulesetCharacter character) where T : class
    {
        return character.conditionsByCategory
            .Any(keyValuePair => keyValuePair.Value
                .Any(rulesetCondition => rulesetCondition.ConditionDefinition.HasSubFeatureOfType<T>()));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool HasArmorCategory(RulesetCharacter character, string category)
    {
        // required for wildshape scenarios
        if (character is not RulesetCharacterHero)
        {
            return false;
        }

        var equipedItem = character.CharacterInventory.InventorySlotsByName[EquipmentDefinitions.SlotTypeTorso]
            .EquipedItem;

        if (equipedItem == null || !equipedItem.ItemDefinition.IsArmor)
        {
            return false;
        }

        var armorDescription = equipedItem.ItemDefinition.ArmorDescription;
        var element = DatabaseHelper.GetDefinition<ArmorTypeDefinition>(armorDescription.ArmorType);

        return DatabaseHelper.GetDefinition<ArmorCategoryDefinition>(element.ArmorCategory)
            .IsPhysicalArmor && element.ArmorCategory == category;
    }
}
