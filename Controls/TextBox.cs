using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ArchivalTibiaV71MapEditor.Constants;
using ArchivalTibiaV71MapEditor.Controls.Addons;
using ArchivalTibiaV71MapEditor.Fonts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ArchivalTibiaV71MapEditor.Controls;

public interface IFocusedTextBox
{
    public TextBox Focused { get; }
    void SetFocus(TextBox textBox);
    void LoseFocus();
}

public class FocusedTextBox : IFocusedTextBox
{
    public TextBox Focused { get; private set; }
    public void SetFocus(TextBox textBox)
    {
        Focused = textBox;
    }

    public void LoseFocus()
    {
        Focused = null;
    }
}


public class TextBox : ControlBase
{
    private class KeyPressEvent
    {
        public TimeSpan TotalGameTime { get; init; }
        public Keys Key { get; init; }
    }


    public class Node<T>
    {
        public T Value { get; set; }
        public Node<T> Next { get; set; }

        public void Clear()
        {
            Next = default;
            Value = default;
        }
    }

    private class KeyCache
    {
        public int Length { get; private set; }
        public int Capacity { get; private set; }
        public Node<KeyPressEvent> Head {get; private set; }

        public KeyCache(int capacity)
        {
            Capacity = capacity;
        }

        public void Update(KeyPressEvent keyPressEvent)
        {
            if (Head == null)
            {
                Head = new Node<KeyPressEvent>
                {
                    Value = keyPressEvent
                };

                return;
            }


            var findNode = Head;
            if (findNode.Value.Key == keyPressEvent.Key)
            {
                findNode.Value = keyPressEvent;
                return;
            }

            while ((findNode = findNode.Next) != null)
            {
                if (findNode.Value.Key == keyPressEvent.Key)
                {
                    findNode.Value = keyPressEvent;
                    return;
                }
            }

            if (Length == Capacity)
            {
                LeastRecentlyUsed(keyPressEvent);
            }
            else
            {
                var node = Head;
                var lastNode = node;
                while ((node = node.Next) != null)
                {
                    lastNode = node;
                }

                lastNode.Next = new Node<KeyPressEvent>
                {
                    Value = keyPressEvent
                };
            }
        }

        public KeyPressEvent Find(Keys key)
        {
            if (Head == null)
            {
                return null;
            }

            var node = Head;
            if (node.Value.Key == key)
            {
                return node.Value;
            }

            while ((node = node.Next) != null)
            {
                if (node.Value.Key == key)
                {
                    return node.Value;
                }
            }

            return null;
        }

        // always evict the least recently used
        private void LeastRecentlyUsed(KeyPressEvent keyPressEvent)
        {
            Debug.Assert(Head != null, "Head cannot be null in LeastRecentlyUsed");

            var node = Head;
            var lastNode = node;
            var previousNode = node;
            while ((node = node.Next) != null)
            {
                if (lastNode.Value.TotalGameTime.TotalMilliseconds > node.Value.TotalGameTime.TotalMilliseconds)
                {
                    previousNode = lastNode;
                    lastNode = node;
                }
            }

            var nextNode = previousNode.Next?.Next;
            previousNode.Next?.Clear();
            previousNode.Next = new Node<KeyPressEvent>
            {
                Value = keyPressEvent,
                Next = nextNode
            };
        }
    }

    private readonly Texture2D _spriteSheet;
    private readonly IFont _font;
    public bool ReadOnly { get; set; }
    private string _text;
    private CachedString _cached;
    private bool _textDirty;
    // private TimeSpan _lastInputTime = TimeSpan.Zero;
    // private Keys[] _lastKeys = Array.Empty<Keys>();
    // private Keys _lastKey = Keys.Home;

    private readonly KeyCache _lastKeys = new KeyCache(7);

    public string Text
    {
        get => _text;
        set
        {
            if (_text == value)
                return;
            _text = value;
            _cached = _text == null ? null : CachedString.Create(_font, _text);
            _textDirty = true;
            IsDirty = true;
        }
    }

    public Color Color { get; set; } = Color.White;
    private TextBoxStates _state = TextBoxStates.Normal;
    private Vector2 _textPos;
    private bool _clicked = false;
    public bool Focused => _state == TextBoxStates.Focused;


    public TextBox(IWindow window, IControl parent, Rectangle rect, string text = "", bool visible = true,
        bool readOnly = false) : base(window, parent, visible)
    {
        SetRect(rect);
        _font = IoC.Get<IFont>();
        _text = text;
        ReadOnly = readOnly;
        _spriteSheet = Ui.SpriteSheet;
    }

    public override void Draw(SpriteBatch sb, GameTime gameTime, DrawComponents drawComponents)
    {
        if (!Visible)
            return;
        if (IsDirty)
            Recalculate();


        sb.Draw(_spriteSheet, CleanRect, Ui.Button.SmallButtonNormal, _state == TextBoxStates.Focused ? Color.LightBlue : Color);

        if (_cached == null)
            return;
        var caret = (gameTime.TotalGameTime.Seconds % 2 == 0) && _state == TextBoxStates.Focused;
        drawComponents.FontRenderer.DrawCachedString(sb, _cached, _textPos, caret ? Color.Red : Color.White);
    }

