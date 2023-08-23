using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ArchivalTibiaV71MapEditor.Constants;
using ArchivalTibiaV71MapEditor.Utilities;

namespace ArchivalTibiaV71MapEditor.Controls.Addons
{
    public class HorizontalScrollBar : ControlBase
    {
        private const int BarEndsWidth = 6;
        private readonly Texture2D _spriteSheet;
        private readonly Border _border;
        private Rectangle[] _backgroundRects;
        private Rectangle _barLeftRect;
        private Rectangle[] _barMiddleRects;
        private Rectangle _barRightRect;
        private Rectangle _barHitBox;
        private readonly IconButtonUsingOverlay _leftArrow;
        private readonly IconButtonUsingOverlay _rightArrow;
        private readonly Draggable _draggable;
        private Rectangle _bgHitBox;
        private float _dragMultiplier;
        private int _verticalOffset;
        private readonly MouseScrollable _mouseScrollable;
        private readonly bool _scrollable;

        public HorizontalScrollBar(IWindow window, ICanHaveHorizontalScrollBar parent,
            Border border, bool scrollable = true)
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
            _leftArrow = new IconButtonUsingOverlay(window, this, Ui.ScrollBar.LeftArrow,
                Ui.ScrollBar.ArrowNormalOverlay, Ui.ScrollBar.ArrowPressedOverlay, Rectangle.Empty);
            _leftArrow.SetOnClick(ScrollUp);
            _rightArrow = new IconButtonUsingOverlay(window, this, Ui.ScrollBar.RightArrow,
                Ui.ScrollBar.ArrowNormalOverlay, Ui.ScrollBar.ArrowPressedOverlay, Rectangle.Empty);
            _rightArrow.SetOnClick(ScrollDown);
            _draggable = new Draggable(MouseButton.Left, 5);
            if (scrollable)
            {
                _mouseScrollable = new MouseScrollable(parent, ModifierKeys.Shift);
                _mouseScrollable.OnScroll = ScrollMouse;
            }
        }

        private void ScrollMouse(int delta)
        {
            var parent = (ICanHaveHorizontalScrollBar) Parent;
            parent.HorizontalScroll(delta);
        }

        private void ScrollDown()
        {
            var parent = (ICanHaveHorizontalScrollBar) Parent;
            parent.HorizontalScroll(1);
        }

        private void ScrollUp()
        {
            var parent = (ICanHaveHorizontalScrollBar) Parent;
            parent.HorizontalScroll(-1);
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
                sb.Draw(_spriteSheet, _backgroundRects[i], Ui.ScrollBar.HorizontalBackground, Color.White);
            }

            // bar
            sb.Draw(_spriteSheet, _barLeftRect, Ui.ScrollBar.HorizontalForegroundStart, Color.White);
            for (int i = 0; i < _barMiddleRects.Length; i++)
            {
                sb.Draw(_spriteSheet, _barMiddleRects[i], Ui.ScrollBar.HorizontalForegroundMiddle,
                    Color.White);
            }

            sb.Draw(_spriteSheet, _barRightRect, Ui.ScrollBar.HorizontalForegroundEnd, Color.White);

