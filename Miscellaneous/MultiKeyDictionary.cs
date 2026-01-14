using System.Collections.Generic;
using System.Linq;

namespace MachineClassLibrary.Miscellaneous;

internal class MultiKeyDictionary<K, E>
{
    private readonly Dictionary<K, E> _mainElements = new();
    private readonly IEnumerable<IGrouping<K, K>> _keyGroups;

    public MultiKeyDictionary(IEnumerable<IGrouping<K, K>> keyGroups)
    {
        _keyGroups = keyGroups;
    }

    public E this[K someKey]
    {
        get
        {
            var key = GetKey(someKey);
            return _mainElements[key];
        }
        set
        {
            var key = GetKey(someKey);
            _mainElements[key] = value;
        }
    }

    private K GetKey(K someKey) => _keyGroups.Single(g => g.Any(e => e.Equals(someKey))).Key;
}
