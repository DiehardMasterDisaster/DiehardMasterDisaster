using DiehardMasterDisaster.Fisobs;
using DiehardMasterDisaster.GunStuff;
using RWCustom;
using UnityEngine;

namespace DiehardMasterDisaster.Hooks;

public static class PlayerGraphicsHooks
{
    private const string SpritePrefix = "DMD_4sc_";
    
    public static void Apply()
    {
        On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
        On.PlayerGraphics.ApplyPalette += PlayerGraphics_ApplyPalette;
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