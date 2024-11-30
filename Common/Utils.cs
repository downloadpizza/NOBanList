using System.Collections.Generic;
using UnityEngine.InputSystem.EnhancedTouch;

namespace NOBanList.Common {
    static class Utils {
        public static void AddAllToDict<K, V>(this Dictionary<K, V> dict, Dictionary<K, V> other) {
            foreach(var key in other.Keys) {
                dict[key] = other[key];
            }
        }
    }
}