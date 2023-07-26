using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DiehardMasterDisaster.Fisobs;
using DiehardMasterDisaster.Guns;
using DiehardMasterDisaster.GunStuff;
using Newtonsoft.Json;
using SlugBase.SaveData;

namespace DiehardMasterDisaster;

public static class PlayerExtension
{
    public class DiehardPlayer
    {
        public readonly bool IsDMD;
        public readonly SlugBaseSaveData SaveData;
        public readonly Dictionary<DiehardEnums.AmmoType, int> StoredAmmo = new();
        public readonly List<AbstractGun> AvailableGuns = new();
        public readonly WeakReference<Player> PlayerRef;

        public Gun EquippedGun;

        public DiehardPlayer(Player player)
        {
            IsDMD = player.SlugCatClass == DiehardEnums.DMD;
            PlayerRef = new WeakReference<Player>(player);

            if (player.room.game.session is StoryGameSession session)
            {
                SaveData = session.saveState.miscWorldSaveData.GetSlugBaseData();
            }
            
            SaveUtils.LoadGuns(this);
            SaveUtils.LoadAmmo(this);
        }

        public int TrySubtractAmmo(DiehardEnums.AmmoType ammoType, int amount)
        {
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

        public void AddAmmo(DiehardEnums.AmmoType ammoType, int amount) => StoredAmmo[ammoType] += amount;

        public bool HasAmmo(DiehardEnums.AmmoType ammoType) => StoredAmmo[ammoType] > 0;
    }

    private static readonly ConditionalWeakTable<Player, DiehardPlayer> _cwt = new ();

    public static DiehardPlayer GetDMD(this Player player) => _cwt.GetValue(player, _ => new(player));
}