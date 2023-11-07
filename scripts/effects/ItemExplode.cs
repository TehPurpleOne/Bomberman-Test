using Godot;
using System;

public class ItemExplode : KinematicBody2D
{
    private Main w;
    private AnimationPlayer anim;
    private Sprite s;
    public Vector2 tilePos = Vector2.Zero;
    public bool killItem = false;

    public override void _Ready() {
        w = (Main)GetTree().GetNodesInGroup("world")[0];
        s = (Sprite)GetNode("Sprite");
        anim = (AnimationPlayer)GetNode("AnimationPlayer");

        anim.Play("BOOM");
    }

    public override void _PhysicsProcess(float delta) {
        if(s.Frame == 2 && !killItem) {
            w.destoryItem(tilePos);
            killItem = true;
        }
    }

    private void onAnimDone(string which) {
        QueueFree();
    }
}
