namespace ArchivalTibiaV71MapEditor.Controls.Addons
{
    public interface ICanHaveHorizontalScrollBar : IControl
    {
        int HorizontalItemCount { get; }
        int HorizontalScrollIndex { get; }
        int HorizontalMaxVisibleItems { get; }
        int HorizontalMaxScrollIndex { get; }
        void HorizontalScroll(int delta);
        void HorizontalScrollTo(int index);
    }
}