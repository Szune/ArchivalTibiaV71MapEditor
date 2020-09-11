using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ArchivalTibiaV71MapEditor.Controls.Addons
{
    // Like the scroll bar that you can drag :/
    public class Draggable : IHitTestable
    {
        private float _minimumDragDistance;

        enum DragState
        {
            End,
            Start,
            Move,
        }

        public Draggable(int minimumDragDistance)
        {
            _minimumDragDistance = minimumDragDistance;
        }
        
        public Draggable(MouseButton button, int minimumDragDistance)
        {
            _button = button;
            _minimumDragDistance = minimumDragDistance;
        }

        private DragState _state = DragState.End;
        private Vector2 _lastMousePosition = Vector2.Zero;
        private Vector2 _moveDelta = Vector2.Zero;
        private Rectangle _bounds;
        private readonly MouseButton _button = MouseButton.Left;

        public void SetRect(Rectangle rect)
        {
            _bounds = rect;
        }

        public HitBox HitTest(MouseButton button)
        {
            var buttonState = button switch
            {
                MouseButton.Left => UiState.Mouse.LeftButton,
                MouseButton.Right => UiState.Mouse.RightButton,
                MouseButton.Middle => UiState.Mouse.MiddleButton,
            };
            var pos = UiState.Mouse.Position.ToVector2();
            switch (_state)
            {
                case DragState.End:
                    if (buttonState != ButtonState.Pressed)
                        return HitBox.Miss;
                    if (_bounds.Contains(pos))
                    {
                        _state = DragState.Start;
                    }

                    break;
                case DragState.Start:
                    // could probably remove this one and just set it in the End case
                    if (buttonState != ButtonState.Pressed)
                    {
                        _state = DragState.End;
                        return HitBox.Miss;
                    }

                    _lastMousePosition = pos;
                    _state = DragState.Move;

                    break;
                case DragState.Move:
                    if (buttonState != ButtonState.Pressed)
                    {
                        _state = DragState.End;
                        return HitBox.Miss;
                    }

                    var delta = _lastMousePosition - pos;
                    if (Math.Abs(delta.X) >= _minimumDragDistance ||
                        Math.Abs(delta.Y) >= _minimumDragDistance)
                    {
                        _moveDelta = delta;
                        _lastMousePosition = pos;
                    }

                    return HitBox.Hit(this);
            }

            return HitBox.Miss;
        }
        
        public HitBox HitTest()
        {
            var buttonState = _button switch
            {
                MouseButton.Left => UiState.Mouse.LeftButton,
                MouseButton.Right => UiState.Mouse.RightButton,
                MouseButton.Middle => UiState.Mouse.MiddleButton,
            };
            var pos = UiState.Mouse.Position.ToVector2();
            switch (_state)
            {
                case DragState.End:
                    if (buttonState != ButtonState.Pressed)
                        return HitBox.Miss;
                    if (_bounds.Contains(pos))
                    {
                        _state = DragState.Start;
                    }

                    break;
                case DragState.Start:
                    // could probably remove this one and just set it in the End case
                    if (buttonState != ButtonState.Pressed)
                    {
                        _state = DragState.End;
                        return HitBox.Miss;
                    }

                    _lastMousePosition = pos;
                    _state = DragState.Move;

                    break;
                case DragState.Move:
                    if (buttonState != ButtonState.Pressed)
                    {
                        _state = DragState.End;
                        return HitBox.Miss;
                    }

                    var delta = _lastMousePosition - pos;
                    if (Math.Abs(delta.X) >= _minimumDragDistance ||
                        Math.Abs(delta.Y) >= _minimumDragDistance)
                    {
                        _moveDelta = delta;
                        _lastMousePosition = pos;
                    }

                    return HitBox.Hit(this);
            }

            return HitBox.Miss;
        }

        public Vector2 GetMoveDelta()
        {
            return _moveDelta / _minimumDragDistance;
        }

        public int GetVerticalMoveDelta()
        {
            return (int) (_moveDelta.Y / _minimumDragDistance);
        }

        public int GetHorizontalMoveDelta()
        {
            return (int) (_moveDelta.X / _minimumDragDistance);
        }

        public void InvalidateDelta()
        {
            _moveDelta = Vector2.Zero;
        }

        public void SetMinimumDragDistance(int minimumDragDistance)
        {
            _minimumDragDistance = minimumDragDistance < 1 
                ? 1 
                : minimumDragDistance;
        }
    }
}