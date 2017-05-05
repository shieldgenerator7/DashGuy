using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Settings")]
    [Range(1,10)]
    public float walkSpeed = 1.0f;
    [Range(1,10)]
    public int dashFrames = 2;//how many frames it takes to complete the dash
    public bool useStreak = false;
    [Header("Objects")]
    public GameObject teleportStreak;
    [Header("Sounds")]
    public AudioClip teleportSound;

    //Processing Variables
    private bool grounded = true;//set in isGrounded()
    private Vector3 gravityVector = new Vector3(0, 0);//the direction of gravity pull (for calculating grounded state)
    private Vector3 sideVector = new Vector3(0, 0);//the direction perpendicular to the gravity direction (for calculating grounded state)
    private float halfWidth = 0;//half of Merky's sprite width
    private int removeVelocityFrames = 0;
    
    //Components
    private CameraController mainCamCtr;//the camera controller for the main camera
    private Rigidbody2D rb2d;
    private BoxCollider2D pc2d;

    // Use this for initialization
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        pc2d = GetComponent<BoxCollider2D>();
        mainCamCtr = Camera.main.GetComponent<CameraController>();
        halfWidth = GetComponent<SpriteRenderer>().bounds.extents.magnitude;
    }

    void FixedUpdate()
    {
        if (removeVelocityFrames >= 0)
        {
            removeVelocityFrames--;
            if (removeVelocityFrames < 0)
            {
                rb2d.velocity = Vector2.zero;
            }
        }
        if (grounded && !rb2d.isKinematic && !isMoving())
        {
            mainCamCtr.discardMovementDelay();
        }
    }

    /// <summary>
    /// Whether or not Merky is moving
    /// Does not consider rotation
    /// </summary>
    /// <returns></returns>
    bool isMoving()
    {
        return rb2d.velocity.magnitude >= 0.1f;
    }

    private bool teleport(Vector3 targetPos)//targetPos is in world coordinations (NOT UI coordinates)
    {
        return teleport(targetPos, true);
    }
    private bool teleport(Vector3 targetPos, bool playSound)//targetPos is in world coordinations (NOT UI coordinates)
    {
        //Get new position
        Vector3 newPos = targetPos;

        //Actually Teleport
        Vector3 oldPos = transform.position;
        Vector3 direction = newPos - oldPos;
        float distance = Vector3.Distance(oldPos, newPos);
        RaycastHit2D[] rch2ds = new RaycastHit2D[1];
        pc2d.Cast(direction, rch2ds, distance, true);
        if (rch2ds[0] && rch2ds[0].collider.gameObject.GetComponent<Rigidbody2D>() == null)
        {
            distance = rch2ds[0].distance;
            newPos = oldPos + direction.normalized * distance;
        }
        float dashSpeed = distance / (Time.deltaTime*dashFrames);
        rb2d.velocity = direction.normalized * dashSpeed;
        removeVelocityFrames = dashFrames;
        showTeleportEffect(oldPos, newPos);
        if (playSound)
        {
            AudioSource.PlayClipAtPoint(teleportSound, oldPos);
        }
        //Momentum Dampening
        if (rb2d.velocity.magnitude > 0.001f)//if Merky is moving
        {
            float newX = rb2d.velocity.x;//the new x velocity
            float newY = rb2d.velocity.y;
            if (Mathf.Sign(rb2d.velocity.x) != Mathf.Sign(direction.x))
            {
                newX = rb2d.velocity.x + direction.x;
                if (Mathf.Sign(rb2d.velocity.x) != Mathf.Sign(newX))
                {//keep from exploiting boost in opposite direction
                    newX = 0;
                }
            }
            if (Mathf.Sign(rb2d.velocity.y) != Mathf.Sign(direction.y))
            {
                newY = rb2d.velocity.y + direction.y;
                if (Mathf.Sign(rb2d.velocity.y) != Mathf.Sign(newY))
                {//keep from exploiting boost in opposite direction
                    newY = 0;
                }
            }
            rb2d.velocity = new Vector2(newX, newY);
        }
        //Gravity Immunity
        grounded = false;
        mainCamCtr.delayMovement(0.3f);
        return true;
    }

    void showTeleportEffect(Vector3 oldp, Vector3 newp)
    {
        if (useStreak)
        {
            showStreak(oldp, newp);
        }
    }
    void showStreak(Vector3 oldp, Vector3 newp)
    {
        GameObject newTS = (GameObject)Instantiate(teleportStreak);
        newTS.GetComponent<TeleportStreakUpdater>().start = oldp;
        newTS.GetComponent<TeleportStreakUpdater>().end = newp;
        newTS.GetComponent<TeleportStreakUpdater>().position();
        newTS.GetComponent<TeleportStreakUpdater>().turnOn(true);
    }

    public void setGravityVector(Vector2 gravity)
    {
        if (gravity.x != gravityVector.x || gravity.y != gravityVector.y)
        {
            gravityVector = gravity;
            //v = P2 - P1    //2016-01-10: copied from an answer by cjdev: http://answers.unity3d.com/questions/564166/how-to-find-perpendicular-line-in-2d.html
            //P3 = (-v.y, v.x) / Sqrt(v.x ^ 2 + v.y ^ 2) * h
            sideVector = new Vector3(-gravityVector.y, gravityVector.x) / Mathf.Sqrt(gravityVector.x * gravityVector.x + gravityVector.y * gravityVector.y);
        }
    }

    bool isGrounded()
    {
        bool isgrounded = isGrounded(gravityVector);
        grounded = isgrounded;
        return isgrounded;
    }
    bool isGrounded(Vector3 direction)
    {
        float length = 0.25f;
        RaycastHit2D[] rh2ds = new RaycastHit2D[10];
        pc2d.Cast(direction, rh2ds, length, true);
        foreach (RaycastHit2D rch2d in rh2ds)
        {
            if (rch2d && rch2d.collider != null && !rch2d.collider.isTrigger)
            {
                GameObject ground = rch2d.collider.gameObject;
                if (ground != null && !ground.Equals(transform.gameObject))
                {
                    return true;
                }
            }
        }
        return false;
    }

    /**
    * Determines whether the given position is occupied or not
    */
    bool isOccupied(Vector3 pos)
    {
        //Debug.DrawLine(pos, pos + new Vector3(0,0.25f), Color.green, 5);
        Vector3 savedOffset = pc2d.offset;
        Vector3 offset = pos - transform.position;
        float angle = transform.localEulerAngles.z;
        Vector3 rOffset = Quaternion.AngleAxis(-angle, Vector3.forward) * offset;//2017-02-14: copied from an answer by robertbu: http://answers.unity3d.com/questions/620828/how-do-i-rotate-a-vector2d.html
        pc2d.offset = rOffset;
        RaycastHit2D[] rh2ds = new RaycastHit2D[10];
        pc2d.Cast(Vector2.zero, rh2ds, 0, true);
        //Debug.DrawLine(pc2d.offset+(Vector2)transform.position, pc2d.bounds.center, Color.grey, 10);
        pc2d.offset = savedOffset;
        foreach (RaycastHit2D rh2d in rh2ds)
        {
            if (rh2d.collider == null)
            {
                break;//reached the end of the valid RaycastHit2Ds
            }
            GameObject go = rh2d.collider.gameObject;
            if (!rh2d.collider.isTrigger)
            {
                if (!go.Equals(transform.gameObject))
                {
                    //Debug.Log("Occupying object: " + go.name);
                    return true;
                }

            }
        }
        return false;//nope, it's not occupied
    }

    /// <summary>
    /// Adjusts the given Vector3 to avoid collision with the objects that it collides with
    /// </summary>
    /// <param name="pos">The Vector3 to adjust</param>
    /// <returns>The Vector3, adjusted to avoid collision with objects it collides with</returns>
    public Vector3 adjustForOccupant(Vector3 pos)
    {
        Vector3 moveDir = new Vector3(0, 0, 0);//the direction to move the pos so that it is valid
        Vector3 savedOffset = pc2d.offset;
        Vector3 offset = pos - transform.position;
        float angle = transform.localEulerAngles.z;
        Vector3 rOffset = Quaternion.AngleAxis(-angle, Vector3.forward) * offset;//2017-02-14: copied from an answer by robertbu: http://answers.unity3d.com/questions/620828/how-do-i-rotate-a-vector2d.html
        pc2d.offset = rOffset;
        RaycastHit2D[] rh2ds = new RaycastHit2D[10];
        pc2d.Cast(Vector2.zero, rh2ds, 0, true);
        pc2d.offset = savedOffset;
        foreach (RaycastHit2D rh2d in rh2ds)
        {
            if (rh2d.collider == null)
            {
                break;//reached the end of the valid RaycastHit2Ds
            }
            GameObject go = rh2d.collider.gameObject;
            if (!rh2d.collider.isTrigger)
            {
                if (!go.Equals(transform.gameObject))
                {
                    Vector3 closPos = rh2d.point;
                    Vector3 dir = pos - closPos;
                    Vector3 size = pc2d.bounds.size;
                    float d2 = (size.magnitude / 2) - Vector3.Distance(pos, closPos);
                    moveDir += dir.normalized * d2;
                }

            }
            //if (go.tag.Equals("HidableArea") || (go.transform.parent != null && go.transform.parent.gameObject.tag.Equals("HideableArea")))
            //{
            //    return true;//yep, it's occupied by a hidden area
            //}
        }
        return pos + moveDir;//not adjusted because there's nothing to adjust for
    }
    
    /// <summary>
    /// Returns true if the given Vector3 is on Merky's sprite
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    public bool gestureOnPlayer(Vector3 pos)
    {
        return Vector3.Distance(pos, transform.position) < halfWidth;
    }

    public void processTapGesture(Vector3 gpos)
    {
        Vector3 prevPos = transform.position;
        Vector3 newPos = gpos;
        teleport(newPos);
        mainCamCtr.checkForAutoMovement(gpos, prevPos);
    }
    public void processTapGesture(GameObject checkPoint)
    {
        throw new System.NotImplementedException("This method needs implemented.");
    }


    public void processHoldGesture(Vector3 gpos, float holdTime, bool finished)
    {
        Debug.DrawLine(transform.position, transform.position + new Vector3(0, halfWidth, 0), Color.blue, 10);
        //Run towards the position
        if (!finished)
        {
            rb2d.velocity = (gpos - transform.position).normalized * walkSpeed;
        }
        else
        {
            rb2d.velocity = new Vector2(0, rb2d.velocity.y);
        }
    }
    public void dropHoldGesture()
    {
    }
}

   
