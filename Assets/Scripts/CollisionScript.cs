using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionScript : MonoBehaviour {

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Triggered!!!");
    }
}
