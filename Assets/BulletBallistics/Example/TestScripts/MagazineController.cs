using UnityEngine;
using System.Collections;
using System;

public abstract class MagazineController : MonoBehaviour {


    /// <summary>
    /// returns if a bullet is available to shoot
    /// </summary>
    /// <returns></returns>
	public abstract bool isBulletAvailable();

}
