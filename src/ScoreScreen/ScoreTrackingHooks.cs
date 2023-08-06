using System.Linq;
using Menu;
using MoreSlugcats;
using UnityEngine;

namespace DiehardMasterDisaster.ScoreScreen;

//-- Stolen with love and adapted from https://github.com/Dual-Iron/score-galore
public static class ScoreTrackingHooks
{
    // -- Vanilla --
    // Food             +1
    // Survived cycle   +10
    // Died in cycle    -3
    // Quit cycle       -3
    // Minute passed    -1
    // Hunter payload   +100
    // Hunter 5P        +40
    // Ascending        +300
    // -- MSC exclusive --
    // Meeting LttM     +40
    // Meeting 5P       +40
    // Pearl read       +20
    // Gourmand quest   +300
    // Sleep w/friend   +15
    
    public static void Apply()
    {
        // -- Real-time score tracking --
        On.HUD.HUD.InitSinglePlayerHud += HUD_InitSinglePlayerHud;

        // Track killing, eating, vomiting, passage of time, friends
        On.SocialEventRecognizer.Killing += CountKills;
        On.Player.AddFood += CountEat;
        On.Player.SubtractFood += CountVomit;
        On.StoryGameSession.TimeTick += CountTime;
        On.SaveState.SessionEnded += CountFriendsSaved;
        On.Oracle.Update += CountMoonand5P;
        On.SLOracleWakeUpProcedure.Update += CountReviveMoon;
        On.SLOracleBehaviorHasMark.GrabObject += CountPearl;
        On.SSOracleBehavior.StartItemConversation += CountPearl5P;
    }

    private static int CurrentCycleScore;

    static int currentCycleTime;

    static SlugcatStats.Name viewStats;

    private static int[] killScores;
    private static int[] KillScores()
    {
        if (killScores == null || killScores.Length != ExtEnum<MultiplayerUnlocks.SandboxUnlockID>.values.Count) {
            killScores = new int[ExtEnum<MultiplayerUnlocks.SandboxUnlockID>.values.Count];
            for (int i = 0; i < killScores.Length; i++) {
                killScores[i] = 1;
            }
            SandboxSettingsInterface.DefaultKillScores(ref killScores);
            killScores[(int)MultiplayerUnlocks.SandboxUnlockID.Slugcat] = 1;
        }
        return killScores;
    }

    private static int KillScore(IconSymbol.IconSymbolData iconData)
    {
        if (!CreatureSymbol.DoesCreatureEarnATrophy(iconData.critType)) {
            return 0;
        }

        var score = StoryGameStatisticsScreen.GetNonSandboxKillscore(iconData.critType);
        if (score == 0 && MultiplayerUnlocks.SandboxUnlockForSymbolData(iconData) is MultiplayerUnlocks.SandboxUnlockID unlockID) {
            score = KillScores()[unlockID.Index];
        }

        return score;
    }

    private static void AddCurrentCycleScore(RainWorldGame game, int score, IconSymbol.IconSymbolData icon, int delay, Color? forcedColor = null)
    {
        if (score == 0) return;

        CurrentCycleScore += score;
    }

    private static void AddCurrentCycleScore(int score)
    {
        if (score == 0) return;

        CurrentCycleScore += score;
    }

    private static int MSC(int score) => ModManager.MSC ? score : 0;

    private static int GetTotalScore(SaveState s)
    {
        if (s == null) {
            return 0;
        }

        var d = s.deathPersistentSaveData;
        var red = s.saveStateNumber == SlugcatStats.Name.Red;
        var arti = s.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Artificer;

        int vanilla = s.totFood + d.survives * 10 + s.kills.Sum(kvp => KillScore(kvp.Key) * kvp.Value)
            - (d.deaths * 3 + d.quits * 3 + s.totTime / 60)
            + (d.ascended ? 300 : 0)
            + (s.miscWorldSaveData.moonRevived ? 100 : 0)
            + (s.miscWorldSaveData.pebblesSeenGreenNeuron ? 40 : 0);

        int msc = (!arti ? d.friendsSaved * 15 : 0)
            + (!red ? s.miscWorldSaveData.SLOracleState.significantPearls.Count * 20 : 0)
            + (!red && !arti && s.miscWorldSaveData.SSaiConversationsHad > 0 ? 40 : 0)
            + (!red && !arti && s.miscWorldSaveData.SLOracleState.playerEncounters > 0 ? 40 : 0)
            + (d.winState.GetTracker(MoreSlugcatsEnums.EndgameID.Gourmand, false) is WinState.GourFeastTracker { GoalFullfilled: true } ? 300 : 0);

        return vanilla + MSC(msc);
    }

    private static void HUD_InitSinglePlayerHud(On.HUD.HUD.orig_InitSinglePlayerHud orig, global::HUD.HUD self, RoomCamera cam)
    {
        orig(self, cam);

        if (self.rainWorld.processManager.currentMainLoop is RainWorldGame game && game.IsStorySession) {
            CurrentCycleScore = 10;
        }
    }

    private static void CountKills(On.SocialEventRecognizer.orig_Killing orig, SocialEventRecognizer self, Creature killer, Creature victim)
    {
        orig(self, killer, victim);

        if (killer is Player && self.room.game.IsStorySession) {
            IconSymbol.IconSymbolData icon = CreatureSymbol.SymbolDataFromCreature(victim.abstractCreature);

            AddCurrentCycleScore(self.room.game, KillScore(icon), icon, 0);
        }
    }

