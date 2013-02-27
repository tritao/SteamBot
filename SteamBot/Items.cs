using Steam.TF2;
using SteamKit2.GC;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SteamBot
{
    partial class Bot
    {
        #region Metal Crafting
        void GetMetalsFromInventory(int numAssets, List<TF2BackpackItem> metals)
        {
            foreach (var asset in Backpack.Items)
            {
                metals.Clear();

                if (!IsMetalItem(asset))
                    continue;

                metals.Add(asset);

                for (int i = 0; i < numAssets - 1; ++i)
                {
                    var item = GetMetalFromInventory(asset.DefIndex, metals);
                    if (item == null) continue;

                    metals.Add(item);
                }

                if (metals.Count == numAssets)
                    return;
            }
        }

        bool IsMetalItem(TF2BackpackItem asset)
        {
            if (!ShouldCraftAsset(asset))
                return false;

            var item = GetItemFromDefIndex(asset.DefIndex);
            if (item == null) return false;

            if (!ShouldCraftItem(item, TF2CraftClass.CraftBar))
                return false;

            if (item.DefIndex == 5000) // Scrap Metal
                return true;

            if (item.DefIndex == 5001) // Reclaimed Metal
                return true;

            return false;
        }

        TF2BackpackItem GetMetalFromInventory(int defIndex, List<TF2BackpackItem> metals)
        {
            foreach (var asset in Backpack.Items)
            {
                if (!IsMetalItem(asset))
                    continue;

                if (metals.Contains(asset))
                    continue;

                var item = GetItemFromDefIndex(asset.DefIndex);

                if (item.DefIndex != defIndex)
                    continue;

                return asset;
            }

            return null;
        }
        #endregion

        #region Weapon Crafting
        void GetWeaponsFromInventory(
            int numWeapons, TF2Class @class, List<TF2BackpackItem> weapons)
        {
            foreach (var asset in Backpack.Items)
            {
                weapons.Clear();

                if (!IsWeaponItem(asset, @class))
                    continue;

                weapons.Add(asset);

                for (int i = 0; i < numWeapons - 1; ++i)
                {
                    var item = GetWeaponFromInventory(@class, weapons);
                    if (item == null) continue;

                    weapons.Add(item);
                }

                if (weapons.Count == numWeapons)
                    return;
            }
        }

        TF2BackpackItem GetWeaponFromInventory(TF2Class @class, List<TF2BackpackItem> weapons)
        {
            foreach (var asset in Backpack.Items)
            {
                if (!IsWeaponItem(asset, @class))
                    continue;

                if (weapons.Contains(asset))
                    continue;

                var item = GetItemFromDefIndex(asset.DefIndex);

                if (!IsSameClassesAssets(item, weapons))
                    continue;

                return asset;
            }

            return null;
        }

        bool IsWeaponItem(TF2BackpackItem asset, TF2Class @class)
        {
            if (!ShouldCraftAsset(asset))
                return false;

            var item = GetItemFromDefIndex(asset.DefIndex);
            if (item == null) return false;

            if (!ShouldCraftItem(item, TF2CraftClass.Weapon))
                return false;

            if ((@class != TF2Class.Any) && !item.UsedByClasses.Contains(@class))
                return false;

            return true;
        }
        #endregion

        #region Crafting Helpers
        void CraftItems(List<ulong> items)
        {
            var craftMsg = new ClientGCMsg<CMsgCraft>();
            craftMsg.Body.Blueprint = 0xFF;
            craftMsg.Body.ItemCount = (ushort)items.Count;
            craftMsg.Body.Items = new ulong[items.Count];

            for (int i = 0; i < items.Count; ++i)
                craftMsg.Body.Items[i] = items[i];

            Logger.WriteLine("Crafting {0} items", craftMsg.Body.ItemCount);
            SteamGC.Send(craftMsg, TF2App);
        }

        bool IsSameClassesAssets(TF2ItemSchema item, List<TF2BackpackItem> assets)
        {
            foreach (var asset in assets)
            {
                var item2 = GetItemFromDefIndex(asset.DefIndex);
                if (!IsSameClassesItem(item, item2))
                    return false;
            }

            return true;
        }

        bool IsSameClassesItem(TF2ItemSchema item1, TF2ItemSchema item2)
        {
            foreach (var @class in item1.UsedByClasses)
                if (!item2.UsedByClasses.Contains(@class))
                    return false;

            return true;
        }

        bool ShouldCraftAsset(TF2BackpackItem asset)
        {
            if (asset.CannotTrade || asset.CannotCraft)
                return false;

            if (asset.CustomName != null)
                return false;

            if (asset.CustomDescription != null)
                return false;

            return true;
        }

        bool ShouldCraftItem(TF2ItemSchema item, TF2CraftClass craftClass)
        {
            if (item.CraftClass != craftClass)
                return false;

            if (item.Quality != TF2ItemQuality.Unique)
                return false;

            if (item.Slot == TF2WeaponSlot.Misc)
                return false;

            if (item.Slot == TF2WeaponSlot.Action)
                return false;

            if (item.Slot == TF2WeaponSlot.Head)
                return false;

            return true;
        }
        #endregion
    }
}
