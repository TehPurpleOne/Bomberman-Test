using Godot;
using System;
using System.Collections.Generic;

public class Player : KinematicBody2D {
    private Main w;
    private AnimationPlayer anim;

    private Vector2 velocity;
    [Export] private float speed = 60;

    public enum states {NULL, INIT, IDLE, WALK, DEAD, CLEAR};
    public states state = states.NULL;
    public states previousState = states.NULL;

    public int poisonState = 0;
    public int poisonTicker = 0;

    private string animDir = "";
    private string animState = "";

    public override void _Ready() {
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

    public override void _Process(float delta) {
        setAnimation();
    }
    
    private void stateLogic(float delta) {
        switch(state) {
            case states.IDLE:
            case states.WALK:
                if(poisonTicker > 0) {
                    if(poisonTicker % 4 == 0) {
                        Visible = !Visible;
                    }
                    poisonTicker--;
                }

                if(poisonTicker == 0 && poisonState != 0) {
                    Show();
                    poisonState = 0;
                }

                float finalSpeed = speed;

                if(poisonState == 3) {
                    finalSpeed = finalSpeed/ 2;
                }

                velocity = w.getDirection() * finalSpeed;
                velocity = MoveAndSlide(velocity, Vector2.Zero);

                if(w.getButton()) {
                    w.setBomb();
                }
                break;
        }
    }

    private states getTransition(float delta) {
        switch(state) {
            case states.INIT:
                if(w.state == Main.states.RUN) {
                    return states.IDLE;
                }
                break;
            
            case states.IDLE:
                if(w.getDirection() != Vector2.Zero) {
                    return states.WALK;
                }
                break;
            
            case states.WALK:
                if(w.getDirection() == Vector2.Zero) {
                    return states.IDLE;
                }
                break;
        }

        if(w.state == Main.states.CLEAR && state != states.CLEAR) {
            return states.CLEAR;
        }

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
    }

    private void setAnimation() {
        switch(state) {
            case states.IDLE:
                animState = "IDLE";
                break;
            
            case states.WALK:
                animState = "WALK";
                break;
            
            case states.DEAD:
                animState = "DEAD";
                break;
            
            case states.CLEAR:
                animState = "CLEAR";
                break;
        }

        switch(w.getDirection()) {
            case Vector2 v when v == Vector2.Up:
                animDir = "NORTH";
                break;
            
            case Vector2 v when v == Vector2.Down:
                animDir = "SOUTH";
                break;
            
            case Vector2 v when v == Vector2.Left:
                animDir = "WEST";
                break;
            
            case Vector2 v when v == Vector2.Right:
                animDir = "EAST";
                break;
        }

        if(animState == "DEAD" || animState == "CLEAR") {
            Show();
            animDir = "";
        }

        string animCheck = animState + animDir;

        if(anim.HasAnimation(animCheck) && animCheck != anim.CurrentAnimation) {
            anim.Play(animCheck);
        }
    }

    private void onHitBoxBodyEntered(Node body) {
        // No health bars to save Bomberman here. Instant death for him!
        setState(states.DEAD);
    }

    private void onAnimDone(string which) {
        switch(which) {
            case "DEAD":
                // Restart the current stage after a death.
                w.restartStage(0);
                break;
        }
    }
}
