using Godot;
using System;
using System.Collections.Generic;

public class Main : Node2D
{
    private TileMap background;
    private TileMap breakables;
    private TileMap items;

    private Dictionary<Vector2, int> activeBreakables = new Dictionary<Vector2, int>();
    private Dictionary<Vector2, int> activeItems = new Dictionary<Vector2, int>();

    public enum states {NULL, INIT, START, RUN};
    public states state = states.NULL;
    public states previousState = states.NULL;

    public override void _Ready() {
        background = (TileMap)GetNode("Background/BackgroundMap");
        breakables = (TileMap)GetNode("Item/ItemMap");
        items = (TileMap)GetNode("Breakable/BreakableMap");

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

                for(int i = 0; i < background.GetUsedCells().Count; i++) {
                    // Get the current game world's map. All tiles should be used to function properly.
                    Vector2 tilePos = (Vector2)background.GetUsedCells()[i];
                    int tileID = (int)background.GetCellv(tilePos);

                    // Next, we going to place breakable blocks at random locations, but...
                    bool setBlock = Convert.ToBoolean(RNGesus.Next(0, 2));

                    GD.Print(tilePos,", ",setBlock);
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

