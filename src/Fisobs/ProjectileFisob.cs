using DiehardMasterDisaster.Guns;
using Fisobs.Core;
using Fisobs.Items;
using Fisobs.Properties;
using Fisobs.Sandbox;

namespace DiehardMasterDisaster.Fisobs;

public class ProjectileFisob : Fisob
{
    public ProjectileFisob(AbstractPhysicalObject.AbstractObjectType type) : base(type)
    {
        //Icon = new ProjectileIcon();
    }

    public override AbstractPhysicalObject Parse(World world, EntitySaveData entitySaveData, SandboxUnlock unlock)
    {
        return new AbstractProjectile(world, entitySaveData.Type.ObjectType, entitySaveData.Pos, entitySaveData.ID);
    }

    private static readonly ProjectileProperties properties = new();

    public override ItemProperties Properties(PhysicalObject forObject)
    {
        return properties;
    }
}