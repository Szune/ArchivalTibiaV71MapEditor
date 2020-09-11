using Microsoft.Xna.Framework.Graphics;

namespace ArchivalTibiaV71MapEditor.Extensions
{
    public static class SpriteBatchExtensions
    {
        public static void UsualBegin(this SpriteBatch sb)
        {
            sb.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp);
        }

        public static void UsualEnd(this SpriteBatch sb)
        {
            sb.End();
        }
    }
}