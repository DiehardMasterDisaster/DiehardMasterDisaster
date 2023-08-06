using Menu;
using RWCustom;
using UnityEngine;

namespace DiehardMasterDisaster.ScoreScreen;

public static class ScoreScreenHooks
{
    public static bool ActuallyShowSleepScreen;
    
    public static void Apply()
    {
        On.ProcessManager.PostSwitchMainProcess += ProcessManager_PostSwitchMainProcess;
        On.RainWorldGame.Win += RainWorldGame_Win;
        On.RainWorldGame.CommunicateWithUpcomingProcess += RainWorldGame_CommunicateWithUpcomingProcess;
    }

    private static void RainWorldGame_CommunicateWithUpcomingProcess(On.RainWorldGame.orig_CommunicateWithUpcomingProcess orig, RainWorldGame self, MainLoopProcess nextProcess)
    {
        orig(self, nextProcess);

        if (nextProcess is not DMDScoreScreen scoreScreen) return;

        int karma = self.GetStorySession.saveState.deathPersistentSaveData.karma;
        
        if (self.sawAGhost != null)
        {
            Debug.Log("Ghost end of process stuff");
            self.manager.CueAchievement(GhostWorldPresence.PassageAchievementID(self.sawAGhost), 2f);
            if (self.GetStorySession.saveState.deathPersistentSaveData.karmaCap == 8)
            {
                self.manager.CueAchievement(RainWorld.AchievementID.AllGhostsEncountered, 10f);
            }
            self.GetStorySession.saveState.GhostEncounter(self.sawAGhost, self.rainWorld);
        }
        int num = karma;
        if (nextProcess.ID == ProcessManager.ProcessID.DeathScreen && !self.GetStorySession.saveState.deathPersistentSaveData.reinforcedKarma)
        {
            num = Custom.IntClamp(num - 1, 0, self.GetStorySession.saveState.deathPersistentSaveData.karmaCap);
        }
        Debug.Log("next screen MAP KARMA: " + num);
        if (self.cameras[0].hud != null)
        {
            self.cameras[0].hud.map.mapData.UpdateData(self.world, 1 + self.GetStorySession.saveState.deathPersistentSaveData.foodReplenishBonus, num, self.GetStorySession.saveState.deathPersistentSaveData.karmaFlowerPosition, putItemsInShelters: true);
        }
        AbstractCreature abstractCreature = self.FirstAlivePlayer;
        if (abstractCreature == null)
        {
            abstractCreature = self.FirstAnyPlayer;
        }
        int num2 = -1;
        Vector2 vector = Vector2.zero;
        if (abstractCreature != null)
        {
            num2 = abstractCreature.pos.room;
            vector = abstractCreature.pos.Tile.ToVector2() * 20f;
            if (nextProcess.ID == ProcessManager.ProcessID.DeathScreen && self.cameras[0].hud != null && self.cameras[0].hud.textPrompt != null)
            {
                num2 = self.cameras[0].hud.textPrompt.deathRoom;
                vector = self.cameras[0].hud.textPrompt.deathPos;
            }
            else if (abstractCreature.realizedCreature != null)
            {
                vector = abstractCreature.realizedCreature.mainBodyChunk.pos;
            }
            if (abstractCreature.realizedCreature != null && abstractCreature.realizedCreature.room != null && num2 == abstractCreature.realizedCreature.room.abstractRoom.index)
            {
                vector = Custom.RestrictInRect(vector, abstractCreature.realizedCreature.room.RoomRect.Grow(50f));
            }
        }        
        
        var sleepDeathScreenDataPackage = new KarmaLadderScreen.SleepDeathScreenDataPackage((nextProcess.ID == ProcessManager.ProcessID.SleepScreen || nextProcess.ID == ProcessManager.ProcessID.Dream) ? self.GetStorySession.saveState.food : self.cameras[0].hud.textPrompt.foodInStomach, new IntVector2(karma, self.GetStorySession.saveState.deathPersistentSaveData.karmaCap), self.GetStorySession.saveState.deathPersistentSaveData.reinforcedKarma, num2, vector, self.cameras[0].hud.map.mapData, self.GetStorySession.saveState, self.GetStorySession.characterStats, self.GetStorySession.playerSessionRecords[0], self.GetStorySession.saveState.lastMalnourished, self.GetStorySession.saveState.malnourished);
        if (ModManager.CoopAvailable)
        {
            for (int i = 1; i < self.GetStorySession.playerSessionRecords.Length; i++)
            {
                if (self.GetStorySession.playerSessionRecords[i].kills != null && self.GetStorySession.playerSessionRecords[i].kills.Count > 0)
                {
                    sleepDeathScreenDataPackage.sessionRecord.kills.AddRange(self.GetStorySession.playerSessionRecords[i].kills);
                }
            }
        }

        scoreScreen.SleepPackage = sleepDeathScreenDataPackage;
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