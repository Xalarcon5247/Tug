using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background : MonoBehaviour
{
    private float maxX = 19.2f;
    public float speed = 0.075f;
    public float currentXpos = 0f;


    // Update is called once per frame
    void Update()
    {
        currentXpos -= speed;
        this.gameObject.transform.position = new Vector3(currentXpos, 0, 0);
        if (currentXpos < -maxX){
            this.gameObject.transform.position = new Vector3(0,0,0);
            currentXpos = 0;
        }
    }
}
