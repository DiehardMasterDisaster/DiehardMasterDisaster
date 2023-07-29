using System;
using DiehardMasterDisaster.Fisobs;
using DiehardMasterDisaster.GunStuff;
using Noise;
using RWCustom;
using Smoke;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DiehardMasterDisaster.Guns;

public class GrenadeLauncher : Gun
{
    public GrenadeLauncher(AbstractGun abstractPhysicalObject, World world) : base(abstractPhysicalObject, world)
    {
        fireSpeed = 20;
        reloadSpeed = 120;
        damageStat = 3f;
        GunSpriteName = "DMDRocketLauncher";
        gunLength = 87;
        randomSpreadStat = 0.2f;
        CheckIfArena(world);
    }

    public override void ShootSound()
    {
        room.PlaySound(SoundID.Slugcat_Terrain_Impact_Medium, firstChunk.pos, 6, 0.5f);
        //SoundHelper.PlayCustomSound("AK-47Shoot", bodyChunks[0], .55f + UnityEngine.Random.value * .02f, .9f + UnityEngine.Random.value * .1f);
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
        var pipeAPO = new AbstractProjectile(room.world, DiehardEnums.AbstractObject.DMDPipe, abstractPhysicalObject.pos, room.world.game.GetNewID());
        pipeAPO.RealizeInRoom();
        var newPipe = (Pipe)pipeAPO.realizedObject;

        //dont let pebbels shoot it !!
        newPipe.firstChunk.pos = firstChunk.pos + aimDir * 5;
        newPipe.InitiateBurn();
        if (aimDir.y > -.4)
        {
            newPipe.firstChunk.vel = aimDir * 15f;
            newPipe.firstChunk.vel.y += 10;
        }
        else
        {
            newPipe.firstChunk.vel = aimDir * 8f + new Vector2(((Player)user).ThrowDirection * 6, 0);
        }

        relatedObjects.Add(newPipe);
    }

    public override void NewRoom(Room newRoom)
    {
        foreach (var o in relatedObjects)
        {
            var p = (Pipe)o;
            p.Pipe_Explode(null);
        }

        base.NewRoom(newRoom);
    }

    public override void Destroy()
    {
        foreach (var o in relatedObjects)
        {
            var p = (Pipe)o;
            p.Pipe_Explode(null);
        }

        base.Destroy();
    }

    public class Pipe : ScavengerBomb
    {
        public Pipe(AbstractPhysicalObject abstractPhysicalObject, World world) : base(abstractPhysicalObject, world)
        {
            bounce = .3f;
            gravity = .8f;
            rotationSpeed = (Random.value * .5f) / 10;
        }

        bool Stuck = false;
        Vector2 stuckPos;

        public void Pipe_InitiateBurn()
        {
            if (burn == 0f)
            {
                burn = 21400000f;
                firstChunk.vel += Custom.RNV() * Random.value * 6f;
            }
            else
            {
                burn = Mathf.Min(burn, 4);
            }
        }

