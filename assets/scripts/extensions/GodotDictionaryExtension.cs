using Godot;
using Godot.Collections;

namespace ExtensionMethods
{
    public static class GodotDictionaryExtensionMethods
    {
        public static Variant GetOrNull(this Dictionary dictionary, string key) {
            if (dictionary.ContainsKey(key)) return dictionary[key];
            Variant variant = new();
            return variant.As<Variant>();
        }
    }
}