using Godot;
using System;

public class BlockExplode : KinematicBody2D
{
    private AnimationPlayer anim;

    public override void _Ready() {
        anim = (AnimationPlayer)GetNode("AnimationPlayer");

        anim.Play("BOOM");
    }

    private void onAnimDone(string which) {
        QueueFree();
    }
}
