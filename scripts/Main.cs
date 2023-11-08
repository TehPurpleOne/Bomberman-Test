using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

public class Main : Node2D
{
    private Global g;
    public TileMap background;
    private TileMap breakables;
    private TileMap items;
    private Player p;

    public Dictionary<Vector2, int> backgroundTiles = new Dictionary<Vector2, int>();
    public Dictionary<Vector2, int> activeBreakables = new Dictionary<Vector2, int>();
    private Dictionary<Vector2, int> activeItems = new Dictionary<Vector2, int>();
    public Dictionary<Vector2, Bomb> activeBombs = new Dictionary<Vector2, Bomb>();
    public Dictionary<Vector2, Explosion> activeExplosions = new Dictionary<Vector2, Explosion>();
    public List<Enemy> activeEnemies = new List<Enemy>();

    private AudioStreamPlayer activeSong;
    
    private bool introDone = false;
    private Vector2 lastTilePos = Vector2.Zero;
    private Vector2 currentTilePos = Vector2.Zero;

    public enum states {NULL, INIT, START, RUN, CLEAR};
    public states state = states.NULL;
    public states previousState = states.NULL;

    public override void _Ready() {
        g = (Global)GetNode("/root/Global");
        p = (Player)GetNode("Actor/Players/Player");
        background = (TileMap)GetNode("Background/BackgroundMap");
        breakables = (TileMap)GetNode("Breakable/BreakableMap");
        items = (TileMap)GetNode("Item/ItemMap");

        // Populate backgroundTiles. This is done here due to the background not changing during gameplay.
        for(int i = 0; i < background.GetUsedCells().Count; i++) {
            Vector2 tilePos = (Vector2)background.GetUsedCells()[i];
            int tileID = (int)background.GetCellv(tilePos);

            backgroundTiles.Add(tilePos, tileID);
        }

        setState(states.INIT);
    }

    public override void _PhysicsProcess(float delta) {
        if(state != states.NULL) { // If the main scene's states is different than NULL...
            stateLogic(delta); // Run state logic.
            states t = getTransition(delta); // Check for state transitions.
            if(t != states.NULL) { // Set the new state if transition is detected.
                setState(t);
            }
        }
    }
    
    private void stateLogic(float delta) {
        switch(state) {
            case states.START:
                if(activeSong.GetPlaybackPosition() >= 1.50f && !introDone) { // Create a second Tween to move the text offscreen.
                    SceneTreeTween tween = CreateTween();
                    Label stageStart = (Label)GetNode("UI/StageStart");
                    tween.TweenProperty(stageStart, "rect_position", new Vector2(-192, 104), 0.5f);
                    tween.SetTrans(Tween.TransitionType.Linear);
                    tween.SetEase(Tween.EaseType.In);
                    introDone = true;
                }
                break;
            
            case states.RUN:
                currentTilePos = background.WorldToMap(p.GlobalPosition);
                if(lastTilePos != currentTilePos) {
                    checkforItems(currentTilePos);
                }
                break;
            
            case states.CLEAR:
                if(activeSong.GetPlaybackPosition() >= 3.0f && !introDone) { // Create a second Tween to move the text offscreen.
                    SceneTreeTween tween = CreateTween();
                    Label stageClear = (Label)GetNode("UI/StageClear");
                    tween.TweenProperty(stageClear, "rect_position", new Vector2(-192, 104), 0.5f);
                    tween.SetTrans(Tween.TransitionType.Linear);
                    tween.SetEase(Tween.EaseType.In);
                    introDone = true;
                }

                if(!activeSong.Playing) {
                    restartStage(1);
                }
                break;
        }
    }

    private states getTransition(float delta) {
        switch(state) {
            case states.INIT:
                return states.START;
            
            case states.START:
                if(!activeSong.Playing) {
                    return states.RUN;
                }
                break;
            
            case states.RUN:
                if(activeEnemies.Count == 0) {
                    return states.CLEAR;
                }
                break;
        }

        return states.NULL;
    }

