using System;
using System.Linq;
using Aether.Geometry;
using Aether.IO.Ast;
using Aether.IO.Util;
using Piglet.Parser;
using Piglet.Parser.Configuration;

namespace Aether.IO
{
    public class SceneParser
    {
        private readonly IParser<object> _parser;
 
        public SceneParser()
        {
            var configurator = ParserFactory.Configure<object>();

            // Non-terminals.

            var sceneFile = CreateNonTerminal<SceneFile>(configurator);
            var directiveList = CreateNonTerminal<Directive[]>(configurator);
            var directive = CreateNonTerminal<Directive>(configurator);
            var standardDirectiveType = CreateNonTerminal<StandardDirectiveType>(configurator);
            var paramSet = CreateNonTerminal<Param[]>(configurator);
            var param = CreateNonTerminal<Param>(configurator);
            var paramValueList = CreateNonTerminal<object[]>(configurator);
            var paramValue = CreateNonTerminal<object>(configurator);
            var point = CreateNonTerminal<Point>(configurator);
            var vector = CreateNonTerminal<Vector>(configurator);
            var matrix = CreateNonTerminal<Matrix4x4>(configurator);
            var floatOrInt = CreateNonTerminal<float>(configurator);
            var activeTransformType = CreateNonTerminal<ActiveTransformType>(configurator);

            // Terminals.

            var floatLiteral = CreateTerminalParse(configurator, @"-?\d*\.\d+", Convert.ToSingle);
            var integerLiteral = CreateTerminalParse(configurator, @"-?\d+", Convert.ToInt32);
            var quotedString = CreateTerminalParse(configurator, "\"(\\\\.|[^\"])*\"", x => x.Substring(1, x.Length - 2));

            // Productions.

            sceneFile.Add(directiveList).SetReduceToFirst();

            directiveList.Add(directiveList, directive).SetReduce((a, b) => a.Concat(new[] { b }).ToArray());
            directiveList.Add(directive).SetReduce(a => new[] { a });

            directive.Add(standardDirectiveType, quotedString, paramSet).SetReduce(
                (a, b, c) => new StandardDirective
                {
                    DirectiveType = a,
                    ImplementationType = b,
                    Parameters = new ParamSet(c)
                });
            directive.Add(standardDirectiveType, quotedString).SetReduce(
                (a, b) => new StandardDirective
                {
                    DirectiveType = a,
                    ImplementationType = b,
                    Parameters = new ParamSet()
                });
            directive.Add(CreateTerminal(configurator, "Texture"), quotedString, quotedString, quotedString, paramSet).SetReduce(
                (_, b, c, d, e) => new TextureDirective
                {
                    Name = b,
                    TextureType = c,
                    TextureClass = d,
                    Parameters = new ParamSet(e)
                });
            directive.Add(CreateTerminal(configurator, "Texture"), quotedString, quotedString, quotedString).SetReduce(
                (_, b, c, d) => new TextureDirective
                {
                    Name = b,
                    TextureType = c,
                    TextureClass = d,
                    Parameters = new ParamSet()
                });
            directive.Add(CreateTerminal(configurator, "Identity")).SetReduce(_ => new IdentityDirective());
            directive.Add(CreateTerminal(configurator, "Translate"), vector).SetReduce(
                (_, b) => new TranslateDirective
                {
                    Translation = b
                });
            directive.Add(CreateTerminal(configurator, "Scale"), vector).SetReduce(
                (_, b) => new ScaleDirective
                {
                    Scale = b
                });
            directive.Add(CreateTerminal(configurator, "Rotate"), floatOrInt, vector).SetReduce(
                (_, b, c) => new RotateDirective
                {
                    Angle = b,
                    Axis = c
                });
            directive.Add(CreateTerminal(configurator, "LookAt"), point, point, vector).SetReduce(
                (_, b, c, d) => new LookAtDirective
                {
                    Eye = b,
                    LookAt = c,
                    Up = d
                });
            directive.Add(CreateTerminal(configurator, "CoordinateSystem"), quotedString).SetReduce(
                (_, b) => new CoordinateSystemDirective
                {
                    Name = b
                });
            directive.Add(CreateTerminal(configurator, "CoordSysTransform"), quotedString).SetReduce(
                (_, b) => new CoordSysTransformDirective
                {
                    Name = b
                });
            directive.Add(CreateTerminal(configurator, "Transform"), matrix).SetReduce(
                (_, b) => new TransformDirective
                {
                    Transform = b
                });
            directive.Add(CreateTerminal(configurator, "ConcatTransform"), matrix).SetReduce(
                (_, b) => new ConcatTransformDirective
                {
                    Transform = b
                });
            directive.Add(CreateTerminal(configurator, "TransformTimes"), floatOrInt, floatOrInt).SetReduce(
                (_, b, c) => new TransformTimesDirective
                {
                    Start = b,
                    End = c
                });
            directive.Add(CreateTerminal(configurator, "ActiveTransform"), activeTransformType).SetReduce(
                (_, b) => new ActiveTransformDirective
                {
                    Type = b
                });
            directive.Add(CreateTerminal(configurator, "MakeNamedMaterial"), quotedString, quotedString, paramSet).SetReduce(
                (_, b, c, d) => new MakeNamedMaterialDirective
                {
                    MaterialName = b,
                    MaterialType = c,
                    Parameters = new ParamSet(d)
                });
            directive.Add(CreateTerminal(configurator, "NamedMaterial"), quotedString).SetReduce(
                (_, b) => new NamedMaterialDirective { MaterialName = b });
            directive.Add(CreateTerminal(configurator, "ObjectBegin"), quotedString).SetReduce(
                (_, b) => new ObjectBeginDirective { Name = b });
            directive.Add(CreateTerminal(configurator, "ObjectEnd")).SetReduce(_ => new ObjectEndDirective());
            directive.Add(CreateTerminal(configurator, "ObjectInstance"), quotedString).SetReduce(
                (_, b) => new ObjectInstanceDirective { Name = b });
            directive.Add(CreateTerminal(configurator, "ReverseOrientation")).SetReduce(_ => new ReverseOrientationDirective());
            directive.Add(CreateTerminal(configurator, "TransformBegin")).SetReduce(_ => new TransformBeginDirective());
            directive.Add(CreateTerminal(configurator, "TransformEnd")).SetReduce(_ => new TransformEndDirective());
            directive.Add(CreateTerminal(configurator, "WorldBegin")).SetReduce(_ => new WorldBeginDirective());
            directive.Add(CreateTerminal(configurator, "WorldEnd")).SetReduce(_ => new WorldEndDirective());
            directive.Add(CreateTerminal(configurator, "AttributeBegin")).SetReduce(_ => new AttributeBeginDirective());
            directive.Add(CreateTerminal(configurator, "AttributeEnd")).SetReduce(_ => new AttributeEndDirective());

            standardDirectiveType.Add(CreateTerminal(configurator, "Accelerator")).SetReduce(_ => StandardDirectiveType.Accelerator);
            standardDirectiveType.Add(CreateTerminal(configurator, "AreaLightSource")).SetReduce(_ => StandardDirectiveType.AreaLightSource);
            standardDirectiveType.Add(CreateTerminal(configurator, "Camera")).SetReduce(_ => StandardDirectiveType.Camera);
            standardDirectiveType.Add(CreateTerminal(configurator, "Film")).SetReduce(_ => StandardDirectiveType.Film);
            standardDirectiveType.Add(CreateTerminal(configurator, "LightSource")).SetReduce(_ => StandardDirectiveType.LightSource);
            standardDirectiveType.Add(CreateTerminal(configurator, "Material")).SetReduce(_ => StandardDirectiveType.Material);
            standardDirectiveType.Add(CreateTerminal(configurator, "PixelFilter")).SetReduce(_ => StandardDirectiveType.PixelFilter);
            standardDirectiveType.Add(CreateTerminal(configurator, "Renderer")).SetReduce(_ => StandardDirectiveType.Renderer);
            standardDirectiveType.Add(CreateTerminal(configurator, "Sampler")).SetReduce(_ => StandardDirectiveType.Sampler);
            standardDirectiveType.Add(CreateTerminal(configurator, "Shape")).SetReduce(_ => StandardDirectiveType.Shape);
            standardDirectiveType.Add(CreateTerminal(configurator, "SurfaceIntegrator")).SetReduce(_ => StandardDirectiveType.SurfaceIntegrator);
            standardDirectiveType.Add(CreateTerminal(configurator, "Volume")).SetReduce(_ => StandardDirectiveType.Volume);
            standardDirectiveType.Add(CreateTerminal(configurator, "VolumeIntegrator")).SetReduce(_ => StandardDirectiveType.VolumeIntegrator);

            paramSet.Add(paramSet, param).SetReduce((a, b) => a.Concat(new[] { b }).ToArray());
            paramSet.Add(param).SetReduce(a => new[] { a });

            param.Add(quotedString, CreateTerminal(configurator, @"\["), paramValueList, CreateTerminal(configurator, @"\]"))
                .SetReduce((a, b, c, d) => GetParamList(a, c));
            param.Add(quotedString, paramValue).SetReduce(GetParam);

            paramValueList.Add(paramValueList, paramValue).SetReduce((a, b) => a.Concat(new[] { b }).ToArray());
            paramValueList.Add(paramValue).SetReduce(a => new [] { a });

            paramValue.Add(floatOrInt).SetReduce(a => a);
            paramValue.Add(quotedString).SetReduce(a => a);

            point.Add(floatOrInt, floatOrInt, floatOrInt).SetReduce((a, b, c) => new Point(a, b, c));
            vector.Add(floatOrInt, floatOrInt, floatOrInt).SetReduce((a, b, c) => new Vector(a, b, c));
            matrix.Add(floatOrInt, floatOrInt, floatOrInt, floatOrInt, floatOrInt, floatOrInt, floatOrInt, floatOrInt, floatOrInt, floatOrInt, floatOrInt, floatOrInt, floatOrInt, floatOrInt, floatOrInt, floatOrInt)
                .SetReduce((m00, m01, m02, m03, m10, m11, m12, m13, m20, m21, m22, m23, m30, m31, m32, m33) => new Matrix4x4(m00, m01, m02, m03, m10, m11, m12, m13, m20, m21, m22, m23, m30, m31, m32, m33));

            floatOrInt.Add(floatLiteral).SetReduceToFirst();
            floatOrInt.Add(integerLiteral).SetReduce(a => (float) a);

            activeTransformType.Add(CreateTerminal(configurator, "StartTime")).SetReduce(x => ActiveTransformType.Start);
            activeTransformType.Add(CreateTerminal(configurator, "EndTime")).SetReduce(x => ActiveTransformType.End);
            activeTransformType.Add(CreateTerminal(configurator, "All")).SetReduce(x => ActiveTransformType.All);

            // Ignore whitespace and comments
            configurator.LexerSettings.Ignore = new[] { @"\s+", @"#[^\n]*\n" };

            _parser = configurator.CreateParser();
        }

