using DiehardMasterDisaster.GunStuff;
using RWCustom;
using UnityEngine;

namespace DiehardMasterDisaster.Guns;

public class AK47 : Gun
{
    public AK47(AbstractPhysicalObject abstractPhysicalObject, World world) : base(abstractPhysicalObject, world)
    {
        fireSpeed = 6;
        reloadSpeed = 35;
        clipSize = 15;
        damageStat = 0.3f;
        automatic = true;
        GunSpriteName = "DMDAK-47";
        gunLength = 60;
        randomSpreadStat = 1.4f;
        angleDiff = 19;
        ammoType = DiehardEnums.AmmoType.Large;
        CheckIfArena(world);
    }

    public override void ShootSound()
    {
        room.PlaySound(DiehardEnums.Sound.DMDAK47Shoot, bodyChunks[0], false, .36f + Random.value * .02f, 1.05f + Random.value * .2f);
    }

    public override void SummonProjectile(PhysicalObject user, bool boostAccuracy)
    {
        var newBullet = new Bullet(user, firstChunk.pos + upDir * 5f + aimDir * (gunLength / 2f), (aimDir.normalized + (UnityEngine.Random.insideUnitCircle * randomSpreadStat * (boostAccuracy ? 0.3f : 1f)) * .045f).normalized, damageStat, 4.5f + 2f * damageStat, 15f + 30f * damageStat);        
        room.AddObject(newBullet);
        newBullet.Fire();
        user.bodyChunks[0].vel -= aimDir * 2f;
        user.bodyChunks[1].vel -= aimDir * 2f;
    }

    public override void ShootEffects()
    {
        var upDir = Custom.PerpendicularVector(aimDir);
        if (upDir.y < 0)
        {
            upDir *= -1f;
        }

        room.AddObject(new Explosion.ExplosionLight(firstChunk.pos + upDir * 5f + aimDir * (gunLength / 2f), 60f, 1f, 4, Color.yellow));
        for (var i = 0; i < 2; i++)
        {
            room.AddObject(new Spark(firstChunk.pos + upDir * 5f + aimDir * (gunLength / 2f), aimDir * 50f * Random.value + Custom.RNV() * 1.5f, Color.Lerp(Color.white, Color.yellow, Random.value), null, 3, 8));
        }
    }
}