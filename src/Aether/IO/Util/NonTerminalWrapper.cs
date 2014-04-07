using Piglet.Parser.Configuration;

namespace Aether.IO.Util
{
    public class NonTerminalWrapper<TResult> : SymbolWrapper<TResult>
    {
        private readonly INonTerminal<object> _nonTerminal;

        public NonTerminalWrapper(INonTerminal<object> nonTerminal)
            : base(nonTerminal)
        {
            _nonTerminal = nonTerminal;
        }

        public ProductionWrapper<TResult> Add()
        {
            return new ProductionWrapper<TResult>(_nonTerminal.AddProduction());
        }

        public ProductionWrapper<T1, TResult> Add<T1>(SymbolWrapper<T1> part)
        {
            return new ProductionWrapper<T1, TResult>(_nonTerminal.AddProduction(part.Symbol));
        }

        public ProductionWrapper<T1, T2, TResult> Add<T1, T2>(
            SymbolWrapper<T1> part1, SymbolWrapper<T2> part2)
        {
            return new ProductionWrapper<T1, T2, TResult>(_nonTerminal.AddProduction(
                part1.Symbol, part2.Symbol));
        }

        public ProductionWrapper<T1, T2, T3, TResult> Add<T1, T2, T3>(
            SymbolWrapper<T1> part1,
            SymbolWrapper<T2> part2,
            SymbolWrapper<T3> part3)
        {
            return new ProductionWrapper<T1, T2, T3, TResult>(_nonTerminal.AddProduction(
                part1.Symbol, part2.Symbol, part3.Symbol));
        }

        public ProductionWrapper<T1, T2, T3, T4, TResult> Add<T1, T2, T3, T4>(
            SymbolWrapper<T1> part1,
            SymbolWrapper<T2> part2,
            SymbolWrapper<T3> part3,
            SymbolWrapper<T4> part4)
        {
            return new ProductionWrapper<T1, T2, T3, T4, TResult>(_nonTerminal.AddProduction(
                part1.Symbol, part2.Symbol, part3.Symbol, part4.Symbol));
        }

        public ProductionWrapper<T1, T2, T3, T4, T5, TResult> Add<T1, T2, T3, T4, T5>(
            SymbolWrapper<T1> part1,
            SymbolWrapper<T2> part2,
            SymbolWrapper<T3> part3,
            SymbolWrapper<T4> part4,
            SymbolWrapper<T5> part5)
        {
            return new ProductionWrapper<T1, T2, T3, T4, T5, TResult>(_nonTerminal.AddProduction(
                part1.Symbol, part2.Symbol, part3.Symbol, part4.Symbol, part5.Symbol));
        }

        public ProductionWrapper<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> Add<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(
            SymbolWrapper<T1> part1,
            SymbolWrapper<T2> part2,
            SymbolWrapper<T3> part3,
            SymbolWrapper<T4> part4,
            SymbolWrapper<T5> part5,
            SymbolWrapper<T6> part6,
            SymbolWrapper<T7> part7,
            SymbolWrapper<T8> part8,
            SymbolWrapper<T9> part9,
            SymbolWrapper<T10> part10,
            SymbolWrapper<T11> part11,
            SymbolWrapper<T12> part12,
            SymbolWrapper<T13> part13,
            SymbolWrapper<T14> part14,
            SymbolWrapper<T15> part15,
            SymbolWrapper<T16> part16)
        {
            return new ProductionWrapper<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>(_nonTerminal.AddProduction(
                part1.Symbol, part2.Symbol, part3.Symbol, part4.Symbol, part5.Symbol, part6.Symbol, part7.Symbol, part8.Symbol,
                part9.Symbol, part10.Symbol, part11.Symbol, part12.Symbol, part13.Symbol, part14.Symbol, part15.Symbol, part16.Symbol));
        }
    }
}