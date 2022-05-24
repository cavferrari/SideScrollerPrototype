using UnityEngine;
using System.Collections;
using Ballistics;

/// <summary>
/// example for changing wind values in runtime
/// </summary>
public class DefaultWindManager : MonoBehaviour {

    public BallisticsSettings Settings;
    public Vector3 MainWind = new Vector3(1f,0f,0.1f);
    public float MainWindStrength = 3f;
    public float RandomWindStrength = 1.5f;
    public float Speed = 0.5f;

    private Vector3 ToDirection;

    private Transform myTrans;

    void Awake()
    {
        MainWind.Normalize();
        Settings.WindDirection = ToDirection = MainWind * MainWindStrength;
        myTrans = transform;
    }

    public void Update()
    {
        if ((ToDirection - Settings.WindDirection).sqrMagnitude < .05f)
        {
            ToDirection = MainWind * MainWindStrength + Random.onUnitSphere * (Random.Range(0, RandomWindStrength) - (RandomWindStrength / 2));
        }
        else
        {
            Settings.WindDirection = Vector3.Lerp(Settings.WindDirection, ToDirection, Speed * Time.deltaTime / (ToDirection - Settings.WindDirection).magnitude);
        }

        myTrans.rotation = Quaternion.LookRotation(Settings.WindDirection);
        myTrans.localScale = Vector3.one * Settings.WindDirection.magnitude;
    }

}
