 using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Ballistics
{
    public class PoolManager : MonoBehaviour
    {
        /// <summary>
        /// all objects in the pool
        /// </summary>
        public Dictionary<int, Queue<GameObject>> Pool = new Dictionary<int, Queue<GameObject>>();

        /// <summary>
        /// current instance
        /// </summary>
        static PoolManager _instance;

        public static PoolManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<PoolManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("Pool");
                        _instance = go.AddComponent<PoolManager>();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// add a object to the pool
        /// </summary>
        /// <param name="ID">the GameObject the object is the instance of</param>
        /// <param name="obj">the object that is added to the pool</param>
        public void AddObject(GameObject ID, GameObject obj)
        {
            int id = ID.GetInstanceID();
            if (Pool.ContainsKey(id))
            {
                Pool[id].Enqueue(obj);
            }
            else
            {
                Pool.Add(id, new Queue<GameObject>());
                Pool[id].Enqueue(obj);
            }
        }

        /// <summary>
        /// get the next object in the queue ( if available )
        /// </summary>
        /// <param name="ID">the base GameObject the object was instantiated from</param>
        /// <returns></returns>
        public GameObject GetNextGameObject(GameObject ID)
        {
            int id = ID.GetInstanceID();

            if (Pool.ContainsKey(id))
            {
                if (Pool[id].Count > 0)
                {
                    GameObject c = Pool[id].Dequeue();
                    PoolingObject pObj = c.GetComponent<PoolingObject>();
                    if (pObj != null)
                    {
                        pObj.ReAwake();
                    }
                    return c;
                }
            }
            return null;
        }
    }
}
