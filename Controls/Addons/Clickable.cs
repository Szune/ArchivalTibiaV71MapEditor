using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ArchivalTibiaV71MapEditor.Controls.Addons
{
    public class Clickable
    {
        [Flags]
        enum State
        {
            Up = 1,
            Down = 2,
            Hovering = 4,
        }

        class ClickState
        {
            public Action OnClick { get; set; }
            public Action OnDown { get; set; }
            public Action OnUp { get; set; }
            private readonly Clickable _clickable;
            public State State;

            public ClickState(Clickable clickable)
            {
                _clickable = clickable;
                State = State.Up;
            }

            public bool HitTest(bool downNow)
            {
                if (State.HasFlag(State.Down))
                {
                    if (downNow)
                    {
                        OnDown?.Invoke();
                        return false;
                    }

                    State = State.Up;
                    OnUp?.Invoke();
                    return false;
                }

                if (!downNow)
                {
                    if (!_clickable._bounds.Contains(UiState.Mouse.Position))
                    {
                        if (State.HasFlag(State.Hovering))
                        {
                            State &= ~State.Hovering; // AND NOT
                            _clickable.OnMouseLeave?.Invoke();
                        }

                        return false;
                    }

                    _clickable.OnHover?.Invoke();
                    State |= State.Hovering;
                    return false;
                }

                if (!_clickable._bounds.Contains(UiState.Mouse.Position))
                    return false;
                State = State.Down;
                OnClick?.Invoke();
                return true;
            }
        }

        public Clickable()
        {
            _left = new ClickState(this);
            _middle = new ClickState(this);
            _right = new ClickState(this);
        }

        private ClickState _left;
        private ClickState _right;
        private ClickState _middle;

        public Action OnLeftDown
        {
            get => _left.OnDown;
            set => _left.OnDown = value;
        }

        public Action OnRightDown
        {
            get => _right.OnDown;
            set => _right.OnDown = value;
        }

        public Action OnMiddleDown
        {
            get => _middle.OnDown;
            set => _middle.OnDown = value;
        }

        public Action OnLeftUp
        {
            get => _left.OnUp;
            set => _left.OnUp = value;
        }

        public Action OnRightUp
        {
            get => _right.OnUp;
            set => _right.OnUp = value;
        }

        public Action OnMiddleUp
        {
            get => _middle.OnUp;
            set => _middle.OnUp = value;
        }

        public Action OnLeftClick
        {
            get => _left.OnClick;
            set => _left.OnClick = value;
        }

        public Action OnRightClick
        {
            get => _right.OnClick;
            set => _right.OnClick = value;
        }

        public Action OnMiddleClick
        {
            get => _middle.OnClick;
            set => _middle.OnClick = value;
        }

        public Action OnHover { get; set; }
        public Action OnMouseLeave { get; set; }
        private Rectangle _bounds;

        public void SetRect(Rectangle rect)
        {
            _bounds = rect;
        }

        public bool IsDown(MouseButton button)
        {
            return button switch
            {
                MouseButton.Left => _left.State,
                MouseButton.Right => _right.State,
                MouseButton.Middle => _middle.State,
            } == State.Down;
        }

        public bool HitTest(MouseButton button)
        {
            var downNow = button switch
            {
                MouseButton.Left => UiState.Mouse.LeftButton,
                MouseButton.Right => UiState.Mouse.RightButton,
                MouseButton.Middle => UiState.Mouse.MiddleButton,
            } == ButtonState.Pressed;

            return button switch
            {
                MouseButton.Left => _left.HitTest(downNow),
                MouseButton.Right => _right.HitTest(downNow),
                MouseButton.Middle => _middle.HitTest(downNow),
            };
        }
    }
}