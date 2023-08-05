using System.Linq;

namespace DiehardMasterDisaster.HUD;

public static class HUDHooks
{
    public static void Apply()
    {
        On.Player.Update += Player_Update;
    }

    private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self,eu);
        if (self.isNPC || !self.IsDMD(out var dmd) || dmd.HUD != null) return;
        
        //-- TODO: Splitscreen support
        var hud = self.abstractCreature.world.game.cameras.FirstOrDefault()?.hud;
        if (hud != null)
        {
            var dmdHUD = new WeaponsHUD(hud, self);
            hud.AddPart(dmd.HUD = dmdHUD);
        }
    }
}