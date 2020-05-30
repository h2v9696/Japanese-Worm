using UnityEngine;
using UnityEditor;
using EditorUtils;

[CustomEditor(typeof(AdAssistant))]
public class AdAssistantEditor : MetaEditor
{

    AdAssistant main;

    public override Object FindTarget()
    {
        return AdAssistant.Instance;
    }

    public override void OnInspectorGUI()
    {
        if (!metaTarget)
        {
            EditorGUILayout.HelpBox("AdAssistant is missing", MessageType.Error);
            return;
        }
        main = (AdAssistant)metaTarget;
        Undo.RecordObject(main, "Ad settings changed");
        EditorGUILayout.Space();

        GUILayout.Space(20);

        main.AdMob_Interstitial_Android = EditorGUILayout.TextField("Android Interstitial ID", main.AdMob_Interstitial_Android);
        main.AdMob_Interstitial_iOS = EditorGUILayout.TextField("iOS Interstitial ID", main.AdMob_Interstitial_iOS);
        GUILayout.Space(20);
        main.AdMob_Baner_Android = EditorGUILayout.TextField("Android Banner ID", main.AdMob_Baner_Android);
        main.AdMob_Baner_iOS = EditorGUILayout.TextField("iOS Banner ID", main.AdMob_Baner_iOS);

    }
}