            // arrows
            _leftArrow.Draw(sb, gameTime, drawComponents);
            _rightArrow.Draw(sb, gameTime, drawComponents);
        }

        public void Offset(int vertical)
        {
            _verticalOffset = vertical;
        }

        public override HitBox HitTest()
        {
            HitBox hit;
            if (_draggable.HitTest().IsHit)
            {
                var delta = (int) (_draggable.GetHorizontalMoveDelta() * _dragMultiplier);
                var parent = (ICanHaveHorizontalScrollBar) Parent;
                parent.HorizontalScroll(-delta);
                _draggable.InvalidateDelta();
                return HitBox.Hit(this);
            }
            else if (MouseManager.IsDown(MouseButton.Left) &&
                     _bgHitBox.Contains(UiState.Mouse.Position))
            {
                var parent = (ICanHaveHorizontalScrollBar) Parent;
                var hitBoxX = UiState.Mouse.Position.X - _bgHitBox.Left;
                var scrollPercent = (float) hitBoxX / _bgHitBox.Width;
                var scrollIndexAtClick = Math.Min((int) (parent.HorizontalMaxScrollIndex * scrollPercent),
                    parent.HorizontalMaxScrollIndex);
                parent.HorizontalScrollTo(scrollIndexAtClick);
            }
            else if ((hit = _leftArrow.HitTest()).IsHit)
            {
                return hit;
            }
            else if ((hit = _rightArrow.HitTest()).IsHit)
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
            base.Recalculate();
            // TODO: clean up this mess
            var leftX = Parent.Bounds.Left + Parent.BorderSize;
            var rightX = Parent.Bounds.Left + Parent.Width - Ui.ScrollBar.LeftArrow.Width - BorderSize;
            var bgWidth = rightX - leftX - Ui.ScrollBar.LeftArrow.Width;

            var y = _border switch
            {
                Border.Top => Parent.Bounds.Top + Parent.BorderSize + _verticalOffset,
                Border.Bottom => Parent.Bounds.Top + Parent.Height - Ui.ScrollBar.LeftArrow.Height -
                Parent.BorderSize + _verticalOffset,
                _ => throw new ArgumentOutOfRangeException()
            };

            var scrollPartWidth = Ui.ScrollBar.LeftArrow.Width;

            var leftArrowRect = new Rectangle(leftX, y, scrollPartWidth, Scroll.Height);
            _leftArrow.SetRect(leftArrowRect);
            var rightArrowRect = new Rectangle(rightX, y, scrollPartWidth, Scroll.Height);
            _rightArrow.SetRect(rightArrowRect);

            var staticBgWidth = Ui.ScrollBar.HorizontalBackground.Width;
            _backgroundRects = Parts.CreateHorizontal(bgWidth, staticBgWidth, y,
                leftX + Ui.ScrollBar.LeftArrow.Width,
                Scroll.Height);

            var parent = (ICanHaveHorizontalScrollBar) Parent;
            var midWidth = CalculateHeightOfMiddlePartOfScrollBar(parent, bgWidth);

            var fullMidBarWidth = midWidth + BarEndsWidth * 2;

            var oneScrollWidth =
                parent.HorizontalItemCount <= parent.HorizontalMaxVisibleItems
                    ? 0
                    : (float) (bgWidth - fullMidBarWidth) / parent.HorizontalMaxScrollIndex;
            _draggable.SetMinimumDragDistance((int) oneScrollWidth);
            _dragMultiplier = (float) Math.Max(Math.Ceiling(1 / oneScrollWidth), 1);
            var barOffset = (int) Math.Ceiling(oneScrollWidth * parent.HorizontalScrollIndex);

            var barStartX = barOffset + leftX + Ui.ScrollBar.LeftArrow.Width;
            _barLeftRect = new Rectangle(barStartX, y,
                Ui.ScrollBar.HorizontalForegroundStart.Width,
                Scroll.Height);
            var midPos = barStartX +
                         Ui.ScrollBar.HorizontalForegroundStart.Width;

            _barMiddleRects = Parts.CreateHorizontal(midWidth,
                Ui.ScrollBar.HorizontalForegroundMiddle.Width, y, midPos, Scroll.Height);
            _barRightRect = new Rectangle(midPos + midWidth, y, Ui.ScrollBar.HorizontalForegroundEnd.Width,
                Scroll.Height);
            _barHitBox = new Rectangle(barStartX, y, fullMidBarWidth, Scroll.Height);
            _bgHitBox = new Rectangle(
                leftX + scrollPartWidth, y, bgWidth, Scroll.Height);
            _draggable.SetRect(_barHitBox);
        }

        private static int CalculateHeightOfMiddlePartOfScrollBar(ICanHaveHorizontalScrollBar parent, int bgWidth)
        {
            int midWidth;
            if (parent.HorizontalItemCount <= parent.HorizontalMaxVisibleItems)
                midWidth = bgWidth - (BarEndsWidth * 2);
            else
            {
                midWidth = ((parent.HorizontalMaxVisibleItems * bgWidth)
                            / parent.HorizontalItemCount)
                           - BarEndsWidth * 2;
            }

            if (midWidth >= bgWidth)
                midWidth = bgWidth - (BarEndsWidth * 2);
            else if (midWidth <= 0)
                midWidth = 0;
            return midWidth;
        }
    }
}
