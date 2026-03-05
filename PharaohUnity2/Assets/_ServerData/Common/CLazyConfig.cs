// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using System;

namespace ServerData
{
    public class CLazyConfig<T>
    {
        private T _config;
        private Func<T> _getter;

        public CLazyConfig(Func<T> getter)
        {
            _getter = getter;
        }

        public T GetConfig()
        {
            if (_config != null)
                return _config;
            _config = _getter();
            _getter = null;
            return _config;
        }
    }
}