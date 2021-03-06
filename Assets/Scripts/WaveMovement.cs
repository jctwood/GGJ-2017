﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveMovement : MonoBehaviour {

    public float timeOffSet;
    public float heightOffSet;
    public float waveSize = 1;
    public float horizontalSpeed;

    private float orginalHeight;

	// Use this for initialization
	void Start ()
    {
        orginalHeight = transform.position.y;
	}
	
	// Update is called once per frame
	void Update ()
    {
        float newHeight;

        newHeight = Mathf.Sin((Time.time + timeOffSet) * waveSize);

        newHeight += orginalHeight;

        newHeight += heightOffSet;

        Vector3 newPosition = transform.position;

        newPosition.y = newHeight;

        transform.position = newPosition;

	}

    
}
