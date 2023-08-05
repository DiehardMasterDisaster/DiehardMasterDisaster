using DiehardMasterDisaster.CoolnessSystem;

namespace DiehardMasterDisaster.HUD;

public static class CoolnessHooks
{
    public static void Apply()
    {
        On.RainWorldGame.ctor += RainWorldGame_ctor;
        On.SocialEventRecognizer.Killing += SocialEventRecognizer_Killing;
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