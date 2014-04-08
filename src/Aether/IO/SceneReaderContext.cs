using System;
using System.Collections.Generic;
using Aether.Geometry;

namespace Aether.IO
{
    public class SceneReaderContext
    {
        public SceneReaderState CurrentState = SceneReaderState.OptionsBlock;
        public TransformSet CurrentTransform;
        public uint ActiveTransformBits = TransformSet.AllTransformsBits;
        public Dictionary<string, TransformSet> NamedCoordinateSystems = new Dictionary<string, TransformSet>();
        public RenderOptions RenderOptions = new RenderOptions();
        public GraphicsState GraphicsState = new GraphicsState();
        public Stack<GraphicsState> GraphicsStateStack = new Stack<GraphicsState>();
        public Stack<TransformSet> TransformStack = new Stack<TransformSet>();
        public Stack<uint> ActiveTransformBitsStack = new Stack<uint>();

        public void ForActiveTransforms(Func<Transform, Transform> callback)
        {
            for (var i = 0; i < TransformSet.MaxTransforms; i++)
                if ((ActiveTransformBits & (1 << i)) != 0)
                    CurrentTransform[i] = callback(CurrentTransform[i]);
        }

        public void VerifyOptions(string name)
        {
            if (CurrentState != SceneReaderState.OptionsBlock)
                throw new InvalidOperationException("Options cannot be set inside world block: '" + name + "' not allowed.");
        }

        public void VerifyWorld(string name)
        {
            if (CurrentState != SceneReaderState.WorldBlock)
                throw new InvalidOperationException("Scene description must be inside world block: '" + name + "' not allowed.");
        }
    }

    public enum SceneReaderState
    {
        OptionsBlock,
        WorldBlock
    }
}