using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaosObjectDrop : ChaosObject {

    public Vector2 direction = Vector2.down;

    public override void activateEffect()
    {
        Rigidbody2D rb2d = GetComponent<Rigidbody2D>();
        rb2d.isKinematic = false;
        rb2d.velocity = direction;
    }
}
