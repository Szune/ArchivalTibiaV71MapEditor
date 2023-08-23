# Changelog

## 2023-08-23
* Fix: floor order changed from 0 being bottom to 0 being top, like the client expects
* Fix: offset tiles based on floor
* Fix: modals closing the wrong window
* Fix: adding roof on other floors than ground floor
* Add: changing floor with mouse scroll
* Add: move tool
* Add: top left position label
* Add: shortcut list in Help -> Shortcuts
* Add: save and file dialogs where you can load or save maps with new names

## 2023-06-10
* Update to .NET 6 and MonoGame 3.8.1
* Render multi-tile sprites correctly, e.g. 2x2 corpses, walls, etc

### Fixes
* Selecting a tile now requires releasing the mouse button before placing the selected tile
* Removing a ground tile is now possible when using the regular remove tool
* A missing `categories.cats` file generates the most basic one instead of crashing
* No longer performs actions unless the map editor window is active
