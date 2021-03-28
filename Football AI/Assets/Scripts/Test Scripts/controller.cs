using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class controller : MonoBehaviour
{
    public float mag = 100;
    Rigidbody rb;

    CapsuleCollider bodyColl;
    Vector3 parentPos;
    Vector3 target = new Vector3(0, 0.5f, 0);
    Vector3 targetVel = new Vector3(1, 0, 1);
    Rigidbody bodyRb;
    Transform bodyTrans;
    public GameObject body;
    // Start is called before the first frame update
    void Start()
    {
        rb = body.GetComponent<Rigidbody>();
        bodyColl = body.GetComponent<CapsuleCollider>();
        parentPos = gameObject.transform.parent.transform.position;
        bodyRb = body.GetComponent<Rigidbody>();
        bodyTrans = body.transform;

        float yrot = Vector2.Angle(Vector2.right, new Vector2(targetVel.x, targetVel.z));
        gameObject.transform.parent.transform.GetChild(2).transform.Rotate(yrot, 0, 0);
    }

    void calcScore()
    {
        if (bodyColl.bounds.Contains(parentPos + target) == true)
        {
            //float timeScore = simTime - fulltimer;
            float velocityAlignScore = Vector3.Dot(targetVel, bodyRb.velocity) / (Vector3.Magnitude(targetVel) * Vector3.Magnitude(bodyRb.velocity));
            float dirAlignScore = -Vector3.Dot(targetVel, target - bodyTrans.localPosition) / (Vector3.Magnitude(targetVel) * Vector3.Magnitude(target - bodyTrans.localPosition));
            float velMagScore = Vector3.Magnitude(bodyRb.velocity);

            Debug.Log(dirAlignScore.ToString() + " " + velocityAlignScore.ToString());
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        calcScore();
        //Debug.Log(rb.velocity);
        if(Input.GetKey(KeyCode.W))
        {
            Vector3 force = new Vector3(mag, 0, 0);
            rb.AddRelativeForce(force);
        }
        if (Input.GetKey(KeyCode.A))
        {
            Vector3 force = new Vector3(0, 0, mag);
            rb.AddRelativeForce(force);
        }
        if (Input.GetKey(KeyCode.S))
        {
            Vector3 force = new Vector3(-mag, 0, 0);
            rb.AddRelativeForce(force);
        }
        if (Input.GetKey(KeyCode.D))
        {
            Vector3 force = new Vector3(0, 0, -mag);
            rb.AddRelativeForce(force);
        }
    }
}
