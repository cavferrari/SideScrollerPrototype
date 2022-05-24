using UnityEngine;
using System.Collections;
using System;
using Ballistics;

public class BasicWeaponController : MonoBehaviour {

    //General

    /// <summary>
    /// the weapon being controlled
    /// </summary>
    public Weapon TargetWeapon;

    /// <summary>
    /// delay between shots
    /// </summary>
    public float ShootDelay = 0.25f;


    /// <summary>
    /// Defines the type of weapon.
    /// </summary>
    public ShootingType WeaponType = ShootingType.SingleShot;


    //Savles / Burst

    /// <summary>
    /// amount of bullets per shot in salve- / burst- mode
    /// </summary>
    public int BulletsPerShot = 3;

    /// <summary>
    /// Additional random spread to each bullet in a burst
    /// </summary>
    public float BurstSpreadAngle = 5;
    //--

    //Salves
    /// <summary>
    /// delay between shots in a salve
    /// </summary>
    public float SalveBulletShootDelay = 0.04f;
    //--

    //Spread
    public SpreadController mySpreadController;
    //--

    //MagazineController
    public MagazineController myMagazineController;
    //--

    //private variables
    private bool shootReset = true;
    private int SalvesBulletCounter;
    private float CooldownTimer = 0;

    //--

    public bool isAiming;

    public Action OnShoot;

    void Update()
    {
        CooldownTimer -= Time.deltaTime;
    }

    /// <summary>
    /// Is the Weapon Aiming
    /// </summary>
    /// <param name="active"></param>
    public void Aim(bool active)
    {
        isAiming = active;
    }

    /// <summary>
    /// Fire the gun. Call this every frame the trigger is held down.
    /// </summary>
    public void Shoot()
    {
        if (myMagazineController.isBulletAvailable() && CooldownTimer <= 0)
        {
            switch (WeaponType)
            {
                case ShootingType.Auto:
                    TargetWeapon.ShootBullet(mySpreadController.GetCurrentSpread(TargetWeapon.PhysicalBulletSpawnPoint));
                    CallOnShoot();
                    break;
                case ShootingType.Burst:
                    if (shootReset)
                    {
                        for (int i = 0; i < BulletsPerShot; i++)
                        {
                            TargetWeapon.ShootBullet(mySpreadController.GetCurrentSpread(TargetWeapon.PhysicalBulletSpawnPoint));
                        }
                        CallOnShoot();
                        shootReset = false;
                    }
                    break;
                case ShootingType.Salves:

                    if (shootReset)
                    {
                        TargetWeapon.ShootBullet(mySpreadController.GetCurrentSpread(TargetWeapon.PhysicalBulletSpawnPoint));
                        SalvesBulletCounter++;
                        CallOnShoot();
                        shootReset = false;
                        StartCoroutine(ShootSalves());
                    }
                    break;
                case ShootingType.SingleShot:
                    if (shootReset)
                    {
                        TargetWeapon.ShootBullet(mySpreadController.GetCurrentSpread(TargetWeapon.PhysicalBulletSpawnPoint));
                        CallOnShoot();
                        shootReset = false;
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Coroutine to Spawn Bullets for shooting in salves
    /// </summary>
    IEnumerator ShootSalves()
    {
        while (SalvesBulletCounter < BulletsPerShot)
        {
            yield return new WaitForSeconds(SalveBulletShootDelay);
            TargetWeapon.ShootBullet(mySpreadController.GetCurrentSpread(TargetWeapon.PhysicalBulletSpawnPoint));
            SalvesBulletCounter++;
            CallOnShoot();
            if (SalvesBulletCounter >= BulletsPerShot)
            {
                CooldownTimer = ShootDelay;
                SalvesBulletCounter = 0;
                break;
            }
        }
    }

    private void CallOnShoot()
    {
        CooldownTimer = ShootDelay;
        ((DefaultMagazineController)myMagazineController).onShoot();
        mySpreadController.onShoot();
        if (OnShoot != null)
        {
            OnShoot();
        }
    }

    /// <summary>
    /// Tells the Weapon, that the fire button has been released to be able to shoot again ( when not in Auto - Mode )
    /// </summary>
    public void StopShoot()
    {
        shootReset = true;
    }
}

public enum ShootingType
{
    SingleShot,
    Salves,
    Auto,
    Burst
}
