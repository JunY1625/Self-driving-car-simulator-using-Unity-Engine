using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gamemanager : MonoBehaviour
{
    public Transform spawn_point;
    public GameObject car1; 
    
    public void CreateObjectOnClick()
    {
        Quaternion spawnRotation = Quaternion.identity; 

        GameObject newObject = Instantiate(car1, spawn_point.position, spawnRotation);
        //newObject.GetComponent<Driver_AI>().start_car();

    }
}
