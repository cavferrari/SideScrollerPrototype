using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Ballistics
{
    public class Weapon : MonoBehaviour
    {

        //PUBLIC _______________________________________________________________

        //General

        /// <summary>
        /// spawnpoint of the bullet visualisation
        /// </summary>
        public Transform VisualSpawnPoint;
        /// <summary>
        /// spawnpoint of the real / physical bullet (usually center of the screen)
        /// </summary>
        public Transform PhysicalBulletSpawnPoint;

        /// <summary>
        /// Lifetime of each bullet
        /// </summary>
        public float LifeTimeOfBullets = 6;
        /// <summary>
        /// Layers that get detected by Raycast
        /// </summary>
        public LayerMask HitMask = new LayerMask();
        /// <summary>
        /// damage the bullet deals at initialisation
        /// </summary>
        public float MuzzleDamage = 80;

        //Bullet
        /// <summary>
        /// initial speed of the bullet
        /// </summary>
        public float MaxBulletSpeed = 550;
        /// <summary>
        /// randomisation of bullet speed
        /// </summary>
        public float randomSpeedOffset = 10;
        /// <summary>
        /// mass of each bullet
        /// </summary>
        public float BulletMass = 0.0065f;
        /// <summary>
        /// bullet diameter
        /// </summary>
        public float Diameter = 0.01f;
        /// <summary>
        /// the drag coefficient ( sphere .5f )
        /// </summary>
        public float DragCoefficient = 0.4f;
        /// <summary>
        /// Prefab of the bullet visuals
        /// </summary>
        public Transform BulletPref;
        //--

        //Zeroing
        /// <summary>
        /// list of Zeroing distances
        /// </summary>
        public List<float> BarrelZeroingDistances = new List<float>();
        /// <summary>
        /// list of angle corrections to zero at distances
        /// </summary>
        public List<float> BarrelZeroingCorrections = new List<float>();
        /// <summary>
        /// current zeroing id
        /// </summary>
        public int currentBarrelZero = -1; //-1 equals no Correction
        public int BarrelZeroCount { get { return BarrelZeroingCorrections.Count; } }
        //--

        //-----------------------------------------------------------------------

        //PRIVATE________________________________________________________________
        BallisticsSettings Settings;

        private BulletHandler bulletHandler;

        //Pool
        private PoolManager myPool;

        //store precalculated drag to save performance
        public float PreDrag;
        private float area;

        //-----------------------------------------------------------------------

        void Awake()
        {

            myPool = PoolManager.instance;

            bulletHandler = BulletHandler.instance;
            if (bulletHandler == null) return;

            if (bulletHandler.Settings != null)
            {
                Settings = bulletHandler.Settings;
            }

            RecalculatePrecalculatedValues();
        }

        //PUBLIC FUNCTIONS____________________________________________________________________________________________

        /// <summary>
        /// calculates mostly unchanged Values
        /// </summary>
        public void RecalculatePrecalculatedValues()
        {
            area = Mathf.Pow(Diameter / 2, 2) * Mathf.PI;
            if (Settings != null)
            {
                PreDrag = (0.5f * Settings.AirDensity * area * DragCoefficient) / BulletMass;

                if (BarrelZeroingDistances.Count > 0)
                {
                    BarrelZeroingDistances.Sort();
                    CalculateBarrelZeroCorrections();
                }
            }
        }

        //public functions ____________________________________________________________________________________

        /// <summary>
        /// precalculates zeroingvalues
        /// </summary>
        public void CalculateBarrelZeroCorrections()
        {
            BarrelZeroingCorrections.Clear();
            for (int i = 0; i < BarrelZeroingDistances.Count; i++)
            {
                float FlightTime;
                float drop;

                if (Settings.useBulletdrag)
                {
                    float k = (Settings.AirDensity * DragCoefficient * Mathf.PI * (Diameter * .5f) * (Diameter * .5f)) / (2 * BulletMass);
                    FlightTime = (Mathf.Exp(k * BarrelZeroingDistances[i]) - 1) / (k * MaxBulletSpeed);
                }
                else
                {
                    FlightTime = BarrelZeroingDistances[i] / MaxBulletSpeed;
                }

                drop = (.5f * -Physics.gravity.y * Mathf.Pow(FlightTime, 2));

                //scope height above barrel
                drop -= VisualSpawnPoint.localPosition.y;

                BarrelZeroingCorrections.Add(360f - Mathf.Atan(drop / BarrelZeroingDistances[i]) * Mathf.Rad2Deg);
            }
        }

        //-----------------------------------------------------------------------------------------------------------


        /// <summary>
        /// Instantiates the Bullet and gives them over to BulletHandler for Calculation
        /// </summary>
        /// <param name="ShootDirection">the direction the bullet is fired in ( usually you want to use 'PhysicalBulletSpawnPoint.forward' for this direction )</param>
        public void ShootBullet(Vector3 ShootDirection)
        {
            Transform bClone = null;
            if (BulletPref != null)
            {
                GameObject cGO = myPool.GetNextGameObject(BulletPref.gameObject);
                if (cGO == null)
                {
                    bClone = (Transform)Instantiate(BulletPref, VisualSpawnPoint.position, Quaternion.identity);
                    bClone.SetParent(myPool.transform);
                }
                else
                {
                    cGO.SetActive(true);
                    bClone = cGO.transform;
                    bClone.position = VisualSpawnPoint.position;
                }
            }
            //calculte in zeroing corrections
            Vector3 dir = (currentBarrelZero != -1 ? Quaternion.AngleAxis(BarrelZeroingCorrections[currentBarrelZero], PhysicalBulletSpawnPoint.right) * ShootDirection : ShootDirection);

            //give the BulletInstace over to the BulletHandler
            bulletHandler.Bullets.Enqueue(new BulletData(this, PhysicalBulletSpawnPoint.position, VisualSpawnPoint.position - PhysicalBulletSpawnPoint.position, dir, LifeTimeOfBullets, randomSpeedOffset == 0 ? MaxBulletSpeed : MaxBulletSpeed + UnityEngine.Random.Range(0f, randomSpeedOffset) - randomSpeedOffset / 2f, bClone));
        }

        /// <summary>
        /// sets currentBarrelZero to zeroID
        /// </summary>
        /// <param name="zeroID">distance id from the BarrelZeroingDistances list</param>
        /// <returns>found zeroing id</returns>
        public bool setZeroingTo(int zeroID)
        {
            if (zeroID < BarrelZeroCount)
            {
                currentBarrelZero = zeroID;
                return true;
            }
            return false;
        }
        /// <summary>
        /// sets currentBarrelZero to the id that equals the distance
        /// </summary>
        /// <param name="distance">distance to zero the weapon to</param>
        /// <returns>found distance in BarrelZeroingDistances list</returns>
        public bool setZeroingTo(float distance)
        {
            if (BarrelZeroingDistances.Contains(distance))
            {
                currentBarrelZero = BarrelZeroingDistances.IndexOf(distance);
                return true;
            }
            return false;
        }
    }
}