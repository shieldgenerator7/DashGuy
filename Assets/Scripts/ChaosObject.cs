using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaosObject : MonoBehaviour {

    [Header("Activation Options")]
    public bool contactWithPlayer = false;
    public bool triggerArea = false;

    void OnCollisionEnter2D(Collision2D coll)
    {
        if (contactWithPlayer && coll.gameObject.tag == "Player")
        {
            activateEffect();
        }
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (triggerArea && coll.gameObject.tag == "Player")
        {
            activateEffect();
        }
    }

    public virtual void activateEffect()
    {
        throw new System.NotImplementedException("This subtype of ChaosObject has not implemented activateEffect.");
    }
}
