using DiehardMasterDisaster.Fisobs;
using DiehardMasterDisaster.GunStuff;
using RWCustom;
using UnityEngine;

namespace DiehardMasterDisaster.Guns;

public class BFG : Gun
{
    public BFG(AbstractPhysicalObject abstractPhysicalObject, World world) : base(abstractPhysicalObject, world)
    {
        fireSpeed = 20;
        reloadSpeed = 120;
        clipSize = 1;
        damageStat = 3f;
        GunSpriteName = "DMDBFG9000";
        gunLength = 87;
        randomSpreadStat = 0.2f;
        ammoCost = 4;
        ammoType = DiehardEnums.AmmoType.Special;
        CheckIfArena(world);
    }

    public override void ShootSound()
    {
        room.PlaySound(SoundID.Slugcat_Terrain_Impact_Medium, firstChunk.pos, 6, 0.5f);
    }

    public override void ShootEffects()
    {
        var upDir = Custom.PerpendicularVector(aimDir);
        if (upDir.y < 0)
        {
            upDir *= -1f;
        }

        room.AddObject(new Explosion.ExplosionLight(firstChunk.pos + upDir * 5f + aimDir * (gunLength / 2f), 100f, 1f, 5, Color.red));
        for (var i = 0; i < 3; i++)
        {
            room.AddObject(new Spark(firstChunk.pos + upDir * 5f + aimDir * (gunLength / 2f), aimDir * 50f * Random.value + Custom.RNV() * 1.5f, Color.Lerp(Color.white, Color.yellow, Random.value), null, 3, 8));
        }
    }

    public override void SummonProjectile(PhysicalObject user, bool boostAccuracy)
    {
        var orbAPO = new AbstractProjectile(room.world, DiehardEnums.AbstractObject.DMDBFGOrb, abstractPhysicalObject.pos, room.world.game.GetNewID());
        orbAPO.RealizeInRoom();
        var orb = (BFGOrb)orbAPO.realizedObject;

        //dont let pebbels shoot it !!
        orb.firstChunk.pos = firstChunk.pos + aimDir * 5;
    }
    
    public class BFGOrb : PhysicalObject, IDrawable
    {
        public BFGOrb(AbstractPhysicalObject abstractPhysicalObject) : base(abstractPhysicalObject)
        {
            bodyChunks = new BodyChunk[1];
            bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 1.5f, 20f);
            bodyChunkConnections = new BodyChunkConnection[0];
            airFriction = 0.01f;
            gravity = 0f;
            bounce = 0f;
            surfaceFriction = 0f;
            collisionLayer = 0;
            waterFriction = 0f;
            buoyancy = 0f;
        }
    
        float age = 1;
    
        public override void Update(bool eu)
        {
            age++;
            base.Update(eu);
        }
    
        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            rCam.ReturnFContainer("HUD").AddChild(sLeaser.sprites[0]);
            //rCam.ReturnFContainer("Items").AddChild(sLeaser.sprites[1]);
        }
    
        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = new FSprite("DMDBFGOrb0", true);
            sLeaser.sprites[0].scale = 1f;
            //sLeaser.sprites[0].shader = room.game.rainWorld.Shaders["Projection"];
    
            //sLeaser.sprites[1] = new FSprite("pixel", true);
            //sLeaser.sprites[1].scale = 10f;
            //sLeaser.sprites[1].color = new Color(0, 0, 0);
    
            AddToContainer(sLeaser, rCam, null);
        }
    
        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
        }
    
        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            sLeaser.sprites[0].SetElementByName("BFGOrb" + (age % 5 == 0 ? 1 : 0));
            sLeaser.sprites[0].SetPosition(firstChunk.pos - camPos);
    
            if (age % 5 == 0)
                sLeaser.sprites[0].rotation = Random.value * 360f;
    
            //sLeaser.sprites[1].SetPosition(firstChunk.pos - camPos);
        }
    }
}