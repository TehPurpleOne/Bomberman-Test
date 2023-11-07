using Godot;
using System;

public class Bomb : KinematicBody2D
{
    private Main w;
    private AnimationPlayer anim;

    public int bombLife = 180;
    public int bombStrength = 0;
    public Vector2 tilePos = Vector2.Zero;
    public bool detonate = false;

    public override void _Ready() {
        w = (Main)GetTree().GetNodesInGroup("world")[0];
        anim = (AnimationPlayer)GetNode("AnimationPlayer");

        anim.Play("IDLE");
    }

    public override void _PhysicsProcess(float delta) {
        // A state machine isn't really necessary for the bombs, as they only tick down, call the explosion script, then delete itself.
        bombLife--;

        if(bombLife <= 0) {
            detonate = true;
        }

        if(detonate) {
            w.detonate(this);
        }
    }
}
