using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetFileNicluder.Logic.Util.StaticExtensions
{
    public static class StaticDictionary
    {

        public static Nullable<V> GetValueOrDefault<K, V>(this IDictionary<K, V> dictionary, K key, Nullable<V> defaultValue = null)
            where V : struct
        {
            if (dictionary.ContainsKey(key)) return dictionary[key];
            else return defaultValue;
        }
    }
}
