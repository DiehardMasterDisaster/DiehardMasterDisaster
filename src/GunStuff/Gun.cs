using System.Collections.Generic;
using System.Linq;
using RWCustom;
using UnityEngine;

namespace DiehardMasterDisaster.GunStuff;

abstract public class Gun : Weapon, IDrawable
{
    public Creature owner;
    public Creature previousOwner;
    public float randomSpreadStat;
    public int gunLength;
    public bool automatic = false;
    public float damageStat;
    public int fireSpeed;
    public int reloadSpeed;
    public int clipSize;
    public Vector2 aimDir;
    public Vector2 lastAimDir;
    public int fireDelay;
    public int reloadTime;
    public bool justShot;
    private bool smokeFromShot;
    private int timeFromLastShotAttempt;
    private bool triggerIsReleased = true;
    public bool flipGun;
    public bool autoFlip;
    public GunSmolder smolder;
    protected int ownerAge;
    public DiehardEnums.AmmoType ammoType = DiehardEnums.AmmoType.Small;

    public int Clip
    {
        get => abstractPhysicalObject.ID.number;
        set => abstractPhysicalObject.ID.number = value;
    }

    public List<PhysicalObject> relatedObjects = new List<PhysicalObject>();

    public Gun(AbstractPhysicalObject abstractPhysicalObject, World world) : base(abstractPhysicalObject, world)
    {
        bodyChunks = new BodyChunk[1];
        bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 5f, 0.07f);
        aimDir = new Vector2(1f, 1f);
        bodyChunkConnections = new BodyChunkConnection[0];
        airFriction = 0.999f;
        gravity = 0.9f;
        bounce = 0.4f;
        surfaceFriction = 0.4f;
        collisionLayer = 2;
        waterFriction = 0.98f;
        buoyancy = 0.4f;
        aimDir.x = (UnityEngine.Random.value < 0 ? 1f : -1f);
        aimDir.y = (UnityEngine.Random.value * 2f - 1f) * .2f;
        lastAimDir = aimDir;
        autoFlip = true;
    }

    public void CheckIfArena(World world)
    {
        if (world.game.IsArenaSession)
        {
            Clip = clipSize;
        }
    }

    public string GunSpriteName;

    public override void PlaceInRoom(Room placeRoom)
    {
        base.PlaceInRoom(placeRoom);
        abstractPhysicalObject.ID.number = clipSize;
        firstChunk.pos = placeRoom.MiddleOfTile(abstractPhysicalObject.pos);
        firstChunk.lastPos = firstChunk.pos;
    }

    bool firstupdate = true;

    public Vector2 upDir
    {
        get
        {
            var dir = Custom.PerpendicularVector(aimDir);
            if (dir.y < 0)
            {
                dir *= -1f;
            }

            return dir;
        }
    }

    public override void Update(bool eu)
    {
        if (owner != null) previousOwner = owner;

        if (firstupdate && owner is Player p)
        {
            Clip = p.GetDMD().TrySubtractAmmo(ammoType, clipSize);
        }

        justShot = false;
        aimDir.Normalize();
        lastAimDir = aimDir;
        if (autoFlip)
        {
            if (flipGun && aimDir.x < -.5)
            {
                flipGun = false;
            }
            else if (!flipGun && aimDir.x > .5)
            {
                flipGun = true;
            }
        }

        if (reloadTime > 0)
        {
            if (reloadTime == reloadSpeed - 7)
            {
                room.PlaySound(SoundID.Spear_Bounce_Off_Creauture_Shell, firstChunk.pos + aimDir * 25f, 0.5f, 1.25f);
            }

            if (reloadTime % 11 == 1)
            {
                room.PlaySound(SoundID.Seed_Cob_Pop, firstChunk.pos + aimDir * 25f, 1f, .875f);
            }

            if (reloadTime == 1)
            {
                room.PlaySound(SoundID.Spear_Bounce_Off_Creauture_Shell, firstChunk.pos + aimDir * 25f, 0.8f, 1.4f);
                if (owner is Player player)
                {
                    Clip = player.GetDMD().TrySubtractAmmo(ammoType, clipSize);
                }
            }

            reloadTime--;
        }

        if (fireDelay > 0)
        {
            fireDelay--;
            if (smokeFromShot)
            {
                if (smolder == null)
                {
                    smolder = new GunSmolder(room, firstChunk.pos + upDir * 5f + aimDir * (gunLength / 2f), null, null);
                    room.AddObject(smolder);
                }

                smolder.life = 100;
                for (var i = 0; i < 3; i++)
                {
                    smolder.AddParticle(smolder.pos + upDir * 5f + aimDir * gunLength / 2f, aimDir * (10f + 30f * UnityEngine.Random.value) + UnityEngine.Random.insideUnitCircle * 14f, 30f);
                }
            }
        }

        if (smolder != null)
        {
            smolder.pos = firstChunk.pos + upDir * 5f + aimDir * (gunLength / 2f);
            if (smolder.slatedForDeletetion)
            {
                smolder = null;
            }
        }

        triggerIsReleased = timeFromLastShotAttempt > 1;
        timeFromLastShotAttempt++;

        if (mode == Mode.Free)
        {
            owner = null;
        }

        if (owner != null)
        {
            ownerAge++;
        }

        base.Update(eu);
        firstupdate = false;
    }

    public void TryShoot(PhysicalObject user)
    {
        if (user is not Player)
        {
            return;
        }

        Vector2 targetCoord;
        var right = aimDir.x > 0;
        var pos = firstChunk.pos;
        var recordDist = float.PositiveInfinity;
        BodyChunk recordChunk = null;

        foreach (var testObject in room.physicalObjects[1].Where(x => x is Creature c && !(c == user || c.dead))) //1 represents the main collision layer
        {
            foreach (var chunk in testObject.bodyChunks)
            {
                if (Mathf.Abs(chunk.pos.y - firstChunk.pos.y) <= 30 && (chunk.pos.x > firstChunk.pos.x == right) && (Mathf.Abs(chunk.pos.x - pos.x) < recordDist))
                {
                    recordDist = Mathf.Abs(chunk.pos.x - pos.x);
                    recordChunk = chunk;
                }
            }
        }

        if (recordChunk != null)
        {
            targetCoord = recordChunk.pos - firstChunk.pos;
        }
        else targetCoord = aimDir;

        timeFromLastShotAttempt = 0;
        if (fireDelay == 0 && Clip == 0 && !justShot && (reloadTime == 0 || reloadTime == reloadSpeed))
        {
            smokeFromShot = false;
            fireDelay = fireSpeed;
            Reload();
        }

        if (fireDelay == 0 && Clip > 0)
        {
            Shoot(user, targetCoord);
        }
    }

    public void Reload()
    {
        var canReload = owner is Player p && p.GetDMD().HasAmmo(ammoType);

        if (canReload || owner is Scavenger || owner == null)
        {
            reloadTime = reloadSpeed;
        }
        else if (triggerIsReleased)
        {
            room.PlaySound(SoundID.Rock_Hit_Wall, firstChunk.pos, .9f, 1.35f);
            firstChunk.pos -= aimDir * 5f;
            firstChunk.lastPos = firstChunk.pos;
        }
    }

    public virtual void Shoot(PhysicalObject user, Vector2 fireDir)
    {
        Clip--;
        smokeFromShot = true;
        fireDelay = fireSpeed;
        justShot = true;
        aimDir = fireDir.normalized;
        if (user is Player pla && pla.input[0].y < -.35f) aimDir = new Vector2(0, -1);

        var boostAccuracy = user is Player p && (p.animation == Player.AnimationIndex.Flip || p.bodyMode == Player.BodyModeIndex.Crawl);

        var upDir = Custom.PerpendicularVector(aimDir);
        if (upDir.y < 0)
        {
            upDir *= -1f;
        }

        //effects
        room.AddObject(new Explosion.ExplosionLight(firstChunk.pos + upDir * 5f + aimDir * (gunLength / 2f), 75f, 1f, 5, Color.white));
        for (var i = 0; i < 3; i++)
        {
            room.AddObject(new Spark(firstChunk.pos + upDir * 5f + aimDir * (gunLength / 2f), aimDir * 50f * UnityEngine.Random.value + Custom.RNV() * 1.5f, Color.Lerp(Color.white, Color.yellow, UnityEngine.Random.value), null, 3, 8));
        }

        ShootSound();
        SummonProjectile(user, boostAccuracy);


        room.AddObject(new Spark(firstChunk.pos + upDir * 5f - aimDir * (gunLength / 3f), upDir * 8f + UnityEngine.Random.insideUnitCircle * 3f, Color.yellow, null, 60, 120));
        if (Clip == 0 && automatic)
            fireDelay = 20;
    }

    public abstract void ShootEffects();

    public abstract void SummonProjectile(PhysicalObject user, bool boostAccuracy);

    public override void Grabbed(Creature.Grasp grasp)
    {
        if (grasp?.grabber == null) return;
        owner = grasp.grabber;
        ChangeMode(Mode.Carried);
        
        if (grasp?.grabber is Player player && player.GetDMD().IsDMD)
        {
            
        }
        
        base.Grabbed(grasp);
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1 + clipSize];
        sLeaser.sprites[0] = new FSprite(GunSpriteName, true);
        sLeaser.sprites[0].anchorY = 0.5f; //.8

        for (var i = 1; i <= clipSize; i++)
        {
            sLeaser.sprites[i] = new FSprite("pixel")
            {
                scale = 4,
                isVisible = false
            };
        }

        firstPipAngle = 0 + (angleDiff / 2 * (clipSize - 1));

        AddToContainer(sLeaser, rCam, null);
    }

    public int angleDiff = 24;
    protected int firstPipAngle;

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName(GunSpriteName + (Clip == 0 ? "NoMag" : ""));
        sLeaser.sprites[0].x = Mathf.Lerp(firstChunk.lastPos.x, firstChunk.pos.x, timeStacker) - camPos.x;
        sLeaser.sprites[0].y = Mathf.Lerp(firstChunk.lastPos.y, firstChunk.pos.y, timeStacker) - camPos.y;
        sLeaser.sprites[0].rotation = Custom.AimFromOneVectorToAnother(new Vector2(0f, 0f), Vector3.Slerp(lastAimDir, aimDir, timeStacker)) - 90f;
        if (mode == Mode.OnBack)
        {
            Vector2 v = Vector3.Slerp(lastRotation, rotation, timeStacker);
            var perpV = Custom.PerpendicularVector(v);
            sLeaser.sprites[0].rotation = Custom.AimFromOneVectorToAnother(new Vector2(0f, 0f), perpV);
            sLeaser.sprites[0].scaleY = -1f;
        }
        else
            sLeaser.sprites[0].scaleY = (flipGun ? 1f : -1f);


        if (owner != null && owner is Player p && mode == Mode.Carried)
            for (var i = 1; i <= clipSize; i++)
            {
                sLeaser.sprites[i].isVisible = i <= Clip && p.GetDMD().EquippedGun == this;
                sLeaser.sprites[i].SetPosition(Custom.DegToVec((firstPipAngle - (angleDiff * (i - 1)))) * 30 + owner.firstChunk.pos + new Vector2(0, 20) - camPos);
            }
        else
        {
            for (var i = 1; i <= clipSize; i++)
            {
                sLeaser.sprites[i].isVisible = false;
            }
        }

        for (var i = 1; i <= clipSize; i++)
        {
            sLeaser.sprites[i].alpha = ownerAge / 20f;
        }


        if (slatedForDeletetion || room != rCam.room)
        {
            sLeaser.CleanSpritesAndRemove();
        }
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        if (newContatiner == null)
        {
            newContatiner = rCam.ReturnFContainer("Items");
        }

        var HUDcont = rCam.ReturnFContainer("HUD");

        sLeaser.sprites[0].RemoveFromContainer();
        newContatiner.AddChild(sLeaser.sprites[0]);
        for (var i = 1; i < sLeaser.sprites.Length; i++)
        {
            sLeaser.sprites[i].RemoveFromContainer();
            HUDcont.AddChild(sLeaser.sprites[i]);
        }
    }

    public virtual void ShootSound()
    {
    }
}