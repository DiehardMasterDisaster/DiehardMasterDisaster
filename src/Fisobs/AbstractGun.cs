using DiehardMasterDisaster.Guns;

namespace DiehardMasterDisaster.Fisobs;

public class AbstractGun : AbstractPhysicalObject
{
    //-- TODO: Could be moved into its own saved value, kinda hacky but is what the original code did and I didn't bother to change
    public int CurrentAmmo
    {
        get => ID.number;
        set => ID.number = value;
    } 
    
    //-- TODO: I don't really like those... might be worth it to add another level abstraction in the form of a IGun interface containing the properties and methods of Gun instead of having classes extending it
    public DiehardEnums.AmmoType AmmoType => type.value switch
    {
        nameof(DiehardEnums.AbstractObject.DMDGrenadeLauncherGun) => DiehardEnums.AmmoType.Special,
        nameof(DiehardEnums.AbstractObject.DMDAK47Gun) => DiehardEnums.AmmoType.Large,
        nameof(DiehardEnums.AbstractObject.DMDShotgun) => DiehardEnums.AmmoType.Shells,
        nameof(DiehardEnums.AbstractObject.DMDBFGGun) => DiehardEnums.AmmoType.Special,
        nameof(DiehardEnums.AbstractObject.DMDMinigun) => DiehardEnums.AmmoType.Large,
        nameof(DiehardEnums.AbstractObject.DMDDerringerGun) => DiehardEnums.AmmoType.Small,
        _ => DiehardEnums.AmmoType.Small
    };

    public int ClipSize => type.value switch
    {
        nameof(DiehardEnums.AbstractObject.DMDGrenadeLauncherGun) => 4,
        nameof(DiehardEnums.AbstractObject.DMDAK47Gun) => 15,
        nameof(DiehardEnums.AbstractObject.DMDShotgun) => 3,
        nameof(DiehardEnums.AbstractObject.DMDBFGGun) => 1,
        nameof(DiehardEnums.AbstractObject.DMDMinigun) => 50,
        nameof(DiehardEnums.AbstractObject.DMDDerringerGun) => 2,
        _ => 6
    };

    public AbstractGun(World world, AbstractObjectType type, WorldCoordinate pos, EntityID ID) : base(world, type, null, pos, ID)
    {
    }

    public override void Realize()
    {
        base.Realize();
        if (realizedObject != null) return;

        if (type == DiehardEnums.AbstractObject.DMDAK47Gun)
        {
            realizedObject = new AK47(this, world);
        }
        else if (type == DiehardEnums.AbstractObject.DMDDerringerGun)
        {
            realizedObject = new Derringer(this, world);
        }
        else if (type == DiehardEnums.AbstractObject.DMDMinigun)
        {
            realizedObject = new Minigun(this, world);
        }
        else if (type == DiehardEnums.AbstractObject.DMDGrenadeLauncherGun)
        {
            realizedObject = new GrenadeLauncher(this, world);
        }
        else if (type == DiehardEnums.AbstractObject.DMDShotgun)
        {
            realizedObject = new Shotgun(this, world);
        }
        else if (type == DiehardEnums.AbstractObject.DMDBFGGun)
        {
            realizedObject = new BFG(this, world);
        }
    }
}