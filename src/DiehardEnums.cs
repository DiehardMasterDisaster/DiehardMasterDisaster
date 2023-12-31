﻿using System.Runtime.CompilerServices;

namespace DiehardMasterDisaster;

public static class DiehardEnums
{
    public static readonly SlugcatStats.Name DMD = new("diehardmasterdisaster");

    public static void Init()
    {
        RuntimeHelpers.RunClassConstructor(typeof(AmmoType).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(AbstractObject).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(Sound).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(Process).TypeHandle);
    }

    public static class AbstractObject
    {
        public static readonly AbstractPhysicalObject.AbstractObjectType DMDGrenadeLauncherGun = new(nameof(DMDGrenadeLauncherGun), true);
        public static readonly AbstractPhysicalObject.AbstractObjectType DMDAK47Gun = new(nameof(DMDAK47Gun), true);
        public static readonly AbstractPhysicalObject.AbstractObjectType DMDShotgun = new(nameof(DMDShotgun), true);
        public static readonly AbstractPhysicalObject.AbstractObjectType DMDBFGGun = new(nameof(DMDBFGGun), true);
        public static readonly AbstractPhysicalObject.AbstractObjectType DMDMinigun = new(nameof(DMDMinigun), true);
        public static readonly AbstractPhysicalObject.AbstractObjectType DMDDerringerGun = new(nameof(DMDDerringerGun), true);
    }

    public static class Sound
    {
        public static readonly SoundID DMDAK47Shoot = new(nameof(DMDAK47Shoot), true);
        public static readonly SoundID DMDScoreScreenBoom = new(nameof(DMDScoreScreenBoom), true);
    }

    public static class Process
    {
        public static readonly ProcessManager.ProcessID DMDScoreScreen = new(nameof(DMDScoreScreen), true);
    }

    public static class Song
    {
        public const string DMDScoreScreen = nameof(DMDScoreScreen);
    }
    
    public class AmmoType : ExtEnum<AmmoType>
    {
        public static readonly AmmoType Small = new(nameof(Small), true);
        public static readonly AmmoType Large = new(nameof(Large), true);
        public static readonly AmmoType Shells = new(nameof(Shells), true);
        public static readonly AmmoType Special = new(nameof(Special), true);
        
        public int AmmoStorage => value switch
        {
            nameof(Small) => DiehardOptions.MaxAmmoSmall.Value,
            nameof(Large) => DiehardOptions.MaxAmmoLarge.Value,
            nameof(Shells) => DiehardOptions.MaxAmmoShells.Value,
            nameof(Special) => DiehardOptions.MaxAmmoSpecial.Value,
            _ => 100
        };

        public AmmoType(string value, bool register = false) : base(value, register)
        {
        }
    }
}