using System;

namespace ArchivalTibiaV71MapEditor.World
{
    [Flags]
    public enum TileFlags : byte
    {
        None,
        HasItems = 16,
        HasCreatures = 32,
        HasEvent = 64,
        HasExtra = 128,
    }
}