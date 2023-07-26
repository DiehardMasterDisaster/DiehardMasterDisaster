using DiehardMasterDisaster.Guns;

namespace DiehardMasterDisaster.Fisobs;

public class AbstractGun : AbstractPhysicalObject
{
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