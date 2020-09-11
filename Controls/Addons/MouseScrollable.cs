using System;

namespace ArchivalTibiaV71MapEditor.Controls.Addons
{
    public class MouseScrollable : IHitTestable
    {
        private readonly IControl _parent;
        private readonly ModifierKeys _modifier;
        /// <summary>
        /// Integer is the scroll delta divided by 120.
        /// </summary>
        public Action<int> OnScroll { get; set; }
        /// <summary>
        /// Integer is the scroll delta divided by 120.
        /// </summary>
        public Action<int> OnScrollForward { get; set; }
        /// <summary>
        /// Integer is the scroll delta divided by 120.
        /// </summary>
        public Action<int> OnScrollBackward { get; set; }

        public MouseScrollable(IControl parent, ModifierKeys modifier)
        {
            _parent = parent;
            _modifier = modifier;
        }

        public HitBox HitTest()
        {
            var delta = UiState.ScrollDelta;
            if (delta != 0 && _parent.Bounds.Contains(UiState.Mouse.Position) && KeyboardManager.IsModifierDown(_modifier))
            {
                if (delta > 0)
                {
                    OnScrollBackward?.Invoke(delta / 120);
                }
                else
                {
                    OnScrollForward?.Invoke(delta / 120);
                }
                OnScroll?.Invoke(delta / 120);
                return HitBox.Hit(_parent);
            }
            
            return HitBox.Miss;
        }
    }
}