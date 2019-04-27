using System;

namespace Octans
{
    public struct Point3I 
    {
        public int X;
        public int Y;
        public int Z;

        public Point3I(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public int this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return X;
                    case 1:
                        return Y;
                    case 2:
                        return Z;
                }

                throw new IndexOutOfRangeException();
            }
            set
            {
                switch (index)
                {
                    case 0:
                        X = value;
                        break;
                    case 1:
                        Y = value;
                        break;
                    case 2:
                        Z = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }
    }
}