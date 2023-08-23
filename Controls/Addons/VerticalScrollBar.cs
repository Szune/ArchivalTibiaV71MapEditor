using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ArchivalTibiaV71MapEditor.Constants;
using ArchivalTibiaV71MapEditor.Utilities;

namespace ArchivalTibiaV71MapEditor.Controls.Addons
{
    public class VerticalScrollBar : ControlBase
    {
        private const int BarEndsHeight = 6;
        private readonly Texture2D _spriteSheet;
        private readonly Border _border;
        private Rectangle[] _backgroundRects;
        private Rectangle _barUpRect;
        private Rectangle[] _barMiddleRects;
        private Rectangle _barDownRect;
        private Rectangle _barHitBox;
        private readonly IconButtonUsingOverlay _upArrow;
        private readonly IconButtonUsingOverlay _downArrow;
        private readonly Draggable _draggable;
        private Rectangle _bgHitBox;
        private float _dragMultiplier;
        private int _horizontalOffset;
        private readonly MouseScrollable _mouseScrollable;
        private readonly bool _scrollable;

        public VerticalScrollBar(IWindow window, ICanHaveVerticalScrollBar parent, Border border, bool scrollable = true)
            : base(window, parent)
        {
            // scrollbars must be attached to a control
            Window = window;
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            _spriteSheet = Ui.SpriteSheet;
            _border = border;
            _scrollable = scrollable;
            ZIndex = parent.ZIndex;
            Visible = true;
            _upArrow = new IconButtonUsingOverlay(window, this, Ui.ScrollBar.UpArrow,
                Ui.ScrollBar.ArrowNormalOverlay, Ui.ScrollBar.ArrowPressedOverlay, Rectangle.Empty);
            _upArrow.SetOnClick(ScrollUp);
            _downArrow = new IconButtonUsingOverlay(window, this, Ui.ScrollBar.DownArrow,
                Ui.ScrollBar.ArrowNormalOverlay, Ui.ScrollBar.ArrowPressedOverlay, Rectangle.Empty);
            _downArrow.SetOnClick(ScrollDown);
            _draggable = new Draggable(MouseButton.Left, 5);

            if (scrollable)
            {
                _mouseScrollable = new MouseScrollable(parent, ModifierKeys.None);
                _mouseScrollable.OnScroll = ScrollMouse;
            }
        }

        private void ScrollMouse(int delta)
        {
            var parent = (ICanHaveVerticalScrollBar) Parent;
            parent.VerticalScroll(delta);
        }

        private void ScrollDown()
        {
            var parent = (ICanHaveVerticalScrollBar) Parent;
            parent.VerticalScroll(1);
        }

        private void ScrollUp()
        {
            var parent = (ICanHaveVerticalScrollBar) Parent;
            parent.VerticalScroll(-1);
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime, DrawComponents drawComponents)
        {
            if (!Visible)
                return;
            if (IsDirty)
                Recalculate();

            // background
            for (int i = 0; i < _backgroundRects.Length; i++)
            {
                sb.Draw(_spriteSheet, _backgroundRects[i], Ui.ScrollBar.VerticalBackground, Color.White);
            }

            // bar
            sb.Draw(_spriteSheet, _barUpRect, Ui.ScrollBar.VerticalForegroundStart, Color.White);
            for (int i = 0; i < _barMiddleRects.Length; i++)
            {
                sb.Draw(_spriteSheet, _barMiddleRects[i], Ui.ScrollBar.VerticalForegroundMiddle, Color.White);
            }

            sb.Draw(_spriteSheet, _barDownRect, Ui.ScrollBar.VerticalForegroundEnd, Color.White);

            // arrows
            _upArrow.Draw(sb, gameTime, drawComponents);
            _downArrow.Draw(sb, gameTime, drawComponents);
        }

        public void Offset(int horizontal)
        {
            _horizontalOffset = horizontal;
        }

