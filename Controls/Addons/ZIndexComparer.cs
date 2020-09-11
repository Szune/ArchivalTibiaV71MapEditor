using System.Collections.Generic;

namespace ArchivalTibiaV71MapEditor.Controls.Addons
{
    public class ZIndexComparer : Comparer<IControl>
    {
        public override int Compare(IControl x, IControl y)
        {
            if (x?.ZIndex < y?.ZIndex)
                return -1;
            if (x?.ZIndex > y?.ZIndex)
                return 1;
            return 0;
        }

        public static readonly ZIndexComparer Instance = new ZIndexComparer();
    }
}