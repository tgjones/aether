using System;
using System.Collections.Generic;
using Aether.Geometry;
using Aether.IO.Ast;
using Aether.Textures;

namespace Aether.IO
{
    public class TextureParams
    {
        private readonly ParamSet _geomParams;
        private readonly ParamSet _materialParams;
        private readonly Dictionary<string, Texture<float>> _floatTextures;
        private readonly Dictionary<string, Texture<Spectrum>> _spectrumTextures;

        public TextureParams(ParamSet geomParams, ParamSet materialParams,
            Dictionary<string, Texture<float>> floatTextures,
            Dictionary<string, Texture<Spectrum>> spectrumTextures)
        {
            _geomParams = geomParams;
            _materialParams = materialParams;
            _floatTextures = floatTextures;
            _spectrumTextures = spectrumTextures;
        }

        public bool FindBoolean(string name, bool defaultValue)
        {
            return _geomParams.FindBoolean(name, defaultValue);
        }

        public int FindInt32(string name, int defaultValue)
        {
            return _geomParams.FindInt32(name, defaultValue);
        }

        public Point FindPoint(string name, Point defaultValue)
        {
            return _geomParams.FindPoint(name, defaultValue);
        }

        public float FindSingle(string name, float defaultValue)
        {
            return _geomParams.FindSingle(name, defaultValue);
        }

        public Spectrum FindSpectrum(string name, Spectrum defaultValue)
        {
            return _geomParams.FindSpectrum(name, defaultValue);
        }

        public string FindString(string name, string defaultValue)
        {
            return _geomParams.FindString(name, defaultValue);
        }

        public Texture<Spectrum> GetSpectrumTexture(string n, Spectrum defaultValue)
        {
            var name = _geomParams.FindTexture(n);
            if (string.IsNullOrEmpty(name))
                name = _materialParams.FindTexture(n);

            if (!string.IsNullOrEmpty(name))
            {
                if (_spectrumTextures.ContainsKey(name))
                    return _spectrumTextures[name];
                throw new InvalidOperationException(string.Format("Couldn't find spectrum texture named '{0}' for parameter '{1}'", name, n));
            }
            var value = _geomParams.FindSpectrum(n, _materialParams.FindSpectrum(n, defaultValue));
            return new ConstantTexture<Spectrum>(value);
        }

        public Texture<float> GetFloatTexture(string n, float defaultValue)
        {
            var name = _geomParams.FindTexture(n);
            if (string.IsNullOrEmpty(name))
                name = _materialParams.FindTexture(n);

            if (!string.IsNullOrEmpty(name))
            {
                if (_floatTextures.ContainsKey(name))
                    return _floatTextures[name];
                throw new InvalidOperationException(string.Format("Couldn't find float texture named '{0}' for parameter '{1}'", name, n));
            }
            var value = _geomParams.FindSingle(n, _materialParams.FindSingle(n, defaultValue));
            return new ConstantTexture<float>(value);
        }

        public Texture<float> GetOptionalFloatTexture(string n)
        {
            var name = _geomParams.FindTexture(n);
            if (string.IsNullOrEmpty(name))
                name = _materialParams.FindTexture(n);

            if (string.IsNullOrEmpty(name))
                return null;

            if (_floatTextures.ContainsKey(name))
                return _floatTextures[name];
            throw new InvalidOperationException(string.Format("Couldn't find float texture named '{0}' for parameter '{1}'", name, n));
        }
    }
}