        public override HitBox HitTest()
        {
            HitBox hit;
            if (_draggable.HitTest().IsHit)
            {
                var delta =(int) (_draggable.GetVerticalMoveDelta() * _dragMultiplier);
                var parent = (ICanHaveVerticalScrollBar) Parent;
                parent.VerticalScroll(-delta);
                _draggable.InvalidateDelta();
                return HitBox.Hit(this);
            }
            else if (MouseManager.IsDown(MouseButton.Left) &&
                     _bgHitBox.Contains(UiState.Mouse.Position))
            {
                var parent = (ICanHaveVerticalScrollBar) Parent;
                var hitBoxY = UiState.Mouse.Position.Y - _bgHitBox.Top;
                var scrollPercent = (float)hitBoxY / _bgHitBox.Height;
                var scrollIndexAtClick = Math.Min((int)(parent.VerticalMaxScrollIndex * scrollPercent), parent.VerticalMaxScrollIndex);
                parent.VerticalScrollTo(scrollIndexAtClick);
            }
            else if ((hit = _upArrow.HitTest()).IsHit)
            {
                return hit;
            }
            else if ((hit = _downArrow.HitTest()).IsHit)
            {
                return hit;
            }
            else if (_scrollable && (hit = _mouseScrollable.HitTest()).IsHit)
            {
                return hit;
            }

            return HitBox.Miss;
        }

        public override void Recalculate()
        {
            // TODO: clean up this mess
            var upY = Parent.Bounds.Top + Parent.BorderSize;
            var downY = Parent.Bounds.Top + Parent.Height - Ui.ScrollBar.DownArrow.Height - BorderSize;
            var bgHeight = downY - upY - Ui.ScrollBar.UpArrow.Height;

            var x = _border switch
            {
                Border.Left => Parent.Bounds.Left + Parent.BorderSize + _horizontalOffset,
                Border.Right => Parent.Bounds.Left + Parent.Width - Ui.ScrollBar.UpArrow.Width -
                                Parent.BorderSize + _horizontalOffset,
                _ => throw new ArgumentOutOfRangeException()
            };

            var scrollPartHeight = Ui.ScrollBar.UpArrow.Height;

            var upArrowRect = new Rectangle(x, upY, Scroll.Width, scrollPartHeight);
            _upArrow.SetRect(upArrowRect);
            var downArrowRect = new Rectangle(x, downY, Scroll.Width, scrollPartHeight);
            _downArrow.SetRect(downArrowRect);

            var staticBgHeight = Ui.ScrollBar.VerticalBackground.Height;
            _backgroundRects = Parts.CreateVertical(bgHeight, staticBgHeight, x,
                upY + Ui.ScrollBar.UpArrow.Height,
                Scroll.Width);

            var parent = (ICanHaveVerticalScrollBar) Parent;
            var midHeight = CalculateHeightOfMiddlePartOfScrollBar(parent, bgHeight);

            var fullMidBarHeight = midHeight + BarEndsHeight * 2;

            var oneScrollHeight =
                parent.VerticalItemCount <= parent.VerticalMaxVisibleItems
                    ? 0
                    : (float) (bgHeight - fullMidBarHeight) / parent.VerticalMaxScrollIndex;
            _draggable.SetMinimumDragDistance((int) oneScrollHeight);
            _dragMultiplier = (float)Math.Max(Math.Ceiling(1 / oneScrollHeight), 1);
            var barOffset = (int) Math.Ceiling(oneScrollHeight * parent.VerticalScrollIndex);

            var barStartY = barOffset + upY + Ui.ScrollBar.UpArrow.Height;
            _barUpRect = new Rectangle(x, barStartY,
                Scroll.Width,
                Ui.ScrollBar.VerticalForegroundStart.Height);
            var midPos = barStartY +
                         Ui.ScrollBar.VerticalForegroundStart.Height;

            _barMiddleRects = Parts.CreateVertical(midHeight,
                Ui.ScrollBar.VerticalForegroundMiddle.Height, x, midPos, Scroll.Width);
            _barDownRect = new Rectangle(x, midPos + midHeight,
                Scroll.Width,
                Ui.ScrollBar.VerticalForegroundEnd.Height);
            _barHitBox = new Rectangle(x, barStartY, Scroll.Width, fullMidBarHeight);
            _bgHitBox = new Rectangle(x, upY + scrollPartHeight, Scroll.Width, bgHeight);
            _draggable.SetRect(_barHitBox);
        }

        private static int CalculateHeightOfMiddlePartOfScrollBar(ICanHaveVerticalScrollBar parent, int bgHeight)
        {
            int midHeight;
            if (parent.VerticalItemCount <= parent.VerticalMaxVisibleItems)
                midHeight = bgHeight - (BarEndsHeight * 2);
            else
            {
                midHeight = ((parent.VerticalMaxVisibleItems * bgHeight)
                            / parent.VerticalItemCount)
                           - BarEndsHeight * 2;
            }

            if (midHeight >= bgHeight)
                midHeight = bgHeight - (BarEndsHeight * 2);
            else if (midHeight <= 0)
                midHeight = 0;
            return midHeight;
        }
    }
}
