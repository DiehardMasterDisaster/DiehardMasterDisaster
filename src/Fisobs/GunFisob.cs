using Fisobs.Core;
using Fisobs.Items;
using Fisobs.Properties;
using Fisobs.Sandbox;

namespace DiehardMasterDisaster.Fisobs;

public class GunFisob : Fisob
{
    public GunFisob(AbstractPhysicalObject.AbstractObjectType type) : base(type)
    {
        //Icon = new ProjectileIcon();
    }

    public override AbstractPhysicalObject Parse(World world, EntitySaveData entitySaveData, SandboxUnlock unlock)
    {
        return new AbstractGun(world, entitySaveData.Type.ObjectType, entitySaveData.Pos, entitySaveData.ID);
    }

    private static readonly GunProperties properties = new();

    public override ItemProperties Properties(PhysicalObject forObject)
    {
        return properties;
    }
}