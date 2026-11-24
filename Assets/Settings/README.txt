Settings

Since Assignment 13 the project uses URP (Universal Render Pipeline 17.6) with the 2D Renderer.
The colour space stays Gamma, so the pixel art keeps its old colours.
Project-wide settings are applied from code by Assets/Scripts/Editor/ProjectConfigurator.cs
(menu: Dungeon Knight > Build Project). The URP assets here are created by
Assets/Scripts/Editor/JuiceBuilder.cs if they are missing:

- DungeonKnightURP.asset: the render pipeline asset (no HDR, no MSAA, render scale 1, LDR colour grading).
  It is set as the default pipeline in Graphics settings. Quality levels do not override it.
- DungeonKnight2DRenderer.asset: the 2D Renderer used by the pipeline. The game has no 2D lights;
  sprites use the URP Sprite-Unlit-Default material, so they look like before.
- JuiceVolumeProfile.asset: Bloom, Color Adjustments and Vignette for the Global Volume in the
  Dungeon and Endless scenes. The pause menu EFFECTS button turns the Volume on and off.

Other settings:
- Player: landscape only, package com.daulet.dungeonknight, version 1.0.0, full screen,
  renders under the notch (the HUD stays inside the safe area).
- Physics 2D: zero gravity. The layer collision matrix is set at runtime by GameLayers.cs.
- Quality: anti-aliasing off, so pixel art stays sharp.
- Editor: Visible Meta Files, Force Text serialization, 2D mode.
- Art import rules: PixelArtImportSettings.cs (16 px per unit, point filter, no compression).
