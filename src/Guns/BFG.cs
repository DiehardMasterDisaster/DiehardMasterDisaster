using System.Collections.Generic;
using System.Linq;
using DiehardMasterDisaster.Fisobs;
using DiehardMasterDisaster.GunStuff;
using MoreSlugcats;
using Noise;
using RWCustom;
using UnityEngine;

namespace DiehardMasterDisaster.Guns;

public class BFG : Gun
{
    public BFG(AbstractGun abstractPhysicalObject, World world) : base(abstractPhysicalObject, world)
    {
        fireSpeed = 20;
        reloadSpeed = 120;
        damageStat = 3f;
        GunSpriteName = "DMDBFG9000";
        gunLength = 87;
        randomSpreadStat = 0.2f;
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
        Vector2 direction;

        if (user is Player player && player.IsDMD(out var dmd) && dmd.ManualAim)
        {
            direction = aimDir.normalized;
        }
        else
        {
            direction = new Vector2(aimDir.x, 0).normalized;
        }
        
        room.AddObject(new BFGOrb(user, firstChunk.pos + direction * 5, direction));
    }

    public class BFGOrb : Projectile
    {
        public BFGOrb(PhysicalObject shooter, Vector2 pos, Vector2 direction) : base(shooter, pos, direction * speed, 1.5f, lifeTime: detonationAge)
        {
        }

        private const float speed = 5f;
        private const int detonationAge = 120;
        private const int finishAge = 300;
        private const float range = 250;
        private const int damageInterval = 20;
        private const float damage = 0.5f;

        private bool detonated;
        private LightningMachine lightning;

        public List<Creature> AffectedCreatures() => room.updateList.Where(x => x is Creature creature && Custom.DistLess(pos, creature.mainBodyChunk.pos, range) && !creature.dead && creature != shooter).Select(x => x as Creature).ToList();
        
        public override void Update(bool eu)
        {
            base.Update(eu);

            if (Random.value < (detonated ? 0.1f : 0.05f))
            {
                room.AddObject(new SingularityBomb.SparkFlash(pos, detonated ? 300 : 200f, new Color(0f, 1f, 0f)));
            }

            if (lightning == null)
            {
                lightning = new LightningMachine(pos, new Vector2(pos.x, pos.y), new Vector2(pos.x, pos.y + 10f), 0f, permanent: false, radial: true, 0.3f, 1f, 1f);
                lightning.volume = 0.8f;
                lightning.impactType = 3;
                lightning.lightningType = 0.33f;
                room.AddObject(lightning);
            }
            
            var dist = Mathf.Clamp(age / 50f, 0.2f, 1f);
            lightning.pos = pos;
            lightning.startPoint = new Vector2(Mathf.Lerp(pos.x, detonated ? 400 : 200f, dist * 2f - 2f), pos.y);
            lightning.endPoint = new Vector2(Mathf.Lerp(pos.x, detonated ? 400 : 200f, dist * 2f - 2f), pos.y + 10f);
            lightning.chance = Mathf.Lerp(0.2f, 0.8f, dist);

            if (age % damageInterval == 0)
            {
                foreach (var creature in AffectedCreatures())
                {
                    creature.killTag = (shooter as Creature)?.abstractCreature;
                    creature.Violence(shooter.firstChunk, default, creature.bodyChunks[Random.Range(0, creature.bodyChunks.Length)], null, Creature.DamageType.Electric, damage, 20);
                }
            }

            if (age > finishAge)
            {
                Finish();
            }
        }

        public override void HitTerrain(Vector2 collisionPos)
        {
            Detonate();
        }

        public override void HitChunk(BodyChunk chunk)
        {
            Detonate();
        }

        public override void Expire()
        {
            Detonate();
        }

        private void Detonate()
        {
            if (detonated) return;

            vel = Vector2.zero;
            age = detonationAge;
            expired = true;

            room.AddObject(new SingularityBomb.SparkFlash(pos, 300f, new Color(0f, 1f, 0f)));
            room.AddObject(new Explosion(room, null, pos, 7, 450f, 6.2f, 0.5f, 280f, 0.25f, shooter as Creature, 0.3f, 160f, 1f));
            room.AddObject(new Explosion(room, null, pos, 7, 2000f, 4f, 0f, 400f, 0.25f, shooter as Creature, 0.3f, 200f, 1f));
            room.AddObject(new Explosion.ExplosionLight(pos, 280f, 1f, 7, new Color(0, 1, 0)));
            room.AddObject(new Explosion.ExplosionLight(pos, 230f, 1f, 3, new Color(1f, 1f, 1f)));
            room.AddObject(new Explosion.ExplosionLight(pos, 2000f, 2f, 60, new Color(0, 1, 0)));
            room.AddObject(new ShockWave(pos, 2000f, 0.185f, 180));

            for (var i = 0; i < 25; i++)
            {
                var rnv = Custom.RNV();
                if (room.GetTile(pos + rnv * 20f).Solid)
                {
                    if (!room.GetTile(pos - rnv * 20f).Solid)
                    {
                        rnv *= -1f;
                    }
                    else
                    {
                        rnv = Custom.RNV();
                    }
                }
                for (var j = 0; j < 3; j++)
                {
                    room.AddObject(new Spark(pos + rnv * Mathf.Lerp(30f, 60f, Random.value), rnv * Mathf.Lerp(7f, 38f, Random.value) + Custom.RNV() * 20f * Random.value, Color.Lerp(new Color(0, 1, 0), new Color(1f, 1f, 1f), Random.value), null, 11, 28));
                }
                room.AddObject(new Explosion.FlashingSmoke(pos + rnv * 40f * Random.value, rnv * Mathf.Lerp(4f, 20f, Mathf.Pow(Random.value, 2f)), 1f + 0.05f * Random.value, new Color(1f, 1f, 1f), new Color(0, 1, 0), Random.Range(3, 11)));
            }
            
            room.ScreenMovement(pos, default, 0.9f);
            room.PlaySound(SoundID.Bomb_Explode, pos);
            room.InGameNoise(new InGameNoise(pos, 9000f, shooter, 1f));
            
            foreach (var creature in AffectedCreatures())
            {
                creature.killTag = (shooter as Creature)?.abstractCreature;
                creature.Die();
            }

            detonated = true;
        }

        private void Finish()
        {
            lightning?.Destroy();
            lightning = null;
            Destroy();
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            rCam.ReturnFContainer("HUD").AddChild(sLeaser.sprites[0]);
        }
    
        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = new FSprite("DMDBFGOrb0", true);
            sLeaser.sprites[0].scale = 1f;
    
            AddToContainer(sLeaser, rCam, null);
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
        }
    
        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            sLeaser.sprites[0].SetElementByName("DMDBFGOrb" + (age % 5 == 0 ? 1 : 0));
            sLeaser.sprites[0].SetPosition(Vector2.Lerp(lastPos, pos, timeStacker) - camPos);
    
            if (age % 5 == 0)
                sLeaser.sprites[0].rotation = Random.value * 360f;
    
            if (detonated || slatedForDeletetion || room != rCam.room)
            {
                sLeaser.CleanSpritesAndRemove();
            }
        }
    }
}