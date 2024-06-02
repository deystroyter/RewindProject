using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeRandomPosition : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            transform.position = new Vector3(Random.Range(0, 5), Random.Range(0, 5), Random.Range(0, 5));
        }
    }
}
