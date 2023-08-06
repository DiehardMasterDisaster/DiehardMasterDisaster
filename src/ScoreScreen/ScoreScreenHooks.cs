namespace DiehardMasterDisaster.ScoreScreen;

public static class ScoreScreenHooks
{
    public static bool ActuallyShowSleepScreen;
    
    public static void Apply()
    {
        On.ProcessManager.PostSwitchMainProcess += ProcessManager_PostSwitchMainProcess;
        On.RainWorldGame.Win += RainWorldGame_Win;
    }

    private static void RainWorldGame_Win(On.RainWorldGame.orig_Win orig, RainWorldGame self, bool malnourished)
    {
        if (self.IsDMD() && self.session is StoryGameSession session && session.playerSessionRecords[0].time > 0)
        {
            DMDScoreScreen.LastSessionTimer = session.playerSessionRecords[0].time + session.playerSessionRecords[0].playerGrabbedTime;
        }
        
        orig(self, malnourished);
    }

    private static void ProcessManager_PostSwitchMainProcess(On.ProcessManager.orig_PostSwitchMainProcess orig, ProcessManager self, ProcessManager.ProcessID ID)
    {
	    if (!ActuallyShowSleepScreen && (ID == ProcessManager.ProcessID.SleepScreen || ID == ProcessManager.ProcessID.StarveScreen) && self.oldProcess is RainWorldGame game && game.IsDMD())
        {
            ID = DiehardEnums.Process.DMDScoreScreen;
            self.currentMainLoop = new DMDScoreScreen(self, game.GetStorySession);
        }
        
        orig(self, ID);
    }
}