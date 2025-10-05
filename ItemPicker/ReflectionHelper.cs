using System;
using System.Reflection;

public static class ReflectionHelper
{
    // Get a private or public field
    public static T GetField<T>(this object obj, string fieldName)
    {
        var field = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (field == null)
            throw new ArgumentException($"Field '{fieldName}' not found on type {obj.GetType()}.");
        return (T)field.GetValue(obj);
    }

    // Set a private or public field
    public static void SetField<T>(this object obj, string fieldName, T value)
    {
        var field = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (field == null)
            throw new ArgumentException($"Field '{fieldName}' not found on type {obj.GetType()}.");
        field.SetValue(obj, value);
    }

    // Get a private or public property
    public static T GetProperty<T>(this object obj, string propertyName)
    {
        var prop = obj.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (prop == null)
            throw new ArgumentException($"Property '{propertyName}' not found on type {obj.GetType()}.");
        return (T)prop.GetValue(obj);
    }

    // Set a private or public property
    public static void SetProperty<T>(this object obj, string propertyName, T value)
    {
        var prop = obj.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (prop == null)
            throw new ArgumentException($"Property '{propertyName}' not found on type {obj.GetType()}.");
        prop.SetValue(obj, value);
    }

    // Get a member (property or field)
    public static T GetMember<T>(this object obj, string memberName)
    {
        var type = obj.GetType();

        var prop = type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (prop != null)
            return (T)prop.GetValue(obj);

        var field = type.GetField(memberName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (field != null)
            return (T)field.GetValue(obj);

        throw new ArgumentException($"No property or field named '{memberName}' found on type {type}.");
    }

    // Set a member (property or field)
    public static void SetMember<T>(this object obj, string memberName, T value)
    {
        var type = obj.GetType();

        var prop = type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (prop != null)
        {
            prop.SetValue(obj, value);
            return;
        }

        var field = type.GetField(memberName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (field != null)
        {
            field.SetValue(obj, value);
            return;
        }

        throw new ArgumentException($"No property or field named '{memberName}' found on type {type}.");
    }

    // Invoke a method (private or public)
    public static object Call(this object obj, string methodName, params object[] parameters)
    {
        var method = obj.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (method == null)
            throw new ArgumentException($"Method '{methodName}' not found on type {obj.GetType()}.");
        return method.Invoke(obj, parameters);
    }

    // Get all fields (with optional private access)
    public static FieldInfo[] GetFields(this object obj, bool includePublic = true)
    {
        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        if (includePublic)
            flags |= BindingFlags.Public;
        return obj.GetType().GetFields(flags);
    }

    // Get all properties (with optional private access)
    public static PropertyInfo[] GetProperties(this object obj, bool includePublic = true)
    {
        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        if (includePublic)
            flags |= BindingFlags.Public;
        return obj.GetType().GetProperties(flags);
    }

    // Get all methods (with optional private access)
    public static MethodInfo[] GetMethods(this object obj, bool includePublic = true)
    {
        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        if (includePublic)
            flags |= BindingFlags.Public;
        return obj.GetType().GetMethods(flags);
    }
}
