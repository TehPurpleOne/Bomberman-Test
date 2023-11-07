using Godot;
using System;
using System.Collections.Generic;

public class Enemy : KinematicBody2D
{
    public Main w;
    public AnimationPlayer anim;

    public Vector2 velocity;
    [Export] public float speed = 60;

    public Vector2 targetPos = Vector2.Zero;
    public List<Vector2> availableDirections = new List<Vector2>();

    public enum states {NULL, INIT, CHOOSE, MOVE, HIT, DEAD};
    public states state = states.NULL;
    public states previousState = states.NULL;

    /* public override void _Ready() {
        w = (Main)GetTree().GetNodesInGroup("world")[0];
        anim = (AnimationPlayer)GetNode("AnimationPlayer");

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

    }

    private void exitState(states oldState, states newState) { 
        
    }

    private void setState(states newState) {
        previousState = state;
        state = newState;

        exitState(previousState, newState);

        enterState(newState, previousState);
    } */
}
