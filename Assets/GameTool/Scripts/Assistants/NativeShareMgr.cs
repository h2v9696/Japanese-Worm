using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;


public class NativeShareMgr : SingletonMonoBehaviour<NativeShareMgr>
{
    [SerializeField]
    public string subject = "";
    [SerializeField]
    public string textShare = "";
    [SerializeField]
    public string urlAndroid = "";
    [SerializeField]
    public string urlIOS = "";


    public void ShareLink()
    {
        string url = "";

#if UNITY_ANDROID
        url = urlAndroid;
#elif UNITY_IOS
    url = urlIOS;
#endif
        Share(textShare + url, subject);
    }

    void Share(string url, string subject)
    {
#if UNITY_ANDROID
        AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
        AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");

        intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
        intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), url);
        intentObject.Call<AndroidJavaObject>("setType", "text/plain");

        AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");

        AndroidJavaObject jChooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intentObject, subject);
        currentActivity.Call("startActivity", jChooser);
#elif UNITY_IOS
		CallSocialShareAdvanced(url, subject, "", "");
#else
        Debug.Log("No sharing set up for this platform.");
#endif
    }

#if UNITY_IOS
	public struct ConfigStruct
	{
		public string title;
		public string message;
	}

	[DllImport ("__Internal")] private static extern void showAlertMessage(ref ConfigStruct conf);
	
	public struct SocialSharingStruct
	{
		public string text;
		public string url;
		public string image;
		public string subject;
	}
	
	[DllImport ("__Internal")] private static extern void showSocialSharing(ref SocialSharingStruct conf);
	
	public static void CallSocialShare(string title, string message)
	{
		ConfigStruct conf = new ConfigStruct();
		conf.title  = title;
		conf.message = message;
		showAlertMessage(ref conf);
	}

	public static void CallSocialShareAdvanced(string defaultTxt, string subject, string url, string img)
	{
		SocialSharingStruct conf = new SocialSharingStruct();
		conf.text = defaultTxt; 
		conf.url = url;
		conf.image = img;
		conf.subject = subject;
		
		showSocialSharing(ref conf);
	}
#endif
}