        public SceneFile Parse(string pbrtCode)
        {
            return new SceneFile((Directive[]) _parser.Parse(pbrtCode));
        }

        private static NonTerminalWrapper<T> CreateNonTerminal<T>(IParserConfigurator<object> configurator)
        {
            return new NonTerminalWrapper<T>(configurator.CreateNonTerminal());
        }

        private static TerminalWrapper<T> CreateTerminalParse<T>(
            IParserConfigurator<object> configurator,
            string regex, Func<string, T> onParse)
        {
            return new TerminalWrapper<T>(configurator.CreateTerminal(regex, x => onParse(x)));
        }

        private static TerminalWrapper<string> CreateTerminal(
            IParserConfigurator<object> configurator,
            string regex)
        {
            return new TerminalWrapper<string>(configurator.CreateTerminal(regex));
        }

        private static Param GetParamList(string typeAndName, object[] value)
        {
            var newValue = (value.Length == 1)
                ? value[0]
                : value;
            return GetParam(typeAndName, newValue);
        }

        private static Param GetParam(string typeAndName, object value)
        {
            var splitNameAndType = typeAndName.Split(' ');
            var paramType = splitNameAndType[0];
            var paramName = splitNameAndType[1];

            var newValue = GetParamValue(paramType, value);

            return new Param
            {
                Name = paramName,
                Value = newValue
            };
        }

        private static object GetParamValue(string paramType, object value)
        {
            float x, y, z;

            switch (paramType)
            {
                case "integer" :
                    if (value is float)
                        return (int) (float) value;
                    return value;
                case "bool" :
                    return Convert.ToBoolean(value);
                case "point" :
                    GetThreeSingles(value, out x, out y, out z);
                    return new Point(x, y, z);
                case "vector":
                    GetThreeSingles(value, out x, out y, out z);
                    return new Vector(x, y, z);
                case "normal":
                    GetThreeSingles(value, out x, out y, out z);
                    return new Normal(x, y, z);
                case "rgb" :
                case "color":
                    GetThreeSingles(value, out x, out y, out z);
                    return new Spectrum(x, y, z);
                default :
                    return value;
            }
        }

        private static void GetThreeSingles(object value, out float x, out float y, out float z)
        {
            var list = (object[]) value;
            if (list.Length != 3 || !list.All(a => a is float))
                throw new ArgumentOutOfRangeException("Expected three floats in an array.");

            x = (float) list[0];
            y = (float) list[1];
            z = (float) list[2];
        }
    }
}