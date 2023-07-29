using DiehardMasterDisaster.Fisobs;
using DiehardMasterDisaster.GunStuff;
using RWCustom;
using UnityEngine;

namespace DiehardMasterDisaster.Guns;

public class Shotgun : Gun
{
    public Shotgun(AbstractGun abstractPhysicalObject, World world) : base(abstractPhysicalObject, world)
    {
        fireSpeed = 30;
        reloadSpeed = 64;
        damageStat = 1.8f;
        GunSpriteName = "DMDShotty";
        gunLength = 47;
        randomSpreadStat = 5f;
        angleDiff = 30;
        CheckIfArena(world);
    }

    public override void ShootSound()
    {
        room.PlaySound(DiehardEnums.Sound.DMDAK47Shoot, bodyChunks[0], false, .5f + Random.value * .03f, 1f + Random.value * .07f);
        room.PlaySound(DiehardEnums.Sound.DMDAK47Shoot, bodyChunks[0], false, .45f + Random.value * .03f, .9f + Random.value * .07f);
    }

    public override void SummonProjectile(PhysicalObject user, bool boostAccuracy)
    {
        var mult = 6;
        for (var i = mult; i > 0; i--)
        {
            var newBullet = new Bullet(user, firstChunk.pos + aimDir * (gunLength / 2f), (aimDir.normalized + (Random.insideUnitCircle * randomSpreadStat * (boostAccuracy ? 0.3f : 1f)) * .045f).normalized, damageStat / mult, 1.5f + 2f * damageStat / mult, 15f + 30f * damageStat / mult);
            room.AddObject(newBullet);
            newBullet.Fire();
        }

        user.bodyChunks[0].vel -= aimDir * 5f;
        user.bodyChunks[1].vel -= aimDir * 3f;
    }

    public override void ShootEffects()
    {
        var upDir = Custom.PerpendicularVector(aimDir);
        if (upDir.y < 0)
        {
            upDir *= -1f;
        }

        room.AddObject(new Explosion.ExplosionLight(firstChunk.pos + upDir * 5f + aimDir * (gunLength / 2f), 60f, 1f, 4, Color.red));
        for (var i = 0; i < 8; i++)
        {
            room.AddObject(new Spark(firstChunk.pos + upDir * 5f + aimDir * (gunLength / 2f), aimDir * 50f * Random.value + Custom.RNV() * 1.5f, Color.Lerp(Color.white, Color.yellow, Random.value), null, 3, 8));
        }
    }
}