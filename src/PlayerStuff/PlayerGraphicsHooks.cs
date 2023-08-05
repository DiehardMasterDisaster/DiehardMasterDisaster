using DiehardMasterDisaster.GunStuff;
using UnityEngine;

namespace DiehardMasterDisaster.PlayerStuff;

public static class PlayerGraphicsHooks
{
    private const string SpritePrefix = "DMD_4sc_";
    
    public static void Apply()
    {
        On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
        On.PlayerGraphics.ApplyPalette += PlayerGraphics_ApplyPalette;
        On.SlugcatHand.Update += SlugcatHand_Update;
    }

    private static void SlugcatHand_Update(On.SlugcatHand.orig_Update orig, SlugcatHand self)
    {
        if (self.owner.owner is not Player player) return;

        var flipStuff = false;
        if (player.grasps[self.limbNumber]?.grabbed is Gun)
        {
            if (self.limbNumber == 0 && player.flipDirection == 1 || self.limbNumber == 1 && player.flipDirection == -1)
            {
                flipStuff = true;
            }
        }
        if (flipStuff)
        {
            self.limbNumber = self.limbNumber == 1 ? 0 : 1;
            (player.grasps[0], player.grasps[1]) = (player.grasps[1], player.grasps[0]);
        }
        orig.Invoke(self);
        if (flipStuff)
        {
            self.limbNumber = self.limbNumber == 1 ? 0 : 1;
            (player.grasps[0], player.grasps[1]) = (player.grasps[1], player.grasps[0]);
        }
    }

    private static void PlayerGraphics_ApplyPalette(On.PlayerGraphics.orig_ApplyPalette orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);

        sLeaser.sprites[2].color = new Color(50 / 255f, 25 / 255f, 60 / 255f);
    }

    private static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (self.owner is not Player player || !player.GetDMD().IsDMD) return;

        foreach (var sprite in sLeaser.sprites)
        {
            if (Futile.atlasManager._allElementsByName.TryGetValue(SpritePrefix + sprite.element.name, out var element))
            {
                sprite.element = element;
            }
        }
    }
}