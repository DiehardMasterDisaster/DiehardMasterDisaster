using System.Linq;
using DiehardMasterDisaster.Fisobs;
using DiehardMasterDisaster.HUD;
using RWCustom;
using UnityEngine;

namespace DiehardMasterDisaster.GunStuff;

public static class GunHooks
{
    static bool wasPressed;

    public static void Apply()
    {
        On.Player.Update += Player_Update;
        On.Player.ThrowObject += Player_ThrowObject;
        On.Player.ReleaseGrasp += Player_ReleaseGrasp;
        On.Player.SlugcatGrab += Player_SlugcatGrab;
        On.Player.Update += OnPlayer_Update;
    }

    private static void Player_ReleaseGrasp(On.Player.orig_ReleaseGrasp orig, Player self, int grasp)
    {
        var gun = self.grasps[grasp]?.grabbed as Gun;

        orig(self, grasp);
        
        if (gun != null && self.GetDMD().IsDMD)
        {
            self.GetDMD().StoreGun(gun);
        }
    }

    private static void Player_SlugcatGrab(On.Player.orig_SlugcatGrab orig, Player self, PhysicalObject obj, int graspUsed)
    {
        orig(self, obj, graspUsed);

        if (self.grasps[graspUsed]?.grabbed is Gun gun && self.GetDMD().IsDMD && !self.GetDMD().ActuallyEquipGun)
        {
            self.GetDMD().StoreGun(gun);
            if (self.grasps[graspUsed == 0 ? 1 : 0]?.grabbed is not Gun)
            {
                self.GetDMD().EquipGun(gun.abstractGun.type);
            }
        }
    }

    private static void OnPlayer_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);

        var dmd = self.GetDMD();
        if (!dmd.IsDMD) return;
        
        //-- TODO: Proper keybinds
        var prevPressed = Input.GetKey(KeyCode.KeypadDivide);
        var nextPressed = Input.GetKey(KeyCode.KeypadMultiply);
        if ((prevPressed && !dmd.SwapWeaponPrevKey) || nextPressed && !dmd.SwapWeaponNextKey)
        {
            Gun currentWeapon = null;
            foreach (var grasp in self.grasps)
            {
                if (grasp?.grabbed is Gun graspedGun)
                {
                    currentWeapon = graspedGun;
                    break;
                }
            }

            var nextWeapon = dmd.HUD.GetRelativeWeapon(currentWeapon?.abstractGun, prevPressed ? -1 : 1);
            if (nextWeapon != null)
            {
                if (currentWeapon != null)
                {
                    dmd.StoreGun(currentWeapon);
                } 
                dmd.EquipGun(nextWeapon.type);
            }

            if (prevPressed)
            {
                dmd.SwapWeaponPrevKey = true;
            }

            if (nextPressed)
            {
                dmd.SwapWeaponNextKey = true;
            }
        }
        else
        {
            if (!prevPressed)
            {
                dmd.SwapWeaponPrevKey = false;
            }

            if (!nextPressed)
            {
                dmd.SwapWeaponNextKey = false;
            }
        }

        //-- TODO: Should be removed when releasing or only enabled if a file is present or something
        TestStuff(self);
    }

    private static void TestStuff(Player self)
    {
        if (Input.GetKey(KeyCode.KeypadMinus))
        {
            AbstractGun gun = null;
            if (Input.GetKey(KeyCode.Keypad7) && !wasPressed)
            {
                gun = new AbstractGun(self.room.world, DiehardEnums.AbstractObject.DMDMinigun, new WorldCoordinate(self.room.abstractRoom.index, 0, 0, -1), self.room.game.GetNewID());
            }
            else if (Input.GetKey(KeyCode.Keypad8) && !wasPressed)
            {
                gun = new AbstractGun(self.room.world, DiehardEnums.AbstractObject.DMDBFGGun, new WorldCoordinate(self.room.abstractRoom.index, 0, 0, -1), self.room.game.GetNewID());
            }
            else if (Input.GetKey(KeyCode.Keypad9) && !wasPressed)
            {
                gun = new AbstractGun(self.room.world, DiehardEnums.AbstractObject.DMDDerringerGun, new WorldCoordinate(self.room.abstractRoom.index, 0, 0, -1), self.room.game.GetNewID());
            }
            else if (Input.GetKey(KeyCode.Keypad4) && !wasPressed)
            {
                gun = new AbstractGun(self.room.world, DiehardEnums.AbstractObject.DMDShotgun, new WorldCoordinate(self.room.abstractRoom.index, 0, 0, -1), self.room.game.GetNewID());
            }
            else if (Input.GetKey(KeyCode.Keypad5) && !wasPressed)
            {
                gun = new AbstractGun(self.room.world, DiehardEnums.AbstractObject.DMDGrenadeLauncherGun, new WorldCoordinate(self.room.abstractRoom.index, 0, 0, -1), self.room.game.GetNewID());
            }
            else if (Input.GetKey(KeyCode.Keypad6) && !wasPressed)
            {
                gun = new AbstractGun(self.room.world, DiehardEnums.AbstractObject.DMDAK47Gun, new WorldCoordinate(self.room.abstractRoom.index, 0, 0, -1), self.room.game.GetNewID());
            }
            else if (!(Input.GetKey(KeyCode.Keypad7) || Input.GetKey(KeyCode.Keypad8) || Input.GetKey(KeyCode.Keypad9) || Input.GetKey(KeyCode.Keypad4) || Input.GetKey(KeyCode.Keypad5) || Input.GetKey(KeyCode.Keypad6)))
            {
                wasPressed = false;
            }

            if (gun != null)
            {
                wasPressed = true;

                gun.CurrentAmmo = gun.ClipSize;
                gun.RealizeInRoom();
                gun.realizedObject.bodyChunks[0].HardSetPosition(self.mainBodyChunk.pos);

                self.GetDMD().AddAmmo(gun.AmmoType, Mathf.CeilToInt(gun.AmmoType.AmmoStorage / 5f));
                ((WeaponsHUD)self.room.game.cameras.FirstOrDefault().hud.parts.FirstOrDefault(x => x is WeaponsHUD)).Show();
            }
        }
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