    public override void Recalculate()
    {
        base.Recalculate();
        if (_textDirty)
        {
            if (_text == null)
            {
                _cached = null;
            }
            else
            {
                _cached = CachedString.Create(_font, _text);
            }

            _textDirty = false;
        }

        if (_cached != null)
            _textPos = new Vector2(
                CleanRect.Left + (Width / 2) - (_cached.MeasuredSize.X / 2),
                CleanRect.Top + (CleanRect.Height / 2 - _cached.MeasuredSize.Y / 2));
        IsDirty = false;
    }

    public override HitBox HitTest()
    {
        if (!IsVisible())
            return HitBox.Miss;

        switch (_state)
        {
            case TextBoxStates.Normal:
                if (MouseManager.IsUp(MouseButton.Left))
                    return HitBox.Miss;
                if (Bounds.Contains(UiState.Mouse.Position))
                {
                    _state = TextBoxStates.Focused;
                    IoC.Get<IFocusedTextBox>().SetFocus(this);
                    IsDirty = true;
                    return HitBox.Hit(this);
                }

                break;
            case TextBoxStates.Focused:
                if (MouseManager.IsDown(MouseButton.Left) && !Bounds.Contains(UiState.Mouse.Position))
                {
                    _state = TextBoxStates.Normal;
                    IoC.Get<IFocusedTextBox>().LoseFocus();
                    IsDirty = true;
                }
                else
                {
                    return HitBox.Hit(this);
                }

                break;
        }

        return HitBox.Miss;
    }

    public void UpdateText(GameTime gameTime)
    {
        var keys = UiState.Keyboard.GetPressedKeys();
        var shift = UiState.Keyboard.IsKeyDown(Keys.LeftShift) ||
                    UiState.Keyboard.IsKeyDown(Keys.RightShift) ||
                    UiState.Keyboard.CapsLock;

        KeyPressEvent keyPressEvent;
        foreach (var key in keys)
        {

            if ((keyPressEvent = _lastKeys.Find(key)) != null)
            {
                if (gameTime.TotalGameTime.TotalMilliseconds - keyPressEvent.TotalGameTime.TotalMilliseconds < 150)
                {
                    continue;
                }
                HandleKeyPress(key, shift);
                _lastKeys.Update(new KeyPressEvent
                {
                    Key = key,
                    TotalGameTime = gameTime.TotalGameTime
                });
            }
            else
            {
                HandleKeyPress(key, shift);
                _lastKeys.Update(new KeyPressEvent
                {
                    Key = key,
                    TotalGameTime = gameTime.TotalGameTime
                });
            }
        }

        // var keys = UiState.Keyboard.GetPressedKeys();
        // var shift = UiState.Keyboard.IsKeyDown(Keys.LeftShift) ||
        //             UiState.Keyboard.IsKeyDown(Keys.RightShift) ||
        //             UiState.Keyboard.CapsLock;
        // if (keys.Length < 1)
        // {
        //     _lastKeys = Array.Empty<Keys>();
        //     return;
        // }
        //
        // if (_lastKeys.SequenceEqual(keys) &&
        //     gameTime.TotalGameTime.TotalMilliseconds - _lastInputTime.TotalMilliseconds < 150)
        // {
        //     return;
        // }
        //
        // _lastKeys = keys;
        // _lastInputTime = gameTime.TotalGameTime;
        //
        // if (keys.Length == 0)
        // {
        //     return;
        // }
        //
        // foreach (var key in keys.Take(1))
        // {
        //     // if (key == _lastKey &&
        //     //     gameTime.TotalGameTime.TotalMilliseconds - _lastInputTime.TotalMilliseconds < 150)
        //     // {
        //     //     continue;
        //     // }
        //     HandleKeyPress(key, shift);
        //     // _lastKey = key;
        //     // _lastInputTime = gameTime.TotalGameTime;
        // }
    }

    private void HandleKeyPress(Keys key, bool shift)
    {
        switch (key)
        {
            case Keys.Back when Text.Length > 0:
                if (!shift)
                {
                    Text = Text[..^1];
                }
                else
                {
                    Text = Text.Length > 5 ? Text[..^5] : "";
                }
                break;
            case Keys.Back:
            case Keys.Tab:
            case Keys.Enter:
            case Keys.Escape:
            case Keys.LeftShift:
            case Keys.RightShift:
            case Keys.LeftControl:
            case Keys.RightControl:
            case Keys.LeftAlt:
            case Keys.RightAlt:
                break;
            case Keys.Space:
                Text += ' ';
                break;
            default:
                Text += key.KeyToString(shift);
                break;
        }
    }
}