    private void enterState(states newState, states oldState) { 
        switch(newState) {
            case states.INIT:
                // The first thing we want to do is populate the map with the destrucable blocks.
                Random RNGesus = new Random();

                // Set the maximum amount of enemies to be spawned
                int maxEnemies = g.levelID + 3; // This will add an additional enemy depending on the level ID.
                maxEnemies = Mathf.Clamp(maxEnemies, maxEnemies, 8); // Set the maximum amount of enemies to spawn in. In this case, is 8.

                // Get the player's starting position and the distance of the new tile from the player.
                Vector2 playerStartPos = new Vector2((float)Math.Floor(p.GlobalPosition.x / background.CellSize.x), (float)Math.Floor(p.GlobalPosition.y / background.CellSize.y));

                for(int i = 0; i < background.GetUsedCells().Count; i++) { // Start the block population loop.
                    // Get the current game world's map. All tiles should be used to function properly.
                    Vector2 tilePos = (Vector2)background.GetUsedCells()[i];
                    int tileID = (int)background.GetCellv(tilePos);

                    // Next, we going to place breakable blocks at random locations since this is an example, but...
                    bool setBlock = Convert.ToBoolean(RNGesus.Next(0, 2));

                    // We need to ignore player starting positions, and any blocks that are not walkable.
                    float dist = tilePos.DistanceTo(playerStartPos);

                    if(tileID != 0 && tileID != 1 && setBlock && dist > 1) { // Everything checks out. Drop a block.
                        breakables.SetCellv(tilePos, 0);

                        // Add the tile's position to activeBreakables. This is so we can calculate explosions later.
                        activeBreakables.Add(tilePos, breakables.GetCellv(tilePos));

                        // Let's call RNGesus once more for items.
                        bool setItem = RNGesus.Next(0, 128) < 32;

                        if(setItem) { // Items spawn at a rate of about 25%. If successful, choose from a pool of available items and place them on the items tilemap.
                            int itemID = RNGesus.Next(0, 3);
                            items.SetCellv(tilePos, itemID);
                            activeItems.Add(tilePos, itemID);
                        }
                    }

                    // Now we're going to place enemies about the stage.
                    bool blockCheck = breakables.GetCellv(tilePos) == -1 && background.GetCellv(tilePos) == 2;

                    if(blockCheck && activeEnemies.Count < maxEnemies) { // If the maximum amount of enemies hasn't been hit, spawn a new one.
                        PackedScene loader = (PackedScene)ResourceLoader.Load("res://scenes/enemy/Pollun.tscn");
                        Enemy result = (Enemy)loader.Instance();
                        Control enemyLayer = (Control)GetNode("Actor/Enemies");

                        enemyLayer.AddChild(result);
                        activeEnemies.Add(result);
                    }
                }

                // Lets make a quick list of empty spots that enemies can start from while preventing them from spawning on top of the player.
                List<Vector2> emptySpots = new List<Vector2>();

                for(int i = 0; i < background.GetUsedCells().Count; i++) {
                    Vector2 tilePos = (Vector2)background.GetUsedCells()[i];
                    int tileID = (int)background.GetCellv(tilePos);

                    bool blockCheck = breakables.GetCellv(tilePos) == -1;

                    bool safeSpot = tileID == 2 
                    && tilePos != playerStartPos 
                    && tilePos != playerStartPos + Vector2.Up 
                    && tilePos != playerStartPos + Vector2.Down 
                    && tilePos != playerStartPos + Vector2.Left 
                    && tilePos != playerStartPos + Vector2.Right
                    && blockCheck;

                    if(safeSpot) {
                        emptySpots.Add(tilePos);
                    }
                }

                // Finally, let's place the available enemies in the safe spots.
                for(int i = 0; i < activeEnemies.Count; i++) {
                    int whichSpot = RNGesus.Next(0, emptySpots.Count); // Pick an location based on it's index number.

                    activeEnemies[i].GlobalPosition = background.MapToWorld(emptySpots[whichSpot]) + background.CellSize / 2; // Position the enemy.

                    emptySpots.Remove(emptySpots[i]); // Remove the used location to prevent two enemies from sharing the same space.
                }

                // With blocks, items, and enemies in place, we can begin  the stage!
                break;
            
            case states.START:
                playMusic("Intro"); // Play the intro music.

                SceneTreeTween tween = CreateTween(); // Create a Tween to display the Stage Start text.
                Label stageStart = (Label)GetNode("UI/StageStart");
                tween.TweenProperty(stageStart, "rect_position", new Vector2(64, 104), 0.5f);
                tween.SetTrans(Tween.TransitionType.Linear);
                tween.SetEase(Tween.EaseType.In);
                break;
            
            case states.RUN:
                playMusic("Stage"); // Play the stage music.
                break;
            
            case states.CLEAR:
                playMusic("Clear"); // Play the intro music.

                SceneTreeTween clearTween = CreateTween(); // Create a Tween to display the Stage Start text.
                Label stageClear = (Label)GetNode("UI/StageClear");
                clearTween.TweenProperty(stageClear, "rect_position", new Vector2(64, 104), 0.5f);
                clearTween.SetTrans(Tween.TransitionType.Linear);
                clearTween.SetEase(Tween.EaseType.In);
                break;
        }
    }

