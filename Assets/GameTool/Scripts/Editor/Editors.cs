using UnityEngine;
using System.Collections;
using EditorUtils;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public class NativeShareEditor : MetaEditor {
    public override Object FindTarget() {
        return NativeShareMgr.Instance;
    }

    public override void OnInspectorGUI() {
        if (!metaTarget) {
            EditorGUILayout.HelpBox("SessionAssistant is missing", MessageType.Error);
            return;
        }

        NativeShareMgr main = (NativeShareMgr) metaTarget;
        Undo.RecordObject(main, "");



        EditorGUILayout.Space();
        main.subject = EditorGUILayout.TextField("Subject", main.subject);

        EditorGUILayout.Space();
        main.urlAndroid = EditorGUILayout.TextField("URL Android", main.urlAndroid);

        EditorGUILayout.Space();
        main.urlIOS = EditorGUILayout.TextField("URL IOS", main.urlIOS);
        EditorGUILayout.Space();
        GUILayout.Label("Text Share", GUILayout.Width(300));
        main.textShare = EditorGUILayout.TextArea(main.textShare, GUI.skin.textArea, GUILayout.ExpandWidth(true), GUILayout.Height(300));

     
        EditorGUILayout.Space();
    }
}

public class ProjectParametersEditor : MetaEditor {
    public override Object FindTarget() {
        return ProjectParameters.Instance;
    }

    public override void OnInspectorGUI() {
        if (!metaTarget) {
            EditorGUILayout.HelpBox("SessionAssistant is missing", MessageType.Error);
            return;
        }

        ProjectParameters main = (ProjectParameters) metaTarget;
        Undo.RecordObject(main, "");

       

        EditorGUILayout.Space();
        main.ios_AppID = EditorGUILayout.TextField("iOS AppID", main.ios_AppID);

      
        EditorGUILayout.Space();
    }
}