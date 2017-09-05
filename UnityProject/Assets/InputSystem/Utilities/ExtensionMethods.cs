using System;
using System.Collections.Generic;

public static class ExtensionMethods
{
    public static List<T> SemiDeepClone<T>(this List<T> list) where T : System.ICloneable
    {
        var clone = new List<T>(list.Count);
        for (int i = 0; i < list.Count; i++)
        {
            if (EqualityComparer<T>.Default.Equals(list[i], default(T)))
                clone.Add(default(T));
            else
                clone.Add((T)list[i].Clone());
        }
        return clone;
    }

    public static string GetNiceName(this Type t)
    {
        if (t.IsGenericType)
        {
            string nameBase = t.Name.Substring(0, t.Name.LastIndexOf("`", StringComparison.InvariantCulture));
            Type[] genericArguments = t.GetGenericArguments();
            string[] genericArgumentStrings = new string[genericArguments.Length];
            for (int i = 0; i < genericArguments.Length; i++)
                genericArgumentStrings[i] = GetNiceName(genericArguments[i]);
            return string.Format("{0}<{1}>", nameBase, string.Join(", ", genericArgumentStrings));
        }

        return t.Name;
    }
}
