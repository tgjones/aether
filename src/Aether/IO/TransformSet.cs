using Aether.Geometry;

namespace Aether.IO
{
    public class TransformSet
    {
        public const int MaxTransforms = 2;
        public const int StartTransformBits = 1 << 0;
        public const int EndTransformBits = 1 << 1;
        public const int AllTransformsBits = (1 << MaxTransforms) - 1;

        private readonly Transform[] _transforms;

        public TransformSet()
        {
            _transforms = new Transform[MaxTransforms];
            for (var i = 0; i < MaxTransforms; i++)
                _transforms[i] = new Transform();
        }

        public Transform this[int index]
        {
            get { return _transforms[index]; }
            set { _transforms[index] = value; }
        }

        public bool IsAnimated
        {
            get
            {
                for (var i = 0; i < MaxTransforms - 1; i++)
                    if (!_transforms[i].Equals(_transforms[i + 1]))
                        return true;
                return false;
            }
        }

        public TransformSet Clone()
        {
            var result = new TransformSet();
            for (var i = 0; i < MaxTransforms; i++)
                result._transforms[i] = _transforms[i];
            return result;
        }

        public static TransformSet Invert(TransformSet transformSet)
        {
            var result = new TransformSet();
            for (var i = 0; i < MaxTransforms; i++)
                result._transforms[i] = Transform.Invert(transformSet._transforms[i]);
            return result;
        }
    }
}