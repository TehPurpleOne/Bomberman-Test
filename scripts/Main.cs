using Godot;
using System;
using System.Collections.Generic;

public class Main : Node2D
{
    private TileMap background;
    private TileMap breakables;
    private TileMap items;
    private Player p;

    private Dictionary<Vector2, int> activeBreakables = new Dictionary<Vector2, int>();
    private Dictionary<Vector2, int> activeItems = new Dictionary<Vector2, int>();

    public enum states {NULL, INIT, START, RUN};
    public states state = states.NULL;
    public states previousState = states.NULL;

    public override void _Ready() {
        p = (Player)GetNode("Actor/Player");
        background = (TileMap)GetNode("Background/BackgroundMap");
        breakables = (TileMap)GetNode("Breakable/BreakableMap");
        items = (TileMap)GetNode("Item/ItemMap");

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
        
    }

    private states getTransition(float delta) {
        return states.NULL;
    }

    private void enterState(states newState, states oldState) { 
        switch(newState) {
            case states.INIT:
                // The first thing we want to do is populate the map with the destrucable blocks.
                Random RNGesus = new Random();

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
                    }
                }
                break;
        }
    }

    private void exitState(states oldState, states newState) { 
        
    }

    private void setState(states newState) {
        previousState = state;
        state = newState;

        exitState(previousState, newState);

        enterState(newState, previousState);
    }
}

