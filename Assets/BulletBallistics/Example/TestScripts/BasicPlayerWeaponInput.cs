using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicPlayerWeaponInput : MonoBehaviour {

    public float WeaponSpreadWalking;

    public List<WeaponData> Weapons;
    [HideInInspector]
    public int currentWeapon = 0;
    private int weaponBefore = -1;

    

    void Awake()
    {
        for (int i = 0; i < Weapons.Count; i++)
        {
            Weapons[i].weapon.OnShoot += OnShoot;
            ((DefaultMagazineController)Weapons[i].weapon.myMagazineController).OnMagEmptie += OnMagazineEmptie;
        }
    }

    void OnMagazineEmptie()
    {
        Debug.Log("You need to press 'r' to reload!");
    }

    void Update()
    {
        if (currentWeapon != -1)
        {
            HandleWeapon();
        }
        SwitchWeapons();

        ZeroWeapons();
    }

    void ZeroWeapons()
    {
        //setting the zeroing of the current weapon
        if (Weapons[currentWeapon].weapon.TargetWeapon.BarrelZeroingDistances.Count > 0)
        {
            if (Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                Weapons[currentWeapon].weapon.TargetWeapon.currentBarrelZero = Mathf.Clamp(Weapons[currentWeapon].weapon.TargetWeapon.currentBarrelZero + 1, -1, Weapons[currentWeapon].weapon.TargetWeapon.BarrelZeroingCorrections.Count - 1);

            }
            if (Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                Weapons[currentWeapon].weapon.TargetWeapon.currentBarrelZero = Mathf.Clamp(Weapons[currentWeapon].weapon.TargetWeapon.currentBarrelZero - 1, -1, Weapons[currentWeapon].weapon.TargetWeapon.BarrelZeroingCorrections.Count - 1);
            }
        }
        else
        {
            Weapons[currentWeapon].weapon.TargetWeapon.currentBarrelZero = -1;
        }
    }

    /// <summary>
    /// calling shoot/aim/reload on the current weapon
    /// </summary>
    void HandleWeapon()
    {
        //Shoot
        if (Input.GetMouseButton(0))
        {
            Weapons[currentWeapon].weapon.Shoot();
        }
        if (Input.GetMouseButtonUp(0))
        {
            Weapons[currentWeapon].weapon.StopShoot();
        }

        //Aim
        if (Input.GetMouseButtonDown(1))
        {
            Weapons[currentWeapon].weapon.Aim(true);
        }
        if (Input.GetMouseButtonUp(1))
        {
            Weapons[currentWeapon].weapon.Aim(false);
        }

        //Reload
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(Reload(currentWeapon));
        }
    }

    /// <summary>
    /// wait for reload time then update the magazine controller
    /// </summary>
    /// <param name="myCurrentW">current weapon id<param>
    /// <returns></returns>
    IEnumerator Reload(int myCurrentW)
    {
        Debug.Log("Reloading...");
        yield return new WaitForSeconds(Weapons[myCurrentW].ReloadTime);
        if (myCurrentW == currentWeapon)
        {
            ((DefaultMagazineController)Weapons[currentWeapon].weapon.myMagazineController).Reload();
        }
    }

    /// <summary>
    /// switch between weapons in the weapon list
    /// </summary>
    void SwitchWeapons()
    {
        if (Input.mouseScrollDelta.y > 0)
        {
            currentWeapon++;
            if (currentWeapon >= Weapons.Count)
            {
                currentWeapon = 0;
            }
        }

        if (Input.mouseScrollDelta.y < 0)
        {
            currentWeapon--;
            if (currentWeapon < 0)
            {
                currentWeapon = Weapons.Count - 1;
            }
        }

        if (weaponBefore != currentWeapon)
        {
            for (int i = 0; i < Weapons.Count; i++)
            {
                Weapons[i].weapon.gameObject.SetActive(i == currentWeapon); //activate current Weapon
            }
            if (weaponBefore != -1)
            {
                Weapons[weaponBefore].weapon.StopShoot();
                Weapons[weaponBefore].weapon.Aim(false);
            }

        }

        weaponBefore = currentWeapon;
    }

    /// <summary>
    /// refill all MagazineControllers
    /// </summary>
    /// <param name="col"></param>
    void OnTriggerEnter(Collider col)
    {
        if (col.tag == "supply")
        {
            for (int i = 0; i < Weapons.Count; i++)
            {
                DefaultMagazineController defaultCont = ((DefaultMagazineController)Weapons[i].weapon.myMagazineController);
                defaultCont.StoredBullets = defaultCont.MagCount * defaultCont.BulletsPerMag;
            }
        }
    }

    /// <summary>
    /// called when current weapon shoots
    /// </summary>
    void OnShoot()
    {
        //Play Muzzle Particle System
        if (Weapons[currentWeapon].particle != null)
        {
            Weapons[currentWeapon].particle.Simulate(0, true, true);
            ParticleSystem.EmissionModule module = Weapons[currentWeapon].particle.emission;
            module.enabled = true;
            Weapons[currentWeapon].particle.Play(true);
        }
        //Play Shoot Sound
        if (Weapons[currentWeapon].sound != null)
        {
            Weapons[currentWeapon].sound.Play();
        }
    }
}

[System.Serializable]
public struct WeaponData
{
    public BasicWeaponController weapon;
    public AudioSource sound;
    public ParticleSystem particle;
    public Transform ScopePos;
    public float ReloadTime;
    public float RecoilAmount;
}
