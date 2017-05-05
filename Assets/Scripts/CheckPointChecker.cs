using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointChecker : MonoBehaviour {

    public Vector2 spawnPoint = Vector2.zero;//where the player spawns if this is the active checkpoint
    public Vector2 spawnPointOffset = Vector2.left;//the offset of the spawnPOint from this object's position

	// Use this for initialization
	void Start () {
		if (spawnPoint == Vector2.zero)
        {
            spawnPoint = (Vector2)transform.position + spawnPointOffset;
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.gameObject.tag == "Player")
        {
            coll.gameObject.GetComponent<PlayerController>().setSpawnPoint(spawnPoint);
        }
    }
}
