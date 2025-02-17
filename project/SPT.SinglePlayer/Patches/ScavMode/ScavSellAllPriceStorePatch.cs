﻿using SPT.Reflection.Patching;
using EFT.InventoryLogic;
using EFT.UI;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SPT.SinglePlayer.Patches.ScavMode
{
    /**
     * After fetching the list of items for the post-raid scav inventory screen, calculate
     * the total "Sell All" value, and store it for retrieval if the user hits "Sell All"
     */
    public class ScavSellAllPriceStorePatch : ModulePatch
    {
        private static string FENCE_ID = "579dc571d53a0658a154fbec";
        private static string ROUBLE_TID = "5449016a4bdc2d6f028b456f";

        private static FieldInfo _sessionField;

        public static int StoredPrice;

        protected override MethodBase GetTargetMethod()
        {
            Type scavInventoryScreenType = typeof(ScavengerInventoryScreen);
            _sessionField = AccessTools.GetDeclaredFields(scavInventoryScreenType).FirstOrDefault(f => f.FieldType == typeof(ISession));

            return AccessTools.FirstMethod(scavInventoryScreenType, IsTargetMethod);
        }

        private bool IsTargetMethod(MethodBase method)
        {
            // Look for a method with one parameter named `items`
            //   method_3(out IEnumerable<Item> items)
            if (method.GetParameters().Length == 1 && method.GetParameters()[0].Name == "items")
            {
                return true;
            }

            return false;
        }

        [PatchPostfix]
        public static void PatchPostfix(ScavengerInventoryScreen __instance, IEnumerable<Item> items)
        {
            var session = _sessionField.GetValue(__instance) as ISession;
            TraderClass traderClass = session.Traders.FirstOrDefault(x => x.Id == FENCE_ID);

            int totalPrice = 0;
            foreach (Item item in items)
            {
                if (item.TemplateId == ROUBLE_TID)
                {
                    totalPrice += item.StackObjectsCount;
                }
                else
                {
                    totalPrice += traderClass.GetItemPriceOnScavSell(item, true);
                }
            }

            StoredPrice = totalPrice;
        }
    }
}