        public void Pipe_Explode(BodyChunk hitChunk)
        {
            if (slatedForDeletetion)
            {
                return;
            }

            var vector = Vector2.Lerp(firstChunk.pos, firstChunk.lastPos, 0.35f);
            room.AddObject(new SootMark(room, vector, 60f, true));
            room.AddObject(new StickyExplosion(room, this, vector, 7, 200f, 5.2f, 1.4f, 200f, 0.2f, thrownBy, 0.7f, 160f, 1f));
            room.AddObject(new Explosion.ExplosionLight(vector, 230f, .7f, 5, explodeColor));
            room.AddObject(new Explosion.ExplosionLight(vector, 180f, .7f, 3, new Color(1f, 1f, 1f)));
            room.AddObject(new ExplosionSpikes(room, vector, 18, 30f, 9f, 7f, 170f, explodeColor));
            room.AddObject(new ShockWave(vector, 280f, 0.045f, 5));
            for (var i = 0; i < 25; i++)
            {
                var a = Custom.RNV();
                if (room.GetTile(vector + a * 20f).Solid)
                {
                    if (!room.GetTile(vector - a * 20f).Solid)
                    {
                        a *= -1f;
                    }
                    else
                    {
                        a = Custom.RNV();
                    }
                }

                for (var j = 0; j < 3; j++)
                {
                    room.AddObject(new Spark(vector + a * Mathf.Lerp(30f, 60f, Random.value), 2 * a * Mathf.Lerp(7f, 38f, Random.value) + Custom.RNV() * 20f * Random.value, Color.Lerp(explodeColor, new Color(1f, 1f, 1f), Random.value), null, 11, 28));
                }

                for (var k = 0; k < 6; k++)
                {
                    room.AddObject(new CollectToken.TokenSpark(vector + Custom.RNV() * Mathf.Lerp(30f, 60f, Random.value), 2 * Custom.RNV() * Mathf.Lerp(7f, 38f, Random.value) + Custom.RNV() * 15f * Random.value, explodeColor, false));
                }

                room.AddObject(new Explosion.FlashingSmoke(vector + a * 40f * Random.value, 2 * a * Mathf.Lerp(4f, 20f, Mathf.Pow(Random.value, 2f)), 1f + 0.05f * Random.value, new Color(1f, 1f, 1f), explodeColor, Random.Range(3, 11)));
            }

            if (smoke != null)
            {
                for (var k = 0; k < 8; k++)
                {
                    smoke.EmitWithMyLifeTime(vector + Custom.RNV(), Custom.RNV() * Random.value * 17f);
                }
            }

            for (var l = 0; l < 6; l++)
            {
                room.AddObject(new BombFragment(vector, Custom.DegToVec((l + Random.value) / 6f * 360f) * Mathf.Lerp(18f, 38f, Random.value)));
            }

            room.ScreenMovement(vector, default, 1.3f);
            for (var m = 0; m < abstractPhysicalObject.stuckObjects.Count; m++)
            {
                abstractPhysicalObject.stuckObjects[m].Deactivate();
            }

            room.PlaySound(SoundID.Bomb_Explode, vector, .8f, 1.1f + .3f * Random.value);
            room.InGameNoise(new InGameNoise(vector, 18000f, this, 1f));
            var flag = hitChunk != null;
            for (var n = 0; n < 5; n++)
            {
                if (room.GetTile(vector + Custom.fourDirectionsAndZero[n].ToVector2() * 20f).Solid)
                {
                    flag = true;
                    break;
                }
            }

            if (flag)
            {
                if (smoke == null)
                {
                    smoke = new BombSmoke(room, vector, null, explodeColor);
                    room.AddObject(smoke);
                }

                if (hitChunk != null)
                {
                    smoke.chunk = hitChunk;
                }
                else
                {
                    smoke.chunk = null;
                    smoke.fadeIn = 1f;
                }

                smoke.pos = vector;
                smoke.stationary = true;
                smoke.DisconnectSmoke();
            }
            else if (smoke != null)
            {
                smoke.Destroy();
            }

            Destroy();
        }

        public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
        {
            if (firstContact)
            {
                if (speed * bodyChunks[chunk].mass > 7f)
                {
                    room.ScreenMovement(bodyChunks[chunk].pos, Custom.IntVector2ToVector2(direction) * speed * bodyChunks[chunk].mass * 0.1f, Mathf.Max((speed * bodyChunks[chunk].mass - 30f) / 50f, 0f));
                }

                if (speed > 4f && speed * bodyChunks[chunk].loudness * Mathf.Lerp(bodyChunks[chunk].mass, 1f, 0.5f) > 0.5f)
                {
                    room.InGameNoise(new InGameNoise(bodyChunks[chunk].pos + IntVector2.ToVector2(direction) * bodyChunks[chunk].rad * 0.9f, Mathf.Lerp(350f, Mathf.Lerp(100f, 1500f, Mathf.InverseLerp(0.5f, 20f, speed * bodyChunks[chunk].loudness * Mathf.Lerp(bodyChunks[chunk].mass, 1f, 0.5f))), 0.5f), this, 4f));
                }
            }
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            if (Stuck)
            {
                ChangeCollisionLayer(2);
                firstChunk.rad = 0;
                CollideWithTerrain = false;
                gravity = 0;
                firstChunk.vel = Vector2.zero;
                firstChunk.pos = stuckPos;
                rotationSpeed = 0;
            }

            if (firstChunk.contactPoint != new IntVector2(0, 0))
            {
                Stuck = true;
                stuckPos = firstChunk.pos;
            }
        }

        public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
        {
            if (result.obj == null)
            {
                return false;
            }

            ChangeMode(Mode.Free);
            firstChunk.vel = firstChunk.vel * -0.2f;
            SetRandomSpin();
            if (result.obj is Creature creature)
            {
                creature.Violence(firstChunk, firstChunk.vel * firstChunk.mass, result.chunk, result.onAppendagePos, Creature.DamageType.Blunt, 0.1f, 10f);
                room.PlaySound(SoundID.Rock_Hit_Creature, firstChunk);
            }
            else if (result.chunk != null)
            {
                result.chunk.vel += firstChunk.vel * firstChunk.mass / result.chunk.mass;
            }
            else if (result.onAppendagePos != null)
            {
                (result.obj as IHaveAppendages)?.ApplyForceOnAppendage(result.onAppendagePos, firstChunk.vel * firstChunk.mass);
            }

            if (!ignited)
                InitiateBurn();
            return true;
        }

