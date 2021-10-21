using MachineClassLibrary.Machine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;
//using netDxf;

namespace MachineClassLibrary.Classes
{
    internal static class StatMethods
    {
        public static void WriteObject<T>(this object obj, string file)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));

            using FileStream fileStream = new FileStream(file, FileMode.OpenOrCreate);
            serializer.Serialize(fileStream, obj);
        }
        public static T ReadObject<T>(this T obj, string file)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using FileStream fileStream = new FileStream(file, FileMode.Open);
            return (T)serializer.Deserialize(fileStream);
        }

        public static TV GetItemByIndex<TK, TV>(this Dictionary<TK, TV> dictionary, int index)
        {
            int count = dictionary.Count;
            if (index < 0 || index > count - 1)
                throw new Exception("Индекс вне диапазона коллекции");
            var keys = new TK[count];
            dictionary.Keys.CopyTo(keys, 0);
            return dictionary[keys[index]];
        }
        public static void CopyPropertiesTo<T>(this T source, T dest)
        {
            var plist = from prop in typeof(T).GetProperties() where prop.CanRead && prop.CanWrite select prop;

            foreach (var prop in plist)
            {
                prop.SetValue(dest, prop.GetValue(source, null), null);
            }
        }

        public static async Task SerializeObjectJsonAsync(this object obj, string filename)
        {
            await using var file = File.Create(filename);
            await JsonSerializer.SerializeAsync(file, obj).ConfigureAwait(false);
        }

        public static void SerializeObjectJson(this object obj, string filename)
        {
            var json = JsonSerializer.Serialize(obj);
            File.WriteAllText(filename, json);
        }

        public static async Task<T> DeSerializeObjectJsonAsync<T>(string filename)
        {
            await using var file = File.OpenRead(filename);
            return await JsonSerializer.DeserializeAsync<T>(file).ConfigureAwait(false);
        }

        public static T DeSerializeObjectJson<T>(string filename)
        {
            var file = File.ReadAllText(filename);

            return JsonSerializer.Deserialize<T>(file);
        }

        ///// <summary>
        ///// Преобразует Vector3 в Vector2 отсекая Z
        ///// </summary>
        ///// <param name="vector"></param>
        ///// <returns></returns>
        //public static Vector2 SplitZ(this Vector3 vector)
        //{
        //    return new Vector2(vector.X, vector.Y);
        //}
        public static int SetBit(this int variable, int pos)
        {
            var res = variable | 1 << pos;
            return res;
        }
        public static int ResetBit(this int variable, int pos)
        {
            var res = variable & ~(1 << pos);
            return res;
        }

        public static double GetVal(this (Ax num, double val)[] arr, Ax @enum)
        {
            return arr.Where(n => n.num == @enum).Select(v => v.val).First();
        }

        public static int FindIndex<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }
            if (predicate is null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            var n = 0;
            foreach (var item in items)
            {
                if (predicate(item))
                {
                    return n;
                }
                n++;
            }
            return -1;
        }
        public static decimal Map(this decimal value, decimal fromSource, decimal toSource, decimal fromTarget, decimal toTarget)
        {
            return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
        }
    }
}
