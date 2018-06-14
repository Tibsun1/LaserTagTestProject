using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour {

    public Text HP, Ammunition;
    public float health;

	// Use this for initialization
	void Start () {
        HP.text = health.ToString();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
