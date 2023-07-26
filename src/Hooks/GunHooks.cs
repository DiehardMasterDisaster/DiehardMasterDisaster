using DiehardMasterDisaster.Fisobs;
using DiehardMasterDisaster.GunStuff;
using RWCustom;
using UnityEngine;

namespace DiehardMasterDisaster.Hooks;

public static class GunHooks
{
    public static void Apply()
    {
        On.Player.Update += Player_Update;
        On.Player.ThrowObject += Player_ThrowObject;
        
        On.Player.Jump += (orig, self) =>
        {
            orig(self);
            var gun = new AbstractGun(self.room.world, DiehardEnums.AbstractObject.DMDAK47Gun, new WorldCoordinate(self.room.abstractRoom.index, 0, 0, -1), self.room.game.GetNewID());
            gun.RealizeInRoom();
            gun.realizedObject.bodyChunks[0].HardSetPosition(self.mainBodyChunk.pos);

            self.GetDMD().AddAmmo(DiehardEnums.AmmoType.Large, 20);
        };
    }

    private static void Player_ThrowObject(On.Player.orig_ThrowObject orig, Player self, int grasp, bool eu)
    {
        var thrownObject = self.grasps[grasp]?.grabbed;
        var throwDir = new IntVector2(self.ThrowDirection, 0);

        if (thrownObject is Gun gun && (!gun.automatic || gun.Clip == 0))
        {
            for (var i = 0; i < self.grasps.Length; i++)
            {
                gun.TryShoot(self);
                if (gun.justShot)
                {
                    self.bodyChunks[0].vel -= throwDir.ToVector2() * 5f * gun.damageStat;
                    self.bodyChunks[1].vel += throwDir.ToVector2() * 3f * gun.damageStat;
                }

                (self.graphicsModule as PlayerGraphics)?.LookAtObject(gun);
            }
        } 
        else if (self.grasps[1]?.grabbed is Gun otherGun)
        {
            otherGun.fireDelay = 10;
        }

        if (thrownObject is Gun)
        {
            return;
        }
        
        orig(self, grasp, eu);
    }

    private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);
        
        // Gun aim
        for (var i = 0; i < self.grasps.Length; i++)
        {
            if (self.grasps[i]?.grabbed is Gun gun)
            {
                gun.aimDir = Custom.DirVec(Vector2.Lerp(self.bodyChunks[0].pos, self.bodyChunks[1].pos, .3f), gun.firstChunk.pos);
                gun.aimDir = (gun.aimDir + new Vector2(self.flipDirection * 1.55f, (i == 1 && self.grasps[0] != null) ? .35f : 0f)).normalized;
                if (gun.reloadTime > 0)
                {
                    gun.aimDir = (gun.aimDir - new Vector2(0, .5f)).normalized;
                }
                else if (self.input[0].thrw)
                {
                    var dirX = self.input[0].x;
                    if (self.input[0].x == 0)
                    {
                        dirX = self.flipDirection;
                    }

                    gun.aimDir = (gun.aimDir * .1f + new Vector2(dirX, self.input[0].y * .2f)).normalized;
                }

                if (Random.value < 0.02f)
                {
                    (self.graphicsModule as PlayerGraphics)?.LookAtObject(gun);
                }
            }
        }

        // Auto shooting
        if (self.input[0].thrw)
        {
            for (var i = 0; i < self.grasps.Length; i++)
            {
                if (self.grasps[i]?.grabbed is Gun gun && gun.automatic)
                {
                    var throwDir = new IntVector2(self.ThrowDirection, 0);

                    gun.TryShoot(self);
                    if (gun.justShot)
                    {
                        self.bodyChunks[0].vel -= throwDir.ToVector2() * 5f * gun.damageStat;
                        self.bodyChunks[1].vel += throwDir.ToVector2() * 3f * gun.damageStat;
                    }

                    (self.graphicsModule as PlayerGraphics)?.LookAtObject(gun);
                }
            }
        }
    }
}