        public override void WeaponDeflect(Vector2 inbetweenPos, Vector2 deflectDir, float bounceSpeed)
        {
            firstChunk.pos = Vector2.Lerp(firstChunk.pos, inbetweenPos, 0.5f);
            vibrate = 20;
            ChangeMode(Mode.Free);
            firstChunk.vel = deflectDir * bounceSpeed * 0.5f;
            if (!ignited)
                InitiateBurn();
            SetRandomSpin();
        }

        //public override void HitByExplosion(float hitFac, Explosion explosion, int hitChunk)
        //{

        //}

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = new FSprite("DMDSticky", true);
            sLeaser.sprites[0].color = Color.white;
            sLeaser.sprites[0].scale = 0.6f;

            AddToContainer(sLeaser, rCam, null);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            var position = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
            if (vibrate > 0)
            {
                position += Custom.DegToVec(Random.value * 360f) * 2f * Random.value;
            }

            Vector2 drawRotation = Vector3.Slerp(lastRotation, this.rotation, timeStacker);
            sLeaser.sprites[0].rotation = Custom.AimFromOneVectorToAnother(new Vector2(0f, 0f), drawRotation);
            sLeaser.sprites[0].SetPosition(position - camPos);
            sLeaser.sprites[0].scale = 0.4f;