    private static void CountEat(On.Player.orig_AddFood orig, Player self, int add)
    {
        if (self.abstractCreature.world.game.session is not StoryGameSession story) {
            orig(self, add);
            return;
        }

        int before = story.saveState.totFood;

        orig(self, add);

        int after = story.saveState.totFood;

        AddCurrentCycleScore(after - before);
    }

    private static void CountVomit(On.Player.orig_SubtractFood orig, Player self, int sub)
    {
        if (self.abstractCreature.world.game.session is StoryGameSession story) {
            int before = story.saveState.totFood;
            orig(self, sub);
            int after = story.saveState.totFood;

            AddCurrentCycleScore(after - before);
        }
        else {
            orig(self, sub);
        }
    }

    private static void CountTime(On.StoryGameSession.orig_TimeTick orig, StoryGameSession self, float dt)
    {
        orig(self, dt);

        if (self.playerSessionRecords != null && self.playerSessionRecords.Length > 0 && self.playerSessionRecords[0] != null) {
            int minute = self.playerSessionRecords[0].time / 2400;

            if (currentCycleTime < minute) {
                AddCurrentCycleScore(currentCycleTime - minute);
                currentCycleTime = minute;
            }
        }
    }

    private static void CountFriendsSaved(On.SaveState.orig_SessionEnded orig, SaveState self, RainWorldGame game, bool survived, bool newMalnourished)
    {
        int friendsSavedBefore = self.deathPersistentSaveData.friendsSaved;

        orig(self, game, survived, newMalnourished);

        // "Friends sheltered"
        if (self.saveStateNumber != MoreSlugcatsEnums.SlugcatStatsName.Artificer) {
            CurrentCycleScore += 15 * MSC(self.deathPersistentSaveData.friendsSaved - friendsSavedBefore);
        }
    }

    private static void CountMoonand5P(On.Oracle.orig_Update orig, Oracle self, bool eu)
    {
        MiscWorldSaveData m = self.room.game.GetStorySession?.saveState?.miscWorldSaveData;

        if (m == null) {
            orig(self, eu);
            return;
        }

        bool sawNeuron = m.pebblesSeenGreenNeuron;
        int before5P = m.SSaiConversationsHad;
        int beforeLttM = m.SLOracleState.playerEncounters;

        orig(self, eu);

        // "Met Looks to the Moon" and "Met Five Pebbles"
        if (self.room.game.StoryCharacter != SlugcatStats.Name.Red && self.room.game.StoryCharacter != MoreSlugcatsEnums.SlugcatStatsName.Artificer) {
            if (before5P == 0 && m.SSaiConversationsHad > 0) {
                AddCurrentCycleScore(MSC(40));
            }
            if (beforeLttM == 0 && m.SLOracleState.playerEncounters > 0) {
                AddCurrentCycleScore(MSC(40));
            }
        }
        // "Helped Five Pebbles"
        if (!sawNeuron && m.pebblesSeenGreenNeuron) {
            AddCurrentCycleScore(40);
        }
    }

    private static void CountReviveMoon(On.SLOracleWakeUpProcedure.orig_Update orig, SLOracleWakeUpProcedure self, bool eu)
    {
        MiscWorldSaveData m = self.room.game.GetStorySession?.saveState?.miscWorldSaveData;

        if (m == null) {
            orig(self, eu);
            return;
        }
        bool revived = m.moonRevived;

        orig(self, eu);

        // "Delivered Payload"
        if (!revived && m.moonRevived) {
            AddCurrentCycleScore(100);
        }
    }

    private static void CountPearl(On.SLOracleBehaviorHasMark.orig_GrabObject orig, SLOracleBehaviorHasMark self, PhysicalObject item)
    {
        bool read = item is DataPearl p && self.State.significantPearls.Contains(p.AbstractPearl.dataPearlType);

        orig(self, item);

        // "Unique pearls read"
        if (!read && item is DataPearl pearl && self.State.significantPearls.Contains(pearl.AbstractPearl.dataPearlType) && self.oracle.room.game.StoryCharacter != SlugcatStats.Name.Red) {
            AddCurrentCycleScore(MSC(20));
        }
    }

    private static void CountPearl5P(On.SSOracleBehavior.orig_StartItemConversation orig, SSOracleBehavior self, DataPearl item)
    {
        var state = self.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.SLOracleState;

        bool read = item is DataPearl p && state.significantPearls.Contains(p.AbstractPearl.dataPearlType);

        orig(self, item);

        // "Unique pearls read" for arti
        if (!read && item is DataPearl pearl && state.significantPearls.Contains(pearl.AbstractPearl.dataPearlType)) {
            AddCurrentCycleScore(MSC(20));
        }
    }

    public static ScorePackage GetScore(SaveState saveState)
    {
        int current = CurrentCycleScore;
        int total = GetTotalScore(saveState);
        if (saveState.malnourished) {
            // Deaths are always -3. Time during failed cycles doesn't counted.
            current = -3;
        }

        return new ScorePackage { current = current, total = total};
    }

    public struct ScorePackage
    {
        public int current;
        public int total;
    }
}