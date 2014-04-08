using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Aether.Geometry;

namespace Aether.IO.Ast
{
    public class ParamSet : Collection<Param>
    {
        public ParamSet(IEnumerable<Param> parameters)
            : base(parameters.ToList())
        {
            
        }

        public ParamSet()
        {
            
        }

        public string FindString(string name, string defaultValue)
        {
            return Find(name, defaultValue);
        }

        public bool FindBoolean(string name, bool defaultValue)
        {
            return Find(name, defaultValue);
        }

        public int FindInt32(string name, int defaultValue)
        {
            return Find(name, defaultValue);
        }

        public float FindSingle(string name, float defaultValue)
        {
            return Find(name, defaultValue);
        }

        public float[] FindSingleList(string name)
        {
            return Find(name, new float[] { });
        }

        public Spectrum FindSpectrum(string name, Spectrum defaultValue)
        {
            return Find(name, defaultValue);
        }

        public Point FindPoint(string name, Point defaultValue)
        {
            return Find(name, defaultValue);
        }

        public string FindTexture(string name)
        {
            return Find(name, string.Empty);
        }

        private T Find<T>(string name, T defaultValue)
        {
            var param = this.FirstOrDefault(x => x.Name == name);
            if (param == null)
                return defaultValue;
            if (param.Value.GetType() != typeof(T))
                return defaultValue;
            return (T) param.Value;
        }
    }
}