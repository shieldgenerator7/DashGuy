using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaosObjectRotate : ChaosObject {
    public override void activateEffect()
    {
        transform.Rotate(0, 0, 90);
    }
}
