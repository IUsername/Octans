﻿namespace Octans.Light
{
    public class PointLight : ILight
    {
        public PointLight(Point position, Color intensity)
        {
            Position = position;
            Intensity = intensity;
            SamplePoints = new[] {Position};
        }

        public Point Position { get; }
        public Color Intensity { get; }
        public Point[] SamplePoints { get; }
        public int Samples => 1;
        public void Preprocess(IScene scene)
        {
            
        }

        public LightType Type => LightType.DeltaPosition;
        public Spectrum Le(in Ray ray) => throw new System.NotImplementedException();
    }
}