#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class TypeGUI
{
    static Dictionary<Type, Type[]> s_AllBasesDerivedTypes = new Dictionary<Type, Type[]>();
    static Dictionary<Type, string[]> s_AllBasesDerivedNames = new Dictionary<Type, string[]>();
    static Dictionary<Type, Dictionary<Type, int>> s_AllBasesIndicesOfDerivedTypes = new Dictionary<Type, Dictionary<Type, int>>();

    public static Type TypeField(Rect position, GUIContent label, Type baseType, Type value)
    {
        Type[] derivedTypes = null;
        string[] derivedNames = null;
        Dictionary<Type, int> indicesOfDerivedTypes = null;
        GetDerivedTypesInfo(baseType, out derivedTypes, out derivedNames, out indicesOfDerivedTypes);

        EditorGUI.BeginChangeCheck();
        int derivedTypeIndex = EditorGUI.Popup(
                position,
                label.text,
                GetDerivedTypeIndex(value, indicesOfDerivedTypes),
                derivedNames);
        if (EditorGUI.EndChangeCheck())
            return derivedTypes[derivedTypeIndex];
        return value;
    }

    public static void GetDerivedTypesInfo(Type baseType, out Type[] derivedTypes, out string[] derivedNames, out Dictionary<Type, int> indicesOfDerivedTypes)
    {
        if (!s_AllBasesDerivedTypes.ContainsKey(baseType))
        {
            derivedTypes = GetAssignableTypes(baseType)
                .OrderBy(e => GetInheritancePath(e, baseType)).ToArray();

            derivedNames = derivedTypes.Select(
                    e =>
                    (
                        string.Empty.PadLeft(GetInheritanceDepth(e, baseType) * 3) +
                        ObjectNames.NicifyVariableName(e.Name)
                    )
                    ).ToArray();

            indicesOfDerivedTypes = new Dictionary<Type, int>();
            for (int i = 0; i < derivedTypes.Length; i++)
                indicesOfDerivedTypes[derivedTypes[i]] = i;

            s_AllBasesDerivedTypes[baseType] = derivedTypes;
            s_AllBasesDerivedNames[baseType] = derivedNames;
            s_AllBasesIndicesOfDerivedTypes[baseType] = indicesOfDerivedTypes;
        }
        else
        {
            derivedTypes = s_AllBasesDerivedTypes[baseType];
            derivedNames = s_AllBasesDerivedNames[baseType];
            indicesOfDerivedTypes = s_AllBasesIndicesOfDerivedTypes[baseType];
        }
    }

    static IEnumerable<Type> GetAssignableTypes(Type baseType)
    {
        if (!baseType.IsGenericType)
        {
            return (
                from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                from assemblyType in domainAssembly.GetExportedTypes()
                where baseType.IsAssignableFrom(assemblyType) && !assemblyType.IsAbstract && !assemblyType.IsGenericType
                select assemblyType
                );
        }

        Type[] genericArguments = baseType.GetGenericArguments();
        return (
            from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
            from assemblyType in domainAssembly.GetExportedTypes()
            where IsAssignableFrom(assemblyType, baseType, genericArguments) && !assemblyType.IsAbstract
            select assemblyType
            ).Select(t => t.IsGenericType ? t.GetGenericTypeDefinition().MakeGenericType(genericArguments) : t);
    }

    static Type[] s_ObjectTypeArray = new Type[] { typeof(object), typeof(object) };
    static bool IsAssignableFrom(Type type, Type baseType, Type[] baseTypeArguments)
    {
        // If type is not generic, we can just test if it's assignable the regular way.
        if (!type.IsGenericType)
            return baseType.IsAssignableFrom(type);

        // We only support generic types with same generic arguments as base.
        if (type.GetGenericArguments().Length != baseTypeArguments.Length)
            return false;

        // Make a constructed type with generic arguments matching those of the base type.
        // If it's not assignable,
        Type constructedType = type.MakeGenericType(baseTypeArguments);
        if (!baseType.IsAssignableFrom(constructedType))
            return false;

        Type constructedDummyType = type.MakeGenericType(s_ObjectTypeArray);
        if (baseType.IsAssignableFrom(constructedDummyType))
            return false;

        return true;
    }

    static int GetDerivedTypeIndex(Type type, Dictionary<Type, int> indicesOfDerivedTypes)
    {
        return (type == null ? -1 : indicesOfDerivedTypes[type]);
    }

    static string GetInheritancePath(Type type, Type baseType)
    {
        if (type == baseType)
            return string.Empty;
        if (type.BaseType == baseType)
            return type.Name;
        return GetInheritancePath(type.BaseType, baseType) + "/" + type.Name;
    }

    static int GetInheritanceDepth(Type type, Type baseType)
    {
        if (type == baseType)
            return 0;
        bool validBase = true;
        if (type.BaseType.IsAbstract)
            validBase = false;
        if (type.BaseType.IsGenericType && !baseType.IsGenericType)
            validBase = false;
        if (type.BaseType.IsGenericType && type.BaseType.GetGenericArguments()[0].IsGenericParameter)
            validBase = false;
        return GetInheritanceDepth(type.BaseType, baseType) + (validBase ? 1 : 0);
    }
}
#endif
