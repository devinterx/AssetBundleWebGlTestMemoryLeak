using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Serialization;
using UnityEngine.UI;

#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

public class MemoryMonitor : MonoBehaviour
{
    [FormerlySerializedAs("MemoryOutput")]
    public Text memoryOutput;

    private void Start() {
        InvokeRepeating(nameof(Log), 0, 15);
    }
    
    private void Update()
    {
        if (memoryOutput == null) return;

        var total = GetTotalMemorySize() / 1024 / 1024;
        var used = GetUsedMemorySize() / 1024 / 1024;
        var free = GetFreeMemorySize() / 1024 / 1024;

        memoryOutput.text = $"Memory - Total: {total}MB, Used: {used}MB, Free: {free}MB";
    }

    private static void Log()
    {
        var total = GetTotalMemorySize() / 1024 / 1024;
        var used = GetUsedMemorySize() / 1024 / 1024;
        var free = GetFreeMemorySize() / 1024 / 1024;
        
        Debug.Log($"Memory - Total: {total}MB, Used: {used}MB, Free: {free}MB");
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    private static uint GetFreeMemorySize() {
        return GetTotalMemorySize() - GetUsedMemorySize();
    }

    private static uint GetUsedMemorySize()
    {
        return GetTotalStackSize() + GetStaticMemorySize() + GetDynamicMemorySize();
    }

    [DllImport("__Internal")]
    public static extern uint GetTotalMemorySize();

    [DllImport("__Internal")]
    public static extern uint GetTotalStackSize();

    [DllImport("__Internal")]
    public static extern uint GetStaticMemorySize();

    [DllImport("__Internal")]
    public static extern uint GetDynamicMemorySize();
#else
    private static uint GetUsedMemorySize()
    {
        return (uint) Profiler.GetTotalAllocatedMemoryLong();
    }

    private static uint GetFreeMemorySize()
    {
        return GetTotalMemorySize() - GetUsedMemorySize();
    }

    private static uint GetTotalMemorySize()
    {
        return (uint) Profiler.GetTotalReservedMemoryLong();
    }
#endif
}
