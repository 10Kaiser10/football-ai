using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brain2 : MonoBehaviour
{
    private float timer;
    private float fulltimer;
    [System.NonSerialized]
    public float simTime;

    //for score calc
    private float score;
    public bool reachedTarget;
    private float minimumDist;

    //target
    [System.NonSerialized]
    public Vector3 target;
    [System.NonSerialized]
    public Vector3 targetVel;

    //body
    public GameObject body;
    private Transform bodyTrans;
    private Rigidbody bodyRb;
    private CapsuleCollider bodyColl;
    public float MaxForce = 10;

    //neural network
    [System.NonSerialized]
    public int inputNodes;
    [System.NonSerialized]
    public int outputNodes;
    [System.NonSerialized]
    public int[] hiddenLayersNodes;
    [System.NonSerialized]
    public int[] LayersNodes;
    [System.NonSerialized]
    public float[][,] weightsNBiases;

    void Start()
    {
        bodyTrans = body.transform;
        bodyRb = body.GetComponent<Rigidbody>();
        bodyColl = body.GetComponent<CapsuleCollider>();
        timer = 0;
        fulltimer = 0;
        reachedTarget = false;
        minimumDist = 1000;
    }

    float putInBetween(float l, float h, float n)
    {
        float diff = h - l;
        return l + diff * n;
    }

    float leakyRelu(float input)
    {
        if (input > 0)
        {
            return input;
        }
        else
        {
            return 0.01f * input;
        }
    }

    float[] leakyRelu(float[] input)
    {
        float[] output = new float[input.Length + 1];
        output[0] = 1;

        for (int i = 1; i <= input.Length; i++)
        {
            output[i] = leakyRelu(input[i - 1]);
        }

        return output;
    }

    float[] matrixMult(float[,] matrix, float[] vector)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        float[] output = new float[rows];

        for (int i = 0; i < rows; i++)
        {
            output[i] = 0;
            for (int j = 0; j < cols; j++)
            {
                output[i] += matrix[i, j] * vector[j];
            }
        }

        return output;
    }

    float logisticAct(float input)
    {
        return 1 / (1 + Mathf.Exp(-input));
    }

    float[] logisticAct(float[] input)
    {
        for (int i = 0; i < input.Length; i++)
        {
            input[i] = logisticAct(input[i]);
        }
        return input;
    }

    float[] NeuralNetwork(float[] input)
    {
        float[] currentVector;
        currentVector = new float[input.Length + 1];
        currentVector[0] = 1;

        for (int i = 1; i <= input.Length; i++)
        {
            currentVector[i] = input[i - 1];
        }

        for (int i = 0; i < hiddenLayersNodes.Length; i++)
        {
            currentVector = matrixMult(weightsNBiases[i], currentVector);
            currentVector = leakyRelu(currentVector);
        }

        return logisticAct(matrixMult(weightsNBiases[hiddenLayersNodes.Length], currentVector));
    }

    void move()
    {
        float[] inputVec = new float[inputNodes];

        inputVec[0] = target.x;
        inputVec[1] = target.z;
        inputVec[2] = targetVel.x;
        inputVec[3] = targetVel.z;

        inputVec[4] = bodyTrans.localPosition.x;
        inputVec[5] = bodyTrans.localPosition.z;
        inputVec[6] = bodyRb.velocity.x;
        inputVec[7] = bodyRb.velocity.z;

        float[] outputArr = NeuralNetwork(inputVec);

        Vector3 force = new Vector3(putInBetween(-MaxForce, MaxForce, outputArr[0]), 0, putInBetween(-MaxForce, MaxForce, outputArr[1]));

        bodyRb.AddRelativeForce(force);
    }

    void calcScore()
    {
        if (reachedTarget == false && bodyColl.bounds.Contains(target) == true)
        {
            float timeScore = simTime - fulltimer;
            float velocityAlignScore = Vector3.Dot(targetVel, bodyRb.velocity) / (Vector3.Magnitude(targetVel) * Vector3.Magnitude(bodyRb.velocity));
            float dirAlignScore = Vector3.Dot(targetVel, target - bodyTrans.position) / (Vector3.Magnitude(targetVel) * Vector3.Magnitude(target - bodyTrans.position));
            float velocityMagScore = 1 / (1 + Mathf.Pow((Vector3.Magnitude(targetVel) - Vector3.Magnitude(bodyRb.velocity)), 2));

            reachedTarget = true;
            score = 2 + timeScore + velocityAlignScore + dirAlignScore + velocityMagScore;
        }
        else if(reachedTarget == false)
        {
            float thisDist = Vector3.Distance(target, bodyTrans.position);
            minimumDist = Mathf.Min(thisDist, minimumDist);
        }
    }

    public float getScore()
    {
        if(reachedTarget == true)
        {
            return score;
        }
        else
        {
            float distanceScore = - minimumDist;
            float speedScore = 15 / (1 + Mathf.Pow((Vector3.Magnitude(targetVel) - Vector3.Magnitude(bodyRb.velocity)), 2));
            return -15 + distanceScore + speedScore;
        }
    }

    void FixedUpdate()
    {
        fulltimer += Time.deltaTime;
        timer += Time.deltaTime;
        if (timer > 0.02)
        {
            move();
            calcScore();
            timer = 0;
        }
    }
}