    private void exitState(states oldState, states newState) { 
        
    }

    private void setState(states newState) {
        introDone = false;
        previousState = state;
        state = newState;

        exitState(previousState, newState);

        enterState(newState, previousState);
    }

    private void playMusic(string name) {
        // Kill any current song that's playing. This is to prevent multiple songs from overlapping over one another.
        if(activeSong != null) {
            activeSong.Stop();
            activeSong = null;
        }

        Node songs = (Node)GetNode("AudioManager/Music");

        for(int i = 0; i < songs.GetChildCount(); i++) { // Find the song to be played if it exists, set it as the active song, then play.
            AudioStreamPlayer song = (AudioStreamPlayer)songs.GetChild(i);

            if(song.Name == name) {
                song.Play();
                activeSong = song;
            }
        }
    }

    private void playSound(string name) {
        Node sounds = (Node)GetNode("AudioManager/SoundEffects");

        for(int i = 0; i < sounds.GetChildCount(); i++) {
            AudioStreamPlayer sfx = (AudioStreamPlayer)sounds.GetChild(i);

            if(sfx.Name == name) {
                sfx.Play();
            }
        }
    }

    public Vector2 getDirection() { // Returns directional buttons held.
        Vector2 dirHold = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

        return dirHold;
    }

    public bool getButton() { // Returns if the bomb button is pressed or not.
        bool buttonTap = Input.IsActionJustPressed("ui_accept");

        return buttonTap;
    }

    public void setBomb() { // Spawn a bomb!
        if(p.poisonState == 2) { // Player cannot drop bombs in this state.
            return;
        }

        // First, get the player's tile position.
        Vector2 playerTilePos = background.WorldToMap(p.GlobalPosition);

        if(activeBombs.Count < g.maxBombs && !activeBombs.ContainsKey(playerTilePos)) { // Check to see if active bombs exceeds the maximum number of bombs allowed.
            playSound("PlaceBomb");
            PackedScene newBomb = (PackedScene)ResourceLoader.Load("res://scenes/objects/Bomb.tscn");
            Bomb result = (Bomb)newBomb.Instance();

            Control bombLayer = (Control)GetNode("Explosion");
            bombLayer.AddChild(result);

            result.GlobalPosition = background.MapToWorld(playerTilePos) + background.CellSize / 2;
            result.bombStrength = g.bombStrength;

            if(p.poisonState == 1) { // Bombs become pretty useless in this poisoned state.
                result.bombStrength = 1;
            }

            result.tilePos = playerTilePos;
            activeBombs.Add(playerTilePos, result);
        }
    }

