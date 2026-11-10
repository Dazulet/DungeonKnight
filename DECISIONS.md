# Decisions

## Week 10: Assignment 10, three enemy types

- **2026-11-10.** I asked Claude Code to check that the Shooter can hit a moving player. It fired only 3 times in 40 seconds and never hit. Its "too close" distance (3.5 m) was too big for my small arena, and it aimed at where I was, not where I was going. We changed the distance to 2.5 m and added aim lead. The aim line still locks 0.25 s before the shot, so a side step can dodge it.
- **2026-11-10.** I asked Claude Code to audit all three enemies for fair combat. The Patroller's hit only checked distance, so it could hit me through the edge of a pillar. Also, no enemy checked if I was on the screen. We added a line-of-sight check before the hit and an on-screen check before every attack.
- **2026-11-10.** In a test, some enemies died without ever seeing me, because I could shoot them from farther than they can see. I asked Claude Code to make enemies react to hits, but without seeing through walls. Now a hit turns the enemy toward the shot, and for 3 seconds it can see farther. Walls still block its sight.
- **2026-11-10.** I found out that all this work was committed in the wrong local project folder (github.com/Nurzhigit08/DungeonKnight), not in my repository. I asked Claude Code to audit both projects first. All asset GUIDs were the same, so we moved the work file by file in 6 stages, without copying any Git history. After each stage we ran the Unity tests again in my project before I made the commit.
