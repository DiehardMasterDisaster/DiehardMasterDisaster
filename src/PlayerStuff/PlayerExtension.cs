using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DiehardMasterDisaster.Fisobs;
using DiehardMasterDisaster.GunStuff;
using DiehardMasterDisaster.HUD;
using SlugBase.SaveData;

namespace DiehardMasterDisaster;

public static class PlayerExtension
{
    public class DiehardPlayer
    {
        public readonly bool IsDMD;
        public readonly SlugBaseSaveData SaveData;
        public readonly Dictionary<DiehardEnums.AmmoType, int> StoredAmmo = new();
        public readonly List<AbstractGun> StoredGuns = new();
        public readonly WeakReference<Player> PlayerRef;

        public WeaponsHUD HUD;

        public bool ActuallyEquipGun;

        public bool SwapWeaponNextKey;
        public bool SwapWeaponPrevKey;

        public DiehardPlayer(Player player)
        {
            IsDMD = player.SlugCatClass == DiehardEnums.DMD;
            PlayerRef = new WeakReference<Player>(player);

            if (player.room.game.session is StoryGameSession session)
            {
                SaveData = session.saveState.miscWorldSaveData.GetSlugBaseData();
            }
            
            DiehardMasterDisaster.SaveData.SaveUtils.LoadGuns(this);
            DiehardMasterDisaster.SaveData.SaveUtils.LoadAmmo(this);
        }

        public int TrySubtractAmmo(DiehardEnums.AmmoType ammoType, int amount)
        {
            HUD.Show();
            if (amount > StoredAmmo[ammoType])
            {
                var availableAmmo = StoredAmmo[ammoType];
                StoredAmmo[ammoType] = 0;
                return availableAmmo;
            }
            else
            {
                StoredAmmo[ammoType] -= amount;
                return amount;
            }
        }

        public bool AddAmmo(DiehardEnums.AmmoType ammoType, int amount)
        {
            if (StoredAmmo[ammoType] < ammoType.AmmoStorage)
            {
                HUD.Show();
                StoredAmmo[ammoType] = Math.Min(StoredAmmo[ammoType] + amount, ammoType.AmmoStorage);
                return true;
            }

            return false;
        }

        public bool HasAmmo(DiehardEnums.AmmoType ammoType) => StoredAmmo[ammoType] > 0;

        //-- TODO: Add sfx, maybe a small animation
        public void StoreGun(Gun gun)
        {
            if (!PlayerRef.TryGetTarget(out var player) || gun == null) return;

            foreach (var grasp in player.grasps)
            {
                if (grasp?.grabbed == gun)
                {
                    grasp.Release();
                }
            }

            var abstractGun = (AbstractGun)gun.abstractPhysicalObject;
            gun.RemoveFromRoom();
            abstractGun.Abstractize(player.abstractCreature.pos);
            abstractGun.Room.RemoveEntity(abstractGun);

            if (StoredGuns.Any(x => x.type == abstractGun.type))
            {
                if (!StoredGuns.Any(x => x == abstractGun))
                {
                    AddAmmo(abstractGun.AmmoType, abstractGun.ID.number);
                }

                abstractGun.Destroy();
            }
            else
            {
                StoredGuns.Add(abstractGun);
            }

            HUD.Show();
        }

        //-- TODO: Add sfx, maybe a small animation
        public void EquipGun(AbstractPhysicalObject.AbstractObjectType gunType)
        {
            if (!PlayerRef.TryGetTarget(out var player)) return;

            foreach (var gun in StoredGuns)
            {
                if (gun.type == gunType)
                {
                    if (gun.realizedObject == null)
                    {
                        gun.world = player.abstractCreature.world;
                        gun.pos = player.abstractCreature.pos;
                        gun.RealizeInRoom();
                        gun.realizedObject.bodyChunks[0].HardSetPosition(player.mainBodyChunk.pos);
                        ActuallyEquipGun = true;
                        try
                        {
                            player.SlugcatGrab(gun.realizedObject, player.FreeHand());
                        }
                        finally
                        {
                            ActuallyEquipGun = false;
                        }
                    }
                    break;
                }
            }
            
            HUD.Show();
        } 
    }

    private static readonly ConditionalWeakTable<Player, DiehardPlayer> _cwt = new ();

    public static bool IsDMD(this Player player, out DiehardPlayer result)
    {
        var dmd = player.GetDMD();
        if (dmd.IsDMD)
        {
            result = dmd;
            return true;
        }

        result = default;
        return false;
    }

    public static DiehardPlayer GetDMD(this Player player) => _cwt.GetValue(player, _ => new(player));

    public static bool IsDMD(this RainWorldGame game) => game.session is StoryGameSession session && session.saveStateNumber == DiehardEnums.DMD;
}