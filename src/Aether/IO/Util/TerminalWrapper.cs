using Piglet.Parser.Configuration;

namespace Aether.IO.Util
{
    public class TerminalWrapper<T> : SymbolWrapper<T>
    {
        private readonly ITerminal<object> _terminal;

        public TerminalWrapper(ITerminal<object> terminal)
            : base(terminal)
        {
            _terminal = terminal;
        }

        public ITerminal<object> Terminal
        {
            get { return _terminal; }
        }
    }
}