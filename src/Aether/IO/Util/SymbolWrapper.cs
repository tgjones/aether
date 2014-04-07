using Piglet.Parser.Configuration;

namespace Aether.IO.Util
{
    public class SymbolWrapper<T>
    {
        private readonly ISymbol<object> _symbol;

        public SymbolWrapper(ISymbol<object> symbol)
        {
            _symbol = symbol;
        }

        public ISymbol<object> Symbol
        {
            get { return _symbol; }
        }
    }
}