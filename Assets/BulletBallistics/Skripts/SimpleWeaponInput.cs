using System.Collections;
using RootMotion.FinalIK;
using UnityEngine;

namespace Ballistics
{

    //very simple Example, how to make your Weapon shoot
    //for a more complex integration take a look at the BasicWeaponController script
    public class SimpleWeaponInput : MonoBehaviour
    {

        //reference to your weapon
        public Weapon myWeapon;
        public float fireInterval = 1f;
        public float recoilMagnitude = 1f;
        public Recoil recoil;

        private float previousTimer = 0f;
        private float timer;

        void Awake()
        {
            //myWeapon not assigned
            if (myWeapon == null)
            {
                //check for weapon attached to this object
                myWeapon = GetComponent<Weapon>();
            }
        }

        void Update()
        {
            timer += Time.deltaTime;
            if (Input.GetMouseButton(0) && timer - previousTimer > fireInterval)
            {
                previousTimer = timer;
                //call the ShootBullet methode with the bulletdirection when the "Fire1" button is pressed
                myWeapon.ShootBullet(myWeapon.PhysicalBulletSpawnPoint.forward);
                recoil.Fire(recoilMagnitude);
            }
        }
    }
}
