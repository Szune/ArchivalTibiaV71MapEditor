using System;

namespace ArchivalTibiaV71MapEditor.Controls
{
    public interface IModal : IControl
    {
        Guid Id { get; }
        void Update();
    }
}