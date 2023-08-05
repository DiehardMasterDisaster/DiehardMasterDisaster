namespace DiehardMasterDisaster.SaveData;

public static class SaveDataHooks
{
    public static void Apply()
    {
        On.RainWorldGame.Win += RainWorldGame_Win;
    }

    private static void RainWorldGame_Win(On.RainWorldGame.orig_Win orig, RainWorldGame self, bool malnourished)
    {
        if (self.session is StoryGameSession)
        {
            foreach (var absPlayer in self.Players)
            {
                if (absPlayer.realizedCreature is Player player && player.GetDMD().IsDMD)
                {
                    SaveUtils.SaveGuns(player.GetDMD());
                    SaveUtils.SaveAmmo(player.GetDMD());
                }
            }
        }

        orig(self, malnourished);
    }
}