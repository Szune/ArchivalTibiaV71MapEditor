namespace ArchivalTibiaV71MapEditor.Controls.Addons
{
    public readonly struct HitBox
    {
        public static readonly HitBox Miss = new HitBox(false, null);

        private HitBox(bool isHit, IHitTestable control)
        {
            IsHit = isHit;
            Control = control;
        }

        public bool IsHit { get; }
        public IHitTestable Control { get; }

        public static HitBox Hit(IHitTestable control)
        {
            return new HitBox(true, control);
        }
    }
}
