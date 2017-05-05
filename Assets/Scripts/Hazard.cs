using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hazard : MonoBehaviour {

    public bool killOnContact = true;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnCollisionEnter2D(Collision2D coll)
    {
        if (killOnContact && coll.gameObject.tag == "Player")
        {
            coll.gameObject.GetComponent<PlayerController>().kill();
        }
    }
}
