using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnEnableObject : MonoBehaviour
{
 
    public List<GameObject> objects;

    private void Awake()
    {
        if(objects == null)
        {
            objects = new List<GameObject>();
            foreach(GameObject g in gameObject.GetComponentsInChildren<GameObject>())
            {
                objects.Add(g);
            }
        }
    }

    
}
