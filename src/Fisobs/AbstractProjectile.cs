using DiehardMasterDisaster.Guns;

namespace DiehardMasterDisaster.Fisobs;

public class AbstractProjectile : AbstractPhysicalObject
{
    public AbstractProjectile(World world, AbstractObjectType type, WorldCoordinate pos, EntityID ID) : base(world, type, null, pos, ID)
    {
    }

    public override void Realize()
    {
        base.Realize();
        if (realizedObject != null) return;

        if (type == DiehardEnums.AbstractObject.DMDPipe)
        {
            realizedObject = new GrenadeLauncher.Pipe(this, world);
        }
        else if (type == DiehardEnums.AbstractObject.DMDBFGOrb)
        {
            realizedObject = new BFG.BFGOrb(this);
        }
    }
}