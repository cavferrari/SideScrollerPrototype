using UnityEngine;
using System.Collections;

public class DefaultSpreadController : SpreadController {
    /// <summary>
    /// (x=0) -> start spread ; (x=1) -> max spread (in degree)
    /// </summary>
    public AnimationCurve SpreadAngleCurve=new AnimationCurve();

    /// <summary>
    /// Time to go from start- to max-spread
    /// </summary>
    public float AttackTime;

    /// <summary>
    /// Time until spread release
    /// </summary>
    public float HoldTime;

    /// <summary>
    /// Time to go from max- to start-spread
    /// </summary>
    public float ReleaseTime;

    /// <summary>
    /// between 0-1; is the current "time" at the SpreadAngleCurve
    /// </summary>
    [HideInInspector]
    public float currentSpread;

    private float shootTimer=0;

    [HideInInspector]
    private float baseSpread;

    /// <summary>
    /// Calculations for defaultSpreadModel
    /// </summary>
    void SpreadCalc()
    {
        currentSpread = Mathf.Clamp01(shootTimer > 0 ? currentSpread + Time.deltaTime / AttackTime : currentSpread - Time.deltaTime / ReleaseTime);
        shootTimer -= Time.deltaTime;
    }

    void Update()
    {
        SpreadCalc();
    }
     /// <summary>
     /// called from the weapon when it shoots
     /// </summary>
    public override void onShoot()
    {
        shootTimer = HoldTime;
    }

    public override Vector3 GetCurrentSpread(Transform spawn)
    {
        float spread = SpreadAngleCurve.Evaluate(currentSpread) / 2 + baseSpread;
        return (Quaternion.AngleAxis(Random.Range(0,360),spawn.forward)*(Quaternion.AngleAxis(Random.Range(0,spread),spawn.right)) * spawn.forward);
    }

    public override float GetSpreadAngle() {
        return SpreadAngleCurve.Evaluate(currentSpread);
    }

    public void setBaseSpread(float spread)
    {
        baseSpread = spread;
    }
}
