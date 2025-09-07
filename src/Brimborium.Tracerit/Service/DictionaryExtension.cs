namespace System.Collections.Generic;

public static class DictionaryExtension {
    public static void CopyFrom<TKey, TValue>(this Dictionary<TKey,TValue> that, Dictionary<TKey, TValue> src)
        where TKey : notnull {
        foreach (var kv in src) {
            that[kv.Key] = kv.Value;
        }
    }
}