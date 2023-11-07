using Godot;
using System;
using System.Collections.Generic;

public class Pollun : Enemy {
    public override void _Ready() {
        w = (Main)GetTree().GetNodesInGroup("world")[0];
        anim = (AnimationPlayer)GetNode("AnimationPlayer");

        availableDirections.Add(Vector2.Up);
        availableDirections.Add(Vector2.Down);
        availableDirections.Add(Vector2.Left);
        availableDirections.Add(Vector2.Right);

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
            case states.MOVE:
                velocity = MoveAndSlide(velocity, Vector2.Zero);
                break;
        }
    }

    private states getTransition(float delta) {
        switch(state) {
            case states.INIT:
                if(w.state == Main.states.RUN) {
                    return states.CHOOSE;
                }
                break;
            
            case states.CHOOSE:
                if(targetPos != Vector2.Zero) {
                    return states.MOVE;
                }
                break;
        }

        return states.NULL;
    }

    private void enterState(states newState, states oldState) { 
        switch(newState) {
            case states.INIT:
                anim.Play("MOVE");
                break;
            
            case states.CHOOSE:
                while(targetPos == Vector2.Zero) {
                    Random RNGesus = new Random();

                    Vector2 direction = availableDirections[RNGesus.Next(0, 4)];

                    Vector2 newTarget = w.background.WorldToMap(GlobalPosition) + direction;

                    GD.Print(Name," at position ",w.background.WorldToMap(GlobalPosition)," is trying to move to ",newTarget);

                    /* bool openSpace = w.backgroundTiles[newTarget] == 2 || !w.activeBreakables.ContainsKey(newTarget);

                    switch(openSpace) {
                        case true:
                            GD.Print("Location is valid!");
                            targetPos = newTarget;
                            velocity = direction * speed;
                            break;
                        
                        case false:
                            GD.Print("Location is NOT valid...");
                            break;
                    } */
                }
                break;
            
            case states.MOVE:
                
                break;
        }
    }

    private void exitState(states oldState, states newState) { 
        
    }

    private void setState(states newState) {
        GD.Print(Name,", ",newState);
        previousState = state;
        state = newState;

        exitState(previousState, newState);

        enterState(newState, previousState);
    }
}
