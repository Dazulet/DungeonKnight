Settings

The project uses the built-in render pipeline, so no render pipeline assets live here.
Project-wide settings are applied from code by Assets/Scripts/Editor/ProjectConfigurator.cs
(menu: Dungeon Knight > Build Project):

- Player: landscape only, package com.narxoz.dungeonknight, version 1.0.0, full screen,
  renders under the notch (the HUD stays inside the safe area).
- Physics 2D: zero gravity. The layer collision matrix is set at runtime by GameLayers.cs.
- Quality: anti-aliasing off, so pixel art stays sharp.
- Editor: Visible Meta Files, Force Text serialization, 2D mode.
- Art import rules: PixelArtImportSettings.cs (16 px per unit, point filter, no compression).
