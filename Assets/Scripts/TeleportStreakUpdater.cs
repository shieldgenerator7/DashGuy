﻿using UnityEngine;
using System.Collections;

public class TeleportStreakUpdater : MonoBehaviour {

    [Header("Settings")]
    [Range(0.1f,10)]
    public float showDuration = 2.0f;//how long to show the streak (seconds)
    public Vector3 start;
    public Vector3 end;

    private float distance;
    private Vector3 newV;

    private float endShowTime = 0;
    private bool turnedOn = false;

	// Use this for initialization
	void Start () {
        turnOn(turnedOn);
    }

    // Update is called once per frame
    void Update()
    {
        if (turnedOn) {
            Color prevColor = GetComponent<SpriteRenderer>().color;
            GetComponent<SpriteRenderer>().color = new Color(prevColor.r, prevColor.g, prevColor.b, prevColor.a - 0.05f);
            if (Time.time > endShowTime)
            {
                Destroy(gameObject);
            }
        }
	}

    public void position()
    {
        //Set the position
        transform.position = new Vector3(start.x, start.y);

        //Set the size
         distance = Vector3.Distance(start, end);
        Vector3 size = GetComponent<SpriteRenderer>().bounds.size;
        float baseWidth = size.x;
        float baseHeight = size.y;
        float newSize = distance;
        newV = new Vector3(newSize / baseWidth, 1.5f * baseHeight / baseHeight);
        transform.localScale = newV;

        //Set the angle
        float angle = AngleSigned(end-start, Vector3.left, Vector3.back);
        transform.rotation = Quaternion.Euler(0,0,angle);
    }

    public void turnOn(bool tO)
    {
        turnedOn = tO;
        gameObject.SetActive(tO);
        if (turnedOn)
        {
            endShowTime = Time.time + showDuration;
        }
    }

    //2015-12-21: copied from http://forum.unity3d.com/threads/need-vector3-angle-to-return-a-negtive-or-relative-value.51092/#post-324018
    //originally posted by Tinus
    public static float AngleSigned(Vector3 v1, Vector3 v2, Vector3 n)
    {
        return Mathf.Atan2(
            Vector3.Dot(n, Vector3.Cross(v1, v2)),
            Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;
    }
}
