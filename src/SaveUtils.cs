using System.Collections.Generic;
using DiehardMasterDisaster.Fisobs;
using DiehardMasterDisaster.GunStuff;

namespace DiehardMasterDisaster;

public static class SaveUtils
{
    private const string SavedAmmoKey = "DMD_AMMO_DATA";
    private const string SavedGunsKey = "DMD_GUNS_DATA";

    public static void LoadAmmo(PlayerExtension.DiehardPlayer player)
    {
        if (player.SaveData != null && player.SaveData.TryGet(SavedAmmoKey, out Dictionary<string, int> ammoData))
        {
            foreach (var kvp in ammoData)
            {
                if (ExtEnumBase.TryParse(typeof(DiehardEnums.AmmoType), kvp.Key, true, out var extEnum))
                {
                    player.StoredAmmo[(DiehardEnums.AmmoType)extEnum] = kvp.Value;
                }
            }
        }

        //-- Making sure all keys are populated to avoid issues later
        foreach (var ammoType in ExtEnumBase.GetNames(typeof(DiehardEnums.AmmoType)))
        {
            var extEnum = new DiehardEnums.AmmoType(ammoType);
            if (!player.StoredAmmo.ContainsKey(extEnum))
            {
                player.StoredAmmo[extEnum] = 0;
            }
        }
    }

    public static void SaveAmmo(PlayerExtension.DiehardPlayer player)
    {
        var ammoData = new Dictionary<string, int>();
        foreach (var kvp in player.StoredAmmo)
        {
            ammoData[kvp.Key.value] = kvp.Value;
        }
        player.SaveData.Set(SavedAmmoKey, ammoData);
    }

    public static void LoadGuns(PlayerExtension.DiehardPlayer dPlayer)
    {
        if (dPlayer.SaveData == null || !dPlayer.SaveData.TryGet(SavedGunsKey, out List<string> gunsData) || !dPlayer.PlayerRef.TryGetTarget(out var player)) return;

        foreach (var gunData in gunsData)
        {
            var split = gunData.Split('|');
            var type = new AbstractPhysicalObject.AbstractObjectType(split[0]);
            var ammo = int.Parse(split[1]);
            
            var gun = new AbstractGun(player.room.world, type, new WorldCoordinate(player.room.abstractRoom.index, 0, 0, -1), player.room.game.GetNewID())
            {
                pos = player.abstractCreature.pos
            };

            gun.ID.number = ammo;
            dPlayer.StoredGuns.Add(gun);
        }
    }

    public static void SaveGuns(PlayerExtension.DiehardPlayer player)
    {
        var gunData = new List<string>();
        foreach (var gun in player.StoredGuns)
        {
            //-- Realized saved guns have to be removed from the world or they'll be duplicated on load
            if (gun.realizedObject is Gun realizedGun)
            {
                 player.StoreGun(realizedGun);
            }
            gunData.Add($"{gun.type.value}|{gun.ID.number}");
        }
        player.SaveData.Set(SavedGunsKey, gunData);
    }
}