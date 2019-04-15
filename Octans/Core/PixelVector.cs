using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Octans
{
    public readonly struct PixelVector
    {
        public int X { get; }
        public int Y { get; }

        [DebuggerStepThrough]
        public PixelVector(int x, int y)
        {
            X = x;
            Y = y;
        }

        [Pure]
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
                    default:
                        throw new InvalidOperationException("Invalid index.");
                }
            }
        }
    }
}