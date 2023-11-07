# Bomberman Example Demo
An example project of how to do a Bomberman-like game in Godot using C#.

## Controls

Arrows: Movement

Z: Drop a Bomb

## Features

- Full Game Loop: This demo shows a faull game loop with randomized enemy and block placement. More enemies are placed the longer you play!
- Player Functions: Bomberman is capable of movement in 8 directions, dropping bombs, and dying if struck.
- Bombs: Bombs explode taking out anything in their path! You can even chain bombs together!
- Enemy Functions: Only a single enemy is available, but is able to choose which direction to move and die when struck by a bomb.
- Items:
    - Fire Up: Increases the length of a bomb's explosion.
    - Bomb Up: Increases the amount of bombs Bomberman can drop at once.
    - Poison: Causes one of 3 side effects:
        - Prevents the player from dropping bombs.
        - Makes the explosions pitifully weak.
        - Slows the player's speed to a crawl.

## Notes and Bugs

- Sometimes the code to place enemies sometimes places them on top of each other. This is rare, and I will work it out later.
