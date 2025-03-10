﻿using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Models;

namespace SolastaUnfinishedBusiness.Patches;

[UsedImplicitly]
public static class UserMerchantInventoryPatcher
{
    [HarmonyPatch(typeof(UserMerchantInventory), nameof(UserMerchantInventory.CreateMerchantDefinition))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class CreateMerchantDefinition_Patch
    {
        [UsedImplicitly]
        public static void Postfix(ref MerchantDefinition __result)
        {
            //PATCH: supports adding custom items to dungeon maker traders
            MerchantContext.TryAddItemsToUserMerchant(__result);
        }
    }
}
