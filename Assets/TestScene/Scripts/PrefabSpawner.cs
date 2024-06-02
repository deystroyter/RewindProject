using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PrefabSpawner : MonoBehaviour
{
    [SerializeField] private int howMuch;
    [SerializeField] private GameObject prefab;

    private void Start()
    {
        var halfsqrt = Mathf.RoundToInt(Mathf.Sqrt(howMuch) / 2);
        var counter = 0;
        for (int x = halfsqrt; x >= -halfsqrt; x--)
        {
            for (int z = halfsqrt; z >= -halfsqrt; z--)
            {
                if (counter > howMuch)
                {
                    Debug.Log($"{counter} prefabs spawned!");
                    return;
                }
                var go = GameObject.Instantiate(prefab);
                go.transform.parent = this.transform;
                go.transform.position = new Vector3(x*2, 4f, z*2) ;
                counter++;

            }
        }
        Debug.Log($"{counter} cubes spawned!");
       
    }
    private void FixedUpdate()
    {
        
    }

}
