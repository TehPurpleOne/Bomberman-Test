using Godot;
using System;
using System.Collections.Generic;

public class Pollun : Enemy {
    private Vector2 direction = Vector2.Zero;

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
    
    private void stateLogic(float delta) {
        switch(state) {
            case states.MOVE:
                // Move Pollun.
                if(velocity != direction * speed) { // Keep Pollun moving if something were to stop him before reaching his target position.
                    velocity = direction* speed;
                }

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
                if(targetPos != GlobalPosition) {
                    return states.MOVE;
                }
                return states.CHOOSE;
            
            case states.MOVE:
                if(targetPos.DistanceTo(GlobalPosition) < 1) {
                    return states.CHOOSE;
                }
                break;
        }

        switch(state) {
            case states.CHOOSE:
            case states.MOVE:
                if(hit) {
                    return states.HIT;
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
                Random RNGesus = new Random();

                // Target position should never be 0;
                if(targetPos == Vector2.Zero) {
                    targetPos = GlobalPosition;
                }

                // Center the Pollun in the current tile.
                if(GlobalPosition != targetPos && targetPos != Vector2.Zero) {
                    GlobalPosition = targetPos;
                }

                // Add the 4 cardinal directions to the list.
                availableDirections.Add(Vector2.Up);
                availableDirections.Add(Vector2.Down);
                availableDirections.Add(Vector2.Left);
                availableDirections.Add(Vector2.Right);

                // Next, eliminate invalid directions.
                Vector2 previousDir = velocity.Normalized();
                Vector2 tilePos = Vector2.Zero;

                // First, we need to see if Pollun wants to continue on his current path, if one was chosen.
                int behaviorA = RNGesus.Next(0, 256);

                if(behaviorA >= 32) { // Try to stay the course.
                    tilePos = w.background.WorldToMap(GlobalPosition) + previousDir;

                    if(w.background.GetCellv(tilePos) == 2 && !w.activeBreakables.ContainsKey(tilePos) && !w.activeBombs.ContainsKey(tilePos) && previousDir != Vector2.Zero) {
                        // Staying the course was successful!
                        targetPos = GlobalPosition + (previousDir * w.background.CellSize);
                        direction = previousDir;
                        velocity = previousDir * speed;
                    }

                    if(w.background.GetCellv(tilePos) != 2 || w.activeBreakables.ContainsKey(tilePos) || w.activeBombs.ContainsKey(tilePos) || previousDir == Vector2.Zero) {
                        // Whoops. Can't go that way anymore. Let's adjust behaviorA.
                        behaviorA = 0;
                    }
                }

                if(behaviorA < 32) { // Choose a new direction.
                    int behaviorB = RNGesus.Next(0, 4);

                    for(int i = 0; i < availableDirections.Count; i++) {
                        Vector2 desiredDir = availableDirections[behaviorB];

                        tilePos = w.background.WorldToMap(GlobalPosition) + desiredDir;

                        if(w.background.GetCellv(tilePos) == 2 && !w.activeBreakables.ContainsKey(tilePos) && !w.activeBombs.ContainsKey(tilePos)) {
                            // Pollun can move in the desired direction!
                            direction = desiredDir;
                            targetPos = GlobalPosition + (desiredDir * w.background.CellSize);
                            velocity = desiredDir * speed;
                            break;
                        }

                        if(w.background.GetCellv(tilePos) != 2 || w.activeBreakables.ContainsKey(tilePos) || w.activeBombs.ContainsKey(tilePos)) {
                            // That direction is blocked. Let's try another.
                            behaviorB++;
                            behaviorB = Mathf.Wrap(behaviorB, 0, 4);
                        }
                    }
                }
                break;
            
            case states.HIT:
                anim.Play("HIT");
                //Prevent Pollun from killing Bomberman if he is already dead.
                CollisionShape2D obstacleBox = (CollisionShape2D)GetNode("CollisionShape2D");
                obstacleBox.Disabled = true;
                break;

            case states.DEAD:
                anim.Play("DEAD");
                break;
        }
    }

    private void exitState(states oldState, states newState) { 
        
    }

    private void setState(states newState) {
        hit = false;
        previousState = state;
        state = newState;

        exitState(previousState, newState);

        enterState(newState, previousState);
    }

    private void onAnimDone(string which) {
        // Since different enemies have different animations to worry about, checking for animation completion isn't up to the base script.
        switch(which) {
            case "HIT":
                setState(states.DEAD);
                break;
            
            case "DEAD":
                w.activeEnemies.Remove(this);
                QueueFree();
                break;
        }
    }
}
