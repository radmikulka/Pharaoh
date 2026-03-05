// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using System.Collections.Generic;

namespace TycoonBuilder
{
    public class CValueModifyParams
    {
        private readonly List<IValueModifyParam> _params = new();

        public IReadOnlyList<IValueModifyParam> Params => _params;

        public CValueModifyParams Add(IValueModifyParam param)
        {
            _params.Add(param);
            return this;
        }

        public T GetParamOrDefault<T>() where T : IValueModifyParam
        {
            foreach (IValueModifyParam param in _params)
            {
                if (param is T result)
                {
                    return result;
                }
            }

            return default;
        }
    }
}