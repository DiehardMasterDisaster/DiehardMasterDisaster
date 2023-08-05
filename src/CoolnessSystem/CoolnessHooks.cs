using System;
using System.Globalization;
using MonoMod.RuntimeDetour;

namespace DiehardMasterDisaster.CoolnessSystem;

public static class CoolnessHooks
{
    public static void Apply()
    {
        On.RainWorldGame.ctor += RainWorldGame_ctor;
        On.SocialEventRecognizer.Killing += SocialEventRecognizer_Killing;
        On.HUD.KarmaMeter.Draw += KarmaMeter_Draw;
        _ = new Hook(typeof(RegionGate).GetProperty(nameof(RegionGate.MeetRequirement))!.GetGetMethod(), RegionGate_MeetRequirement_Get);
    }

    private static bool RegionGate_MeetRequirement_Get(Func<RegionGate, bool> orig, RegionGate self)
    {
        var result = orig(self);
        if (!self.room.game.IsDMD()) return result;

        int.TryParse(self.karmaRequirements[!self.letThroughDir ? 1 : 0].value, NumberStyles.Any, CultureInfo.InvariantCulture, out var requirement);
        
        return CoolnessTracker.KarmaLevel > requirement;
    }

    private static void KarmaMeter_Draw(On.HUD.KarmaMeter.orig_Draw orig, global::HUD.KarmaMeter self, float timeStacker)
    {
        orig(self, timeStacker);

        if (self.hud.rainWorld.processManager.currentMainLoop is not RainWorldGame game || !game.IsDMD()) return;

        self.karmaSprite.element = Futile.atlasManager.GetElementWithName("sprites/DMDCoolness" + CoolnessTracker.CurrentLevel);
        self.lastFade = 1;
        self.fade = 1;
        self.glowSprite.alpha = 0;
    }

    private static void SocialEventRecognizer_Killing(On.SocialEventRecognizer.orig_Killing orig, SocialEventRecognizer self, Creature killer, Creature victim)
    {
        orig(self, killer, victim);

        if (killer is not Player player || !player.IsDMD(out _)) return;
        
        CoolnessTracker.AddKill(victim);
    }

    private static void RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
    {
        orig(self, manager);
        CoolnessTracker.Reset();
    }
}