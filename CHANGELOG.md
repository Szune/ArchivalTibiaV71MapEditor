# Changelog

## 2023-06-10
* Update to .NET 6 and MonoGame 3.8.1
* Render multi-tile sprites correctly, e.g. 2x2 corpses, walls, etc

### Fixes
* Selecting a tile now requires releasing the mouse button before placing the selected tile
* Removing a ground tile is now possible when using the regular remove tool
* A missing `categories.cats` file generates the most basic one instead of crashing
