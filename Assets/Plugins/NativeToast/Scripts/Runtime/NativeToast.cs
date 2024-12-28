#if UNITY_IPHONE
using System.Runtime.InteropServices;
#endif

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

using UnityEngine;


public class NativeToast : MonoBehaviour
{
#if UNITY_IOS
    [DllImport ("__Internal")]
    private static extern void MakeToast(string message);
#endif

    /// <summary>
    /// Show a Toast message from Android.
    /// </summary>
    /// <param name="message">the message you want to show</param>
    public static void Toast(string message)
    {
#if UNITY_EDITOR
        Debug.Log(message);
#elif UNITY_ANDROID
        AndroidJavaObject activity = new AndroidJavaObject("com.nkstudio.plugin.NativeToast");
        activity.Call("MakeToast", message);
#elif UNITY_IOS
		MakeToast(message);
#endif
    }
}