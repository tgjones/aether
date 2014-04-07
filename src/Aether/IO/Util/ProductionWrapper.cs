using System;
using Piglet.Parser.Configuration;
using Piglet.Parser.Construction;

namespace Aether.IO.Util
{
    public abstract class ProductionWrapperBase
    {
        private readonly IProduction<object> _production;

        protected ProductionWrapperBase(IProduction<object> production)
        {
            _production = production;
        }

        public IProduction<object> Production
        {
            get { return _production; }
        }

        public void SetReduceToFirst()
        {
            _production.SetReduceToFirst();
        }

        public void SetPrecedence(IPrecedenceGroup precedenceGroup)
        {
            _production.SetPrecedence(precedenceGroup);
        }
    }

    public class ProductionWrapper<TResult> : ProductionWrapperBase
    {
        public ProductionWrapper(IProduction<object> production)
            : base(production)
        {
        }

        public void SetReduce(Func<TResult> func)
        {
            Production.SetReduceFunction(o => func());
        }
    }

    public class ProductionWrapper<T1, TResult> : ProductionWrapperBase
    {
        public ProductionWrapper(IProduction<object> production)
            : base(production)
        {
        }

        public void SetReduce(Func<T1, TResult> func)
        {
            Production.SetReduceFunction(o => func((T1) o[0]));
        }
    }

    public class ProductionWrapper<T1, T2, TResult> : ProductionWrapperBase
    {
        public ProductionWrapper(IProduction<object> production)
            : base(production)
        {
        }

        public void SetReduce(Func<T1, T2, TResult> func)
        {
            Production.SetReduceFunction(o => func((T1) o[0], (T2) o[1]));
        }
    }

    public class ProductionWrapper<T1, T2, T3, TResult> : ProductionWrapperBase
    {
        public ProductionWrapper(IProduction<object> production)
            : base(production)
        {
        }

        public void SetReduce(Func<T1, T2, T3, TResult> func)
        {
            Production.SetReduceFunction(o => func((T1) o[0], (T2) o[1], (T3) o[2]));
        }
    }

    public class ProductionWrapper<T1, T2, T3, T4, TResult> : ProductionWrapperBase
    {
        public ProductionWrapper(IProduction<object> production)
            : base(production)
        {
        }

        public void SetReduce(Func<T1, T2, T3, T4, TResult> func)
        {
            Production.SetReduceFunction(o => func((T1) o[0], (T2) o[1], (T3) o[2], (T4) o[3]));
        }
    }

    public class ProductionWrapper<T1, T2, T3, T4, T5, TResult> : ProductionWrapperBase
    {
        public ProductionWrapper(IProduction<object> production)
            : base(production)
        {
        }

        public void SetReduce(Func<T1, T2, T3, T4, T5, TResult> func)
        {
            Production.SetReduceFunction(o => func((T1) o[0], (T2) o[1], (T3) o[2], (T4) o[3], (T5) o[4]));
        }
    }

    public class ProductionWrapper<T1, T2, T3, T4, T5, T6, TResult> : ProductionWrapperBase
    {
        public ProductionWrapper(IProduction<object> production)
            : base(production)
        {
        }

        public void SetReduce(Func<T1, T2, T3, T4, T5, T6, TResult> func)
        {
            Production.SetReduceFunction(o => func((T1) o[0], (T2) o[1], (T3) o[2], (T4) o[3], (T5) o[4], (T6) o[5]));
        }
    }

    public class ProductionWrapper<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> : ProductionWrapperBase
    {
        public ProductionWrapper(IProduction<object> production)
            : base(production)
        {
        }

        public void SetReduce(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> func)
        {
            Production.SetReduceFunction(o => func(
                (T1) o[0], (T2) o[1], (T3) o[2], (T4) o[3], (T5) o[4], (T6) o[5], (T7) o[6], (T8) o[7],
                (T9) o[8], (T10) o[9], (T11) o[10], (T12) o[11], (T13) o[12], (T14) o[13], (T15) o[14], (T16) o[15]));
        }
    }
}