            if (slatedForDeletetion || room != rCam.room)
            {
                sLeaser.CleanSpritesAndRemove();
            }
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
        }
    }

    public class StickyExplosion : Explosion
    {
        public StickyExplosion(Room room, PhysicalObject sourceObject, Vector2 pos, int lifeTime, float rad, float force, float damage, float stun, float deafen, Creature killTagHolder, float killTagHolderDmgFactor, float minStun, float backgroundNoise) : base(room, sourceObject, pos, lifeTime, rad, force, damage, stun, deafen, killTagHolder, killTagHolderDmgFactor, minStun, backgroundNoise)
        {
        }

        public override void Update(bool eu)
        {
            evenUpdate = eu; // this is what the base.update would do for normal explosions

            if (!explosionReactorsNotified)
            {
                explosionReactorsNotified = true;
                for (var i = 0; i < room.updateList.Count; i++)
                {
                    if (room.updateList[i] is IReactToExplosions)
                    {
                        (room.updateList[i] as IReactToExplosions).Explosion(this);
                    }
                }

                if (room.waterObject != null)
                {
                    room.waterObject.Explosion(this);
                }

                if (sourceObject != null)
                {
                    room.InGameNoise(new InGameNoise(pos, backgroundNoise * 2700f, sourceObject, backgroundNoise * 6f));
                }
            }

            room.MakeBackgroundNoise(backgroundNoise);
            var num = rad * (0.25f + 0.75f * Mathf.Sin(Mathf.InverseLerp(0f, lifeTime, frame) * 3.1415927f));
            for (var j = 0; j < room.physicalObjects.Length; j++)
            {
                for (var k = 0; k < room.physicalObjects[j].Count; k++)
                {
                    if (sourceObject != room.physicalObjects[j][k] && !room.physicalObjects[j][k].slatedForDeletetion)
                    {
                        var num2 = 0f;
                        var num3 = float.MaxValue;
                        var num4 = -1;
                        for (var l = 0; l < room.physicalObjects[j][k].bodyChunks.Length; l++)
                        {
                            var num5 = Vector2.Distance(pos, room.physicalObjects[j][k].bodyChunks[l].pos);
                            num3 = Mathf.Min(num3, num5);
                            if (num5 < num)
                            {
                                var num6 = Mathf.InverseLerp(num, num * 0.25f, num5);
                                if (!room.VisualContact(pos, room.physicalObjects[j][k].bodyChunks[l].pos))
                                {
                                    num6 -= 0.5f;
                                }

                                if (num6 > 0f)
                                {
                                    room.physicalObjects[j][k].bodyChunks[l].vel += PushAngle(pos, room.physicalObjects[j][k].bodyChunks[l].pos) * (force / room.physicalObjects[j][k].bodyChunks[l].mass) * num6;
                                    room.physicalObjects[j][k].bodyChunks[l].pos += PushAngle(pos, room.physicalObjects[j][k].bodyChunks[l].pos) * (force / room.physicalObjects[j][k].bodyChunks[l].mass) * num6 * 0.1f;
                                    if (num6 > num2)
                                    {
                                        num2 = num6;
                                        num4 = l;
                                    }
                                }
                            }
                        }

                        if (room.physicalObjects[j][k] == killTagHolder)
                        {
                            num2 *= killTagHolderDmgFactor;
                        }

                        if (deafen > 0f && room.physicalObjects[j][k] is Creature)
                        {
                            (room.physicalObjects[j][k] as Creature).Deafen((int)Custom.LerpMap(num3, num * 1.5f * deafen, num * Mathf.Lerp(1f, 4f, deafen), 650f * deafen, 0f));
                        }

                        if (num4 > -1)
                        {
                            if (room.physicalObjects[j][k] is Creature)
                            {
                                var num7 = 0;
                                while (num7 < Math.Min(Mathf.Round(num2 * damage * 2f), 8f))
                                {
                                    var p = room.physicalObjects[j][k].bodyChunks[num4].pos + Custom.RNV() * room.physicalObjects[j][k].bodyChunks[num4].rad * Random.value;
                                    room.AddObject(new WaterDrip(p, Custom.DirVec(pos, p) * force * Random.value * num2, false));
                                    num7++;
                                }

                                if (killTagHolder != null && room.physicalObjects[j][k] != killTagHolder)
                                {
                                    (room.physicalObjects[j][k] as Creature).SetKillTag(killTagHolder.abstractCreature);
                                }

                                if ((room.physicalObjects[j][k] as Creature) is Player player && player.GetDMD().IsDMD)
                                    (room.physicalObjects[j][k] as Creature).Violence(null, null, room.physicalObjects[j][k].bodyChunks[num4], null, Creature.DamageType.Explosion, 0, num2 * stun * 0.05f);
                                else
                                    (room.physicalObjects[j][k] as Creature).Violence(null, null, room.physicalObjects[j][k].bodyChunks[num4], null, Creature.DamageType.Explosion, num2 * damage / ((!((room.physicalObjects[j][k] as Creature).State is HealthState)) ? 1f : lifeTime), num2 * stun);
                                if (minStun > 0f)
                                {
                                    (room.physicalObjects[j][k] as Creature).Stun((int)(minStun * Mathf.InverseLerp(0f, 0.5f, num2)));
                                }

                                if ((room.physicalObjects[j][k] as Creature).graphicsModule != null && (room.physicalObjects[j][k] as Creature).graphicsModule.bodyParts != null)
                                {
                                    for (var m = 0; m < (room.physicalObjects[j][k] as Creature).graphicsModule.bodyParts.Length; m++)
                                    {
                                        if ((room.physicalObjects[j][k] as Creature) is Player player2 && player2.GetDMD().IsDMD)
                                        {
                                            (room.physicalObjects[j][k] as Creature).graphicsModule.bodyParts[m].pos += PushAngle(pos, (room.physicalObjects[j][k] as Creature).graphicsModule.bodyParts[m].pos) * num2 * force * 2f;
                                            (room.physicalObjects[j][k] as Creature).graphicsModule.bodyParts[m].vel += PushAngle(pos, (room.physicalObjects[j][k] as Creature).graphicsModule.bodyParts[m].pos) * num2 * force * 2f;
                                        }
                                        else
                                        {
                                            (room.physicalObjects[j][k] as Creature).graphicsModule.bodyParts[m].pos += PushAngle(pos, (room.physicalObjects[j][k] as Creature).graphicsModule.bodyParts[m].pos) * num2 * force * 5f;
                                            (room.physicalObjects[j][k] as Creature).graphicsModule.bodyParts[m].vel += PushAngle(pos, (room.physicalObjects[j][k] as Creature).graphicsModule.bodyParts[m].pos) * num2 * force * 5f;
                                        }

                                        if ((room.physicalObjects[j][k] as Creature).graphicsModule.bodyParts[m] is Limb)
                                        {
                                            ((room.physicalObjects[j][k] as Creature).graphicsModule.bodyParts[m] as Limb).mode = Limb.Mode.Dangle;
                                        }
                                    }
                                }
                            }

                            room.physicalObjects[j][k].HitByExplosion(num2, this, num4);
                        }
                    }
                }
            }

            frame++;
            if (frame > lifeTime)
            {
                Destroy();
            }
        }
    }
}