using System.Runtime.CompilerServices;

namespace DiehardMasterDisaster;

public static class DiehardEnums
{
    public static readonly SlugcatStats.Name DMD = new("diehardmasterdisaster");

    public static void Init()
    {
        RuntimeHelpers.RunClassConstructor(typeof(AmmoType).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(AbstractObject).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(Sound).TypeHandle);
    }

    public static class AbstractObject
    {
        public static readonly AbstractPhysicalObject.AbstractObjectType DMDGrenadeLauncherGun = new(nameof(DMDGrenadeLauncherGun), true);
        public static readonly AbstractPhysicalObject.AbstractObjectType DMDAK47Gun = new(nameof(DMDAK47Gun), true);
        public static readonly AbstractPhysicalObject.AbstractObjectType DMDShotgun = new(nameof(DMDShotgun), true);
        public static readonly AbstractPhysicalObject.AbstractObjectType DMDBFGGun = new(nameof(DMDBFGGun), true);
        public static readonly AbstractPhysicalObject.AbstractObjectType DMDMinigun = new(nameof(DMDMinigun), true);
        public static readonly AbstractPhysicalObject.AbstractObjectType DMDDerringerGun = new(nameof(DMDDerringerGun), true);
        public static readonly AbstractPhysicalObject.AbstractObjectType DMDPipe = new(nameof(DMDPipe), true);
        public static readonly AbstractPhysicalObject.AbstractObjectType DMDBFGOrb = new(nameof(DMDBFGOrb), true);
    }

    public static class Sound
    {
        public static readonly SoundID DMDAK47Shoot = new(nameof(DMDAK47Shoot), true);
    }
    
    public class AmmoType : ExtEnum<AmmoType>
    {
        public static readonly AmmoType Small = new(nameof(Small), true);
        public static readonly AmmoType Large = new(nameof(Large), true);
        public static readonly AmmoType Shells = new(nameof(Shells), true);
        public static readonly AmmoType Special = new(nameof(Special), true);

        public AmmoType(string value, bool register = false) : base(value, register)
        {
        }
    }
}