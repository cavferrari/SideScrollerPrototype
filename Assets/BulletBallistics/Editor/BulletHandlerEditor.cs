using UnityEngine;
using System.Collections;
using UnityEditor;
using Ballistics;

[CustomEditor(typeof(BulletHandler))]
public class BulletHandlerEditor : Editor {

    void OnEnable()
    {
        BallisticSettingsManager.LoadSettings();
        ((BulletHandler)target).Settings = BallisticSettingsManager.Settings;
    }
}
