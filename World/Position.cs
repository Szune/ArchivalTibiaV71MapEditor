namespace ArchivalTibiaV71MapEditor.World
{
    public class Position
    {
        public readonly ushort X;
        public readonly ushort Y;
        public readonly byte Z;

        public Position(ushort x, ushort y, byte z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Position other))
                return false;
            return Z == other.Z
                   && X == other.X
                   && Y == other.Y;
        }

        public override int GetHashCode()
        {
            return (X, Y, Z).GetHashCode();
        }

        public Position Add(ushort xyOffset) => new Position((ushort)(X + xyOffset), (ushort)(Y + xyOffset), Z);
        public Position Subtract(ushort xyOffset) => new Position((ushort)(X - xyOffset), (ushort)(Y - xyOffset), Z);
    }
}
