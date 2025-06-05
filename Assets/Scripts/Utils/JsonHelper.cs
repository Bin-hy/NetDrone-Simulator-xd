using System;
using UnityEngine;

public static class JsonHelper
{
    // 反序列化JSON数组
    public static T[] FromJson<T>(string jsonArray)
    {
        // 将数组包装成对象
        string wrapperJson = $"{{\"items\":{jsonArray}}}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(wrapperJson);
        return wrapper.items;
    }

    // 序列化为JSON数组
    public static string ToJson<T>(T[] array)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.items = array;
        return JsonUtility.ToJson(wrapper);
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] items;
    }
}