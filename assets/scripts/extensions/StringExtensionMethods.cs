using Godot;

namespace ExtensionMethods;

public static class StringExtensionMethods
{
    /// <summary> Removes any BBCode tags from a string. </summary>
    public static string StripBBCode(this string s)
    {
        var regex = new RegEx();
        regex.Compile("\\[.+?\\]");
        return regex.Sub(s, "", true);
    }
}