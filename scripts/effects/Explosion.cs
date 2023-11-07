using Godot;
using System;

public class Explosion : KinematicBody2D
{
    private Main w;
    private AnimationPlayer anim;

    public Vector2 tilePos = Vector2.Zero;
    public Sprite s;

    public override void _Ready() {
        w = (Main)GetTree().GetNodesInGroup("world")[0];
        s = (Sprite)GetNode("Sprite");
        anim = (AnimationPlayer)GetNode("AnimationPlayer");

        anim.Play("BOOM");
    }

    private void onAnimDone(string which) {
        w.activeExplosions.Remove(tilePos);
        QueueFree();
    }
}
