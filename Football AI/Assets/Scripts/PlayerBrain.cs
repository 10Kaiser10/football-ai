using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBrain : MonoBehaviour
{
    public GameObject body;
    private Transform bodyTrans;
    private Rigidbody bodyRb;

    [System.NonSerialized]
    public Vector3 target;
    [System.NonSerialized]
    public Vector3 targetVel;

    float timer;

    public float MaxForce = 200;

    Vector3 Normalize(float x, float y)
    {
        Vector3 vec = new Vector3(x, 0, y);
        vec = Vector3.Normalize(vec);
        return MaxForce * vec;
    }

    void move()
    {
        float[] inputVec = new float[PlayerNeuralNet.inputNodes];

        inputVec[0] = (target.x - bodyTrans.localPosition.x) / 30;
        inputVec[1] = (target.z - bodyTrans.localPosition.z) / 30;

        inputVec[2] = targetVel.x / 10;
        inputVec[3] = targetVel.z / 10;
        inputVec[4] = bodyRb.velocity.x / 10;
        inputVec[5] = bodyRb.velocity.z / 10;

        float[] outputArr = PlayerNeuralNet.NeuralNetwork(inputVec);
        //Vector3 force = new Vector3(putInBetween(-MaxForce, MaxForce, outputArr[0]), 0, putInBetween(-MaxForce, MaxForce, outputArr[1]));
        //bodyRb.AddRelativeForce(force);

        bodyRb.AddRelativeForce(Normalize(outputArr[0], outputArr[1]));
        //appliedForce = Normalize(outputArr[0], outputArr[1]);
        //Debug.Log(Normalize(outputArr[0], outputArr[1]));
    }

    void Start()
    {
        bodyTrans = body.transform;
        bodyRb = body.GetComponent<Rigidbody>();
        timer = 0;
    }

    void FixedUpdate()
    {
        timer += Time.deltaTime;
        if (timer > 0.02)
        {
            move();
            timer = 0;
        }
    }
}