    public void detonate(Bomb whichBomb) {
        playSound("BombExplode"); // Play the explode sound effect.

        Vector2 bombPos = whichBomb.tilePos; // Get the bomb's stats.
        int bombStr = whichBomb.bombStrength;
        
        // Make stop flags for the 4 cardinal directions.
        bool nStop = false;
        bool sStop = false;
        bool wStop = false;
        bool eStop = false;

        // Spawn the center of the explosion.
        spawnExplosion("res://scenes/effects/ExplodeCenter.tscn", false, false, bombPos);

        for(int i = 1; i < bombStr + 1; i++) {
            Vector2 nCheck = bombPos + (Vector2.Up * i);
            Vector2 sCheck = bombPos + (Vector2.Down * i);
            Vector2 wCheck = bombPos + (Vector2.Left * i);
            Vector2 eCheck = bombPos + (Vector2.Right * i);

            activeBombs.Remove(bombPos); // Remove the bomb from the dictionary and delete the bomb itself.
            whichBomb.QueueFree();
            
            // This part is going to get a bit long in the tooth, as we have to add checks for each cardinal direction that the bomb flames will travel.

            switch(nStop) {
                case bool v when v = !nStop && backgroundTiles.ContainsKey(nCheck) && backgroundTiles[nCheck] != 2:
                    nStop = true;
                    break;
                
                case bool v when v = !nStop && activeBreakables.ContainsKey(nCheck):
                    destroyBlock(nCheck);
                    nStop = true;
                    break;
                
                case bool v when v = !nStop && activeItems.ContainsKey(nCheck):
                    spawnItemBoom(nCheck);
                    nStop = true;
                    break;

                case bool v when v = !nStop && activeBombs.ContainsKey(nCheck):
                    activeBombs[nCheck].bombLife = 1;
                    nStop = true;
                    break;
                
                case bool v when v = !nStop && activeExplosions.ContainsKey(nCheck):
                    nStop = true;
                    break;
            }

            switch(sStop) {
                case bool v when v = !sStop && backgroundTiles.ContainsKey(sCheck) && backgroundTiles[sCheck] != 2:
                    sStop = true;
                    break;
                
                case bool v when v = !sStop && activeBreakables.ContainsKey(sCheck):
                    destroyBlock(sCheck);
                    sStop = true;
                    break;
                
                case bool v when v = !sStop && activeItems.ContainsKey(sCheck):
                    spawnItemBoom(sCheck);
                    sStop = true;
                    break;

                case bool v when v = !sStop && activeBombs.ContainsKey(sCheck):
                    activeBombs[sCheck].bombLife = 1;
                    sStop = true;
                    break;
                
                case bool v when v = !nStop && activeExplosions.ContainsKey(sCheck):
                    sStop = true;
                    break;
            }

            switch(wStop) {
                case bool v when v = !wStop && backgroundTiles.ContainsKey(wCheck) && backgroundTiles[wCheck] != 2:
                    wStop = true;
                    break;
                
                case bool v when v = !wStop && activeBreakables.ContainsKey(wCheck):
                    destroyBlock(wCheck);
                    wStop = true;
                    break;
                
                case bool v when v = !wStop && activeItems.ContainsKey(wCheck):
                    spawnItemBoom(wCheck);
                    wStop = true;
                    break;

                case bool v when v = !wStop && activeBombs.ContainsKey(wCheck):
                    activeBombs[wCheck].bombLife = 1;
                    wStop = true;
                    break;
                
                case bool v when v = !wStop && activeExplosions.ContainsKey(wCheck):
                    wStop = true;
                    break;
            }

            switch(eStop) {
                case bool v when v = !eStop && backgroundTiles.ContainsKey(eCheck) && backgroundTiles[eCheck] != 2:
                    eStop = true;
                    break;
                
                case bool v when v = !eStop && activeBreakables.ContainsKey(eCheck):
                    destroyBlock(eCheck);
                    eStop = true;
                    break;
                
                case bool v when v = !eStop && activeItems.ContainsKey(eCheck):
                    spawnItemBoom(eCheck);
                    eStop = true;
                    break;

                case bool v when v = !eStop && activeBombs.ContainsKey(eCheck):
                    activeBombs[eCheck].bombLife = 1;
                    eStop = true;
                    break;
                
                case bool v when v = !eStop && activeExplosions.ContainsKey(eCheck):
                    eStop = true;
                    break;
            }

            if(!nStop) {
                switch(i) {
                    case int v when i < bombStr:
                        spawnExplosion("res://scenes/effects/ExplodeVertical.tscn", false, false, nCheck);
                        break;
                    
                    case int v when i == bombStr:
                        spawnExplosion("res://scenes/effects/ExplodeVerticalTip.tscn", false, false, nCheck);
                        break;
                }
            }

            if(!sStop) {
                switch(i) {
                    case int v when i < bombStr:
                        spawnExplosion("res://scenes/effects/ExplodeVertical.tscn", true, false, sCheck);
                        break;
                    
                    case int v when i == bombStr:
                        spawnExplosion("res://scenes/effects/ExplodeVerticalTip.tscn", true, false, sCheck);
                        break;
                }
            }

            if(!wStop) {
                switch(i) {
                    case int v when i < bombStr:
                        spawnExplosion("res://scenes/effects/ExplodeHorizontal.tscn", false, false, wCheck);
                        break;
                    
                    case int v when i == bombStr:
                        spawnExplosion("res://scenes/effects/ExplodeHorizontalTip.tscn", false, false, wCheck);
                        break;
                }
            }

            if(!eStop) {
                switch(i) {
                    case int v when i < bombStr:
                        spawnExplosion("res://scenes/effects/ExplodeHorizontal.tscn", false, true, eCheck);
                        break;
                    
                    case int v when i == bombStr:
                        spawnExplosion("res://scenes/effects/ExplodeHorizontalTip.tscn", false, true, eCheck);
                        break;
                }
            }
        }
    }

    private void spawnExplosion(string path, bool vFlip, bool hFlip, Vector2 pos) {
        if(activeExplosions.ContainsKey(pos)) {
            return;
        }
        
        Control explodeLayer = (Control)GetNode("Explosion");
        PackedScene newBoom = (PackedScene)ResourceLoader.Load(path);
        Explosion result = (Explosion)newBoom.Instance();

        explodeLayer.AddChild(result);
        result.GlobalPosition = background.MapToWorld(pos) + background.CellSize / 2;
        result.s.FlipV = vFlip;
        result.s.FlipH = hFlip;
        result.tilePos = pos;
        
        activeExplosions.Add(pos, result);
    }

    private void spawnItemBoom(Vector2 pos) {
        Control explodeLayer = (Control)GetNode("Explosion");
        PackedScene newBoom = (PackedScene)ResourceLoader.Load("res://scenes/effects/ItemExplode.tscn");
        ItemExplode result = (ItemExplode)newBoom.Instance();

        explodeLayer.AddChild(result);
        result.GlobalPosition = background.MapToWorld(pos) + background.CellSize / 2;
        result.tilePos = pos;
    }

    private void destroyBlock(Vector2 pos) {
        activeBreakables.Remove(pos);
        breakables.SetCellv(pos, -1);

        PackedScene newEffect = (PackedScene)ResourceLoader.Load("res://scenes/effects/BlockExplode.tscn");
        BlockExplode deadBlock = (BlockExplode)newEffect.Instance();
        Control breakLayer = (Control)GetNode("Breakable");
        breakLayer.AddChild(deadBlock);
        deadBlock.GlobalPosition = background.MapToWorld(pos) + background.CellSize / 2;
    }

    public void destoryItem(Vector2 pos) {
        activeItems.Remove(pos);
        items.SetCellv(pos, -1);
    }

    private void checkforItems(Vector2 pos) {
        int tileID = (int)items.GetCellv(pos);

        if(tileID != -1) {
            playSound("ItemGet");

            switch(tileID) {
                case 0:
                    if(g.bombStrength < 6) {
                        g.bombStrength++;
                    }
                    break;
                
                case 1:
                    if(g.maxBombs < 4) {
                        g.maxBombs++;
                    }
                    break;
                
                case 2:
                    Random RNGesus = new Random();

                    p.poisonState = RNGesus.Next(1, 4);
                    p.poisonTicker = RNGesus.Next(400, 601);
                    break;
            }

            items.SetCellv(pos, -1);
            activeItems.Remove(pos);
        }
    }

    public void disableHitboxes() {
        Area2D heroBox = (Area2D)p.GetNode("Area2D");
        heroBox.Monitoring = false;

        for(int i = 0; i < activeEnemies.Count; i++) {
            Area2D badBox = (Area2D)activeEnemies[i].GetNode("Area2D");
            badBox.Monitoring = false;
        }
    }

    public void restartStage(int x) {
        
        if(x <= 0) { // The player died. Prevent users from decrementing the level ID and reset bomb stats.
            g.maxBombs = 1;
            g.bombStrength = 2;
            x = 0;
        }

        g.levelID += x; // Add x to levelID to increment or decrement level ID.

        GetTree().ReloadCurrentScene();
    }
}

