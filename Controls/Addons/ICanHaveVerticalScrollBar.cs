namespace ArchivalTibiaV71MapEditor.Controls.Addons
{
    public interface ICanHaveVerticalScrollBar : IControl
    {
        int VerticalItemCount { get; }
        int VerticalScrollIndex { get; }
        int VerticalMaxVisibleItems { get; }
        int VerticalMaxScrollIndex { get; }
        void VerticalScroll(int delta);
        void VerticalScrollTo(int index);
    }
}