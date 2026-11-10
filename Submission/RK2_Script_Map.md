# Script map: three enemy types (Assignment 10)

A picture of the same map for the video is in `Submission/RK2_Script_Map.png`.

## Which scripts talk to which

```mermaid
flowchart LR
    subgraph Enemy["Each enemy prefab"]
        Brain["PatrollerAI / ShooterAI / ChargerAI<br/>(EnemyBrain&lt;TState&gt;)"]
        FSM["EnemyStateMachine<br/>one state, timer, max 1 change per frame"]
        Vision["EnemyVision<br/>distance → angle → Physics2D.Raycast"]
        Motor["EnemyMotor<br/>Rigidbody2D move, avoid walls, dash"]
        Tele["AttackTelegraph<br/>red circle / line / lane"]
        Icon["EnemyAlertIcon<br/>! and ?"]
        Label["EnemyStateLabel<br/>state name (editor + dev builds)"]
        Health["EnemyHealth<br/>HP, Damaged, Died"]
    end
    Brain --> FSM
    Brain --> Vision
    Brain --> Motor
    Brain --> Tele
    Brain --> Icon
    Brain --> Label
    Health -- "Damaged (hit direction)" --> Brain

    PlayerCombat --> Projectile -- "TakeDamage" --> Health
    Brain -- "Patroller strike, Charger dash" --> PlayerHealth
    Brain -- "Shooter fires" --> EnemyBolt -- "TakeDamage" --> PlayerHealth
    Health -- "Died" --> LevelGoal --> GameManager --> Victory["Victory screen<br/>(GameOverPanel)"]
    LevelGoal --> HUD["HudController<br/>TRY n · ENEMIES n"]
```

Shared code is only the plumbing (state machine, movement, vision, warnings, icons). Each enemy decides in its own script, so the three behave differently.

## Patroller (`PatrollerAI.cs`)

```mermaid
stateDiagram-v2
    [*] --> Patrol
    Patrol --> Chase: sees player
    Patrol --> Search: hit by a bolt
    Chase --> WindUp: within 1.3 m and cooldown ready
    Chase --> Search: lost sight
    WindUp --> Recover: after 0.55 s (strike)
    Recover --> Chase: still sees player
    Recover --> Search: lost sight
    Search --> Chase: sees player
    Search --> Patrol: looked around 2.2 s
```

| State | What it does |
|---|---|
| Patrol | Walks between waypoints at 1.4 m/s and pauses 0.6 s at each one. |
| Chase | Runs at 2.6 m/s toward the player (the player runs at 5 m/s). |
| WindUp | Stops, turns red, and a red circle fills for 0.55 s. |
| Recover | Strikes once (only if the player is still in the circle and not behind a wall), then waits 0.45 s. Cooldown 1.1 s. |
| Search | Walks to the last known spot, or toward a shot, shows "?" and looks around for 2.2 s. |

Sight: 5.5 m, 110° cone, raycast. Tests the player's **positioning**: stay out of the cone and use pillars as cover.

## Shooter (`ShooterAI.cs`)

```mermaid
stateDiagram-v2
    [*] --> Watch
    Watch --> Engage: sees player
    Watch --> Search: hit by a bolt
    Engage --> Aim: clear shot, 2.5–7 m, cooldown ready
    Engage --> Search: lost sight
    Aim --> Recover: after 0.7 s (fires)
    Aim --> Search: lost sight before the lock
    Recover --> Engage: still sees player
    Recover --> Search: lost sight
    Search --> Engage: sees player
    Search --> Watch: 2.5 s
```

| State | What it does |
|---|---|
| Watch | Guards its post and sweeps its view left and right. |
| Engage | Keeps distance: backs away under 2.5 m, walks closer over 7 m, steps sideways if a pillar blocks the shot. |
| Aim | 0.7 s: a red line follows the player, then turns yellow and locks for the last 0.25 s. |
| Recover | Fires one magenta bolt (6.5 m/s) and waits 0.4 s. Cooldown 1.8 s. |
| Search | Looks toward the last known spot for 2.5 s, then walks back to its post. |

Sight: 7.5 m, 120° cone, raycast. Walls stop the bolt. Tests the player's **reaction**: step aside when the line turns yellow.

## Charger (`ChargerAI.cs`)

```mermaid
stateDiagram-v2
    [*] --> Prowl
    Prowl --> Track: sees player
    Track --> WindUp: within 5 m, clear lane, cooldown ready
    Track --> Prowl: lost sight
    WindUp --> Charge: after 0.65 s
    Charge --> Recover: 6.5 m done or hit a wall
    Recover --> Track: still sees player
    Recover --> Prowl: lost sight
```

| State | What it does |
|---|---|
| Prowl | Walks around its home in a fixed pattern. If hit, it stops and looks toward the shot for 1.2 s. |
| Track | Turns to the player and comes closer (stops at 4 m). |
| WindUp | 0.65 s: the direction locks, a wide red lane shows the path, and the body shakes and turns red. |
| Charge | Dashes at 11 m/s in a straight line. It can hit only once per dash. |
| Recover | Stunned and blue for 0.8 s (1.0 s after hitting a wall). Cooldown 2.0 s. |

Sight: 6 m, 100° cone, raycast. Tests the player's **timing**: step out of the lane, then hit it while it is stunned.

## Rules shared by all three

- Every attack has a visible wind-up between 0.3 and 0.8 s, then a recovery and a cooldown.
- No attack in the first 1.5 s of the level, and no attack while the enemy or the player is off-screen.
- Sight is checked every 0.1 s, not every frame.
- A hit turns the enemy toward the shot. For 3 s it can see 1.6 times farther, but walls still block its sight.
- No damage after death: a dead enemy stops thinking in the same frame.
