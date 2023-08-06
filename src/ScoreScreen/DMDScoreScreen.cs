using System.Collections.Generic;
using Menu;
using RWCustom;
using UnityEngine;

namespace DiehardMasterDisaster.ScoreScreen;

public class DMDScoreScreen : Menu.Menu
{
    public static float LastSessionTimer;
    public KarmaLadderScreen.SleepDeathScreenDataPackage SleepPackage;

    private PlayerSessionRecord record;
    private FSprite backgroundSprite;
    private float[] offsets;
    private Queue<ScoreLabel> scoreLabels;
    private List<PlayerSessionRecord.KillRecord> kills;
    
    private float leftAnchor;
    private float rightAnchor;
    private float bottomAnchor;
    private float topAnchor;
    private float hCenterAnchor;
    private float vCenterAnchor;

    private const int InitialScoreDelay = 120;
    private const int ScoreDelay = 50;
    private const int ScoreYOffset = 65;
    
    private float scoreYPos;
    private int timer;
    
    private static readonly Color blueColor = new(144f / 255, 135f / 255, 175f / 255);
    private static readonly Color redColor = new(233f / 255, 30f / 255, 66f / 255);

    public DMDScoreScreen(ProcessManager manager, StoryGameSession session) : base(manager, DiehardEnums.Process.DMDScoreScreen)
    {
        record = session.playerSessionRecords[0];
        offsets = Custom.GetScreenOffsets();
        scoreLabels = new();
        kills = new();

        leftAnchor = offsets[0];
        rightAnchor = offsets[1];
        bottomAnchor = 0;
        topAnchor = 768;
        hCenterAnchor = (leftAnchor + rightAnchor) / 2;
        vCenterAnchor = (bottomAnchor + topAnchor) / 2;

        scoreYPos = topAnchor - 300;
    
        foreach (var rec in session.playerSessionRecords)
        {
            kills.AddRange(rec.kills);
        }
        
        manager.musicPlayer.MenuRequestsSong(DiehardEnums.Song.DMDScoreScreen, 1f, 1f);
        manager.musicPlayer.song.Loop = true;

        pages.Add(new Page(this, null, "main", 0));

        scene = new InteractiveMenuScene(this, pages[0], manager.rainWorld.options.subBackground);
        pages[0].subObjects.Add(scene);
        
        var continueButton = new SimpleButton(this, pages[0], Translate("CONTINUE"), "CONTINUE", new Vector2((manager.rainWorld.options.ScreenSize.x + (1366f - manager.rainWorld.options.ScreenSize.x) / 2f) - 180f - manager.rainWorld.options.SafeScreenOffset.x, Mathf.Max(manager.rainWorld.options.SafeScreenOffset.y, 15f)), new Vector2(110f, 30f));
        pages[0].subObjects.Add(continueButton);
        pages[0].lastSelectedObject = continueButton;

        using (new CustomFont(CustomFont.DukeNukem3DFont2x))
        {
            var label = new MenuLabel(this, pages[0], $"CYCLE {session.saveState.cycleNumber}", new Vector2(hCenterAnchor - 100, topAnchor - 120), new Vector2(200, 200), false);
            label.label.color = redColor;
            pages[0].subObjects.Add(label);
            label = new MenuLabel(this, pages[0], "COMPLETED", new Vector2(hCenterAnchor - 100, topAnchor - 195), new Vector2(200, 200), false);
            label.label.color = redColor;
            pages[0].subObjects.Add(label);
        }

        var minutes = Mathf.FloorToInt(((LastSessionTimer) / 40f) / 60);
        var seconds = Mathf.FloorToInt(((LastSessionTimer) / 40f) % 60);

        var minutesLeft = Mathf.FloorToInt(((session.game.world.rainCycle.cycleLength - session.game.world.rainCycle.timer) / 40f) / 60);
        var secondsLeft = Mathf.FloorToInt(((session.game.world.rainCycle.cycleLength - session.game.world.rainCycle.timer) / 40f) % 60);

        var score = ScoreTrackingHooks.GetScore(session.saveState);

        scoreLabels.Enqueue(new("YOUR TIME", $"{minutes:00}.{seconds:00}"));        
        scoreLabels.Enqueue(new("TIME LEFT", $"{minutesLeft:00}.{secondsLeft:00}"));
        scoreLabels.Enqueue(new("ENEMIES KILLED", record.kills.Count.ToString()));
        scoreLabels.Enqueue(new("CYCLE SCORE", score.current.ToString()));
        scoreLabels.Enqueue(new("TOTAL SCORE", score.total.ToString()));
    }

    public override void Singal(MenuObject sender, string message)
    {
        if (message == "CONTINUE")
        {
            manager.RequestMainProcessSwitch(ProcessManager.ProcessID.SleepScreen);
        }
    }

    public override void Update()
    {
        base.Update();
        timer++;

        using (new CustomFont(CustomFont.DukeNukem3DFont1x))
        {
            if ((timer - InitialScoreDelay) % ScoreDelay == 0 && scoreLabels.Count > 0)
            {
                PlaySound(DiehardEnums.Sound.DMDScoreScreenBoom);
                
                var scoreLabel = scoreLabels.Dequeue();
                var label = new MenuLabel(this, pages[0], scoreLabel.title, new Vector2(leftAnchor + 50, scoreYPos), new Vector2(100f, 100f), false);
                label.label.color = blueColor;
                label.label.alignment = FLabelAlignment.Left;
                pages[0].subObjects.Add(label);

                label = new MenuLabel(this, pages[0], scoreLabel.value, new Vector2(leftAnchor + 750, scoreYPos), new Vector2(100f, 100f), false);
                label.label.color = blueColor;
                label.label.alignment = FLabelAlignment.Right;
                pages[0].subObjects.Add(label);

                scoreYPos -= ScoreYOffset;
            }
        }
    }

    public override void CommunicateWithUpcomingProcess(MainLoopProcess nextProcess)
    {
        base.CommunicateWithUpcomingProcess(nextProcess);

        if (nextProcess is KarmaLadderScreen karmaLadderScreen)
        {
            karmaLadderScreen.GetDataFromGame(SleepPackage);
        }
    }

    public class ScoreLabel
    {
        public string title;
        public string value;
        
        public ScoreLabel(string title, string value)
        {
            this.title = title;
            this.value = value;
        }
    }
}