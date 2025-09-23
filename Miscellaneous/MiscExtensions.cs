using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace MachineClassLibrary.Miscellaneous;

public static class MiscExtensions
{
    public static void SerializeObject(this object obj, string filePath)
    {
        var json = JsonConvert.SerializeObject(obj);
        using var writer = new StreamWriter(filePath, false);
        var l = new TextWriterTraceListener(writer);
        l.WriteLine(json);
        l.Flush();
    }
    public static T? DeserializeObject<T>(string filePath)
    {
        if (filePath is null) return default;
        if(!File.Exists(filePath)) return default;
        try
        {
            var obj = JsonConvert.DeserializeObject(File.ReadAllText(filePath), typeof(T));
            return (T?)obj;
        }
        catch (FileNotFoundException)
        {
            return default;
        }
        catch(DirectoryNotFoundException)
        { return default; }
    }
    public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> en)
    {
        return new ObservableCollection<T>(en);
    }
    public static void AddSubscriptionTo(this IDisposable subscription, IList<IDisposable> subscriptions) => subscriptions?.Add(subscription);
    public static string GetDescription(this Enum value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        FieldInfo field = value.GetType().GetField(value.ToString());
        DescriptionAttribute attribute = field?.GetCustomAttribute<DescriptionAttribute>();
        return attribute?.Description ?? value.ToString();
    }
}
