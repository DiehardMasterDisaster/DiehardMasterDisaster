using DiehardMasterDisaster.Fisobs;
using RWCustom;
using UnityEngine;

namespace DiehardMasterDisaster.GunStuff;

public class Projectile : UpdatableAndDeletable, IDrawable, SharedPhysics.IProjectileTracer
{
    public readonly PhysicalObject shooter;

    public readonly SharedPhysics.TerrainCollisionData terrainCollisionData = new();
    
    public Vector2 pos;
    public Vector2 lastPos;
    public Vector2 vel;
    public float rad;
    public float gravityMod;
    public int maxBounces;
    public float bounciness;
    public int lifeTime;
    public bool alignSprite;

    public int bounces;
    public int age;
    public bool expired;
    
    public Projectile(PhysicalObject shooter, Vector2 pos, Vector2 vel, float rad, float gravityMod = 0, int maxBounces = 0, float bounciness = 1, int lifeTime = -1, bool alignSprite = true)
    {
        this.shooter = shooter;
        this.pos = lastPos = pos;
        this.vel = vel;
        this.rad = rad;
        this.gravityMod = gravityMod;
        this.maxBounces = maxBounces;
        this.bounciness = bounciness;
        this.lifeTime = lifeTime;
        this.alignSprite = alignSprite;
    }

    public virtual void Bounce(Vector2 normal)
    {
        vel = Vector2.Reflect(vel, normal) * bounciness;
        bounces++;
    }

    public virtual void HitTerrain(Vector2 collisionPos)
    {
        expired = true; 
        Destroy();
    }

    public virtual void HitChunk(BodyChunk chunk)
    {
        expired = true; 
        Destroy();
    }

    public virtual void Expire()
    {
        expired = true; 
        Destroy();
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        if (lifeTime > 0)
        {
            age++;
            if (age > lifeTime && !expired)
            {
                Expire();
            }
        }

        if (expired) return;
        
        lastPos = pos;
        pos += vel;
        
        vel.y -= gravityMod * room.gravity;

        var chunkCollision = SharedPhysics.TraceProjectileAgainstBodyChunks(this, room, lastPos, ref pos, rad, 1, shooter, true);

        if (chunkCollision.chunk != null)
        {
            HitChunk(chunkCollision.chunk);
        }
        
        var terrainCollision = terrainCollisionData.TerrainCollision(room, pos, lastPos, vel, rad).contactPoint.ToVector2();

        if (terrainCollision != Vector2.zero)
        {
            if (bounces >= maxBounces && maxBounces > -1)
            {
                HitTerrain(terrainCollision);
            }
            else
            {
                Bounce(terrainCollision * -1);
            }
        }
    }

    public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];
        sLeaser.sprites[1] = new FSprite("Circle20");
        
        AddToContainer(sLeaser, rCam, null);
    }

    public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        sLeaser.sprites[1].SetPosition(Vector2.Lerp(lastPos, pos, timeStacker));
        if (alignSprite)
        {
            sLeaser.sprites[1].rotation = Custom.VecToDeg(vel) - 90;
        }
    }

    public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
    }

    public virtual void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        newContatiner ??= rCam.ReturnFContainer("Midground");
        newContatiner.AddChild(sLeaser.sprites[1]);
    }

    public virtual bool HitThisObject(PhysicalObject obj) => obj is Creature && obj != shooter && (Custom.rainWorld.options.friendlyFire || obj is not Player);

    public virtual bool HitThisChunk(BodyChunk chunk) => true;
}