using System;
using PythonLike;
using System.Runtime.InteropServices;

public static class ArrayExtension 
{
    internal static byte[] ToByteArray(this float[] data)
    {
        byte[] a = new byte[data.Length * sizeof(float)];
        IntPtr ptr = Marshal.AllocHGlobal(a.Length);
        Marshal.Copy(data, 0, ptr, data.Length);
        Marshal.Copy(ptr, a, 0, a.Length);
        Marshal.FreeHGlobal(ptr);
        return a;
    }

    internal static byte[] ToByteArray(this int[] data)
    {
        byte[] a = new byte[data.Length * sizeof(int)];
        IntPtr ptr = Marshal.AllocHGlobal(a.Length);
        Marshal.Copy(data, 0, ptr, data.Length);
        Marshal.Copy(ptr, a, 0, a.Length);
        Marshal.FreeHGlobal(ptr);
        return a;
    }
    //internal static T[] FromDataArray<T>(this byte[] data) where T : struct
    //{
    //    T[] a = new T[data.Length / Marshal.SizeOf<T>()];
    //    IntPtr ptr = Marshal.AllocHGlobal(a.Length);
    //    Marshal.Copy(data, 0, ptr, data.Length);
    //    if (typeof(T) == typeof(int))
    //    {
    //        int[] arr = (int[])(object)a;
    //        Marshal.Copy(ptr, arr, 0, a.Length);
    //    }
    //    else if (typeof(T) == typeof(float))
    //    {
    //        float[] arr = (float[])(object)a;
    //        Marshal.Copy(ptr, arr, 0, a.Length);
    //    }
    //    else if (typeof(T) == typeof(UnityEngine.Color))
    //    {
    //        UnityEngine.Color[] arr = (UnityEngine.Color[])(object)a;
    //        Marshal.Copy(ptr, arr, 0, a.Length);
    //    }
    //    Marshal.FreeHGlobal(ptr);
    //    return a;
    //}
    public static T[] FromByteArray<T>(this byte[] source) where T : struct
    {
        T[] destination = new T[source.Length / Marshal.SizeOf(typeof(T))];
        GCHandle handle = GCHandle.Alloc(destination, GCHandleType.Pinned);
        try
        {
            IntPtr pointer = handle.AddrOfPinnedObject();
            Marshal.Copy(source, 0, pointer, source.Length);
            return destination;
        }
        finally
        {
            if (handle.IsAllocated)
                handle.Free();
        }
    }
    public static T[] FromByteArray<T>(this byte[] source, int length) where T : struct
    {
        T[] destination = new T[length / Marshal.SizeOf(typeof(T))];
        GCHandle handle = GCHandle.Alloc(destination, GCHandleType.Pinned);
        try
        {
            IntPtr pointer = handle.AddrOfPinnedObject();
            Marshal.Copy(source, 0, pointer, length);
            return destination;
        }
        finally
        {
            if (handle.IsAllocated)
                handle.Free();
        }
    }
    public static void FromByteArray<T>(this byte[] source, ref T[] destination, int length) where T : struct
    {
        int newLength = length / Marshal.SizeOf(typeof(T));
        if (destination.Length != newLength)
            System.Array.Resize(ref destination, newLength);
        GCHandle handle = GCHandle.Alloc(destination, GCHandleType.Pinned);
        try
        {
            IntPtr pointer = handle.AddrOfPinnedObject();
            Marshal.Copy(source, 0, pointer, length);
        }
        finally
        {
            if (handle.IsAllocated)
                handle.Free();
        }
    }
    public static byte[] ToByteArray<T>(this T[] source) where T : struct
    {
        GCHandle handle = GCHandle.Alloc(source, GCHandleType.Pinned);
        try
        {
            IntPtr pointer = handle.AddrOfPinnedObject();
            byte[] destination = new byte[source.Length * Marshal.SizeOf(typeof(T))];
            Marshal.Copy(pointer, destination, 0, destination.Length);
            return destination;
        }
        finally
        {
            if (handle.IsAllocated)
                handle.Free();
        }
    }
    public static float[] ToFloatArray<T>(this T[] source) where T : struct
    {
        GCHandle handle = GCHandle.Alloc(source, GCHandleType.Pinned);
        try
        {
            IntPtr pointer = handle.AddrOfPinnedObject();
            int sz = Marshal.SizeOf(typeof(T));
            float[] destination = new float[source.Length * (sz / sizeof(float))];
            Marshal.Copy(pointer, destination, 0, destination.Length);
            return destination;
        }
        finally
        {
            if (handle.IsAllocated)
                handle.Free();
        }
    }

    public static object[] Unpack(this byte[] source, string format)
    {
        return StructConverter.Unpack(format, source);
    }
}
