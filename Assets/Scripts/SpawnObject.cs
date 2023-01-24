using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnObject : MonoBehaviour
{
    public GameObject ObjectToSpawn;
    private GameObject obj;
    public bool OnlyOneAtATime = false;
 
    public void spawn()
    {
        if(ObjectToSpawn != null)
        {
            if(OnlyOneAtATime && obj == null)
            {
                obj = Instantiate(ObjectToSpawn);
            }
            else if (!OnlyOneAtATime)
            {
                Instantiate(ObjectToSpawn);
            }
        }
    }
}
