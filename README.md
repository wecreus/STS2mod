# High Resolution Cards

A mod for Slay the Spire 2 that replaces card portraits with the higher resolution versions already present in the game files.

## What it does

The game normally displays card art from a texture atlas — many card images packed into a single large texture for rendering performance. This comes at the cost of image quality. The game also ships individual, higher resolution versions of every card portrait, but does not use them during normal play.

This mod intercepts the card portrait loading and redirects it to those higher resolution images instead.

## Notes

At 1920x1080, small card previews (such as in the deck view) may look worse than the unmodded version, since the high-resolution images are being scaled down more aggressively. At 2K (1440p) and above, the improvement is visible everywhere.

## Requirements

- Slay the Spire 2

## Installation

1. Copy `HighResolutionCards.dll`, `HighResolutionCards.pck`, and `HighResolutionCards.json` into your `Slay the Spire 2/mods/HighResolutionCards/` folder.
2. Launch the game.

## How it works

When the game loads a card portrait, it goes through a `ResourceFormatLoader` called `AtlasResourceLoader`. This mod patches that loader to check whether a higher resolution version of the requested image exists at `res://images/packed/card_portraits/`. If it does, that image is returned instead of the atlas version. If it does not, the original atlas image is used as a fallback.
