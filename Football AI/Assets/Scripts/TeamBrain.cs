using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamBrain : MonoBehaviour
{
    float timer;

    Vector2 posRange = new Vector2(7.5f, 10);

    PlayerBrain[] playerBrains;
    Transform[] playerTrans;
    Transform[] targetTrans;
    [System.NonSerialized]
    public Transform ball;
    Rigidbody ballRB;

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

    public int side = 1;

    private void Start()
    {
        timer = 0;

        playerBrains = new PlayerBrain[3];
        playerTrans = new Transform[3];
        targetTrans = new Transform[3];
        ballRB = ball.gameObject.GetComponent<Rigidbody>();

        for (int i=0; i<3; i++)
        {
            playerBrains[i] = gameObject.transform.parent.transform.GetChild(i + 1).GetChild(0).GetComponent<PlayerBrain>();
            playerTrans[i] = gameObject.transform.parent.transform.GetChild(i + 1).GetChild(1).GetComponent<Transform>();
            targetTrans[i] = gameObject.transform.parent.transform.GetChild(i + 1).GetChild(2).GetComponent<Transform>();
        }

        move();
    }

    Vector3 scalePos(float x, float z)
    {
        //x = 2 * x - 1;
        //z = 2 * z - 1;
        
        return new Vector3(side*x*posRange.x, 0.5f, side*z*posRange.y);
    }

    Vector3 scaleVel(float x, float z)
    {
        //x = 2 * x - 1;
        //z = 2 * z - 1;

        return new Vector3(side*8 * x, 0, side*8 * z);
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
        float[] output = new float[input.Length + 1];
        output[0] = 1;

        for (int i = 1; i <= input.Length; i++)
        {
            output[i] = logisticAct(input[i - 1]);
        }

        return output;
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
            currentVector = logisticAct(currentVector);
        }

        return (matrixMult(weightsNBiases[hiddenLayersNodes.Length], currentVector));
    }

    void move()
    {
        float[] inputVec = new float[inputNodes];

        inputVec[0] = side * ball.localPosition.x / posRange[0];
        inputVec[1] = side * ball.localPosition.z / posRange[1];
        inputVec[2] = side * ballRB.velocity.x / 8;
        inputVec[3] = side * ballRB.velocity.z / 8;

        //string instr = "";
        //instr = inputVec[0].ToString() + " : " + inputVec[1].ToString();

        //for (int i = 0; i < 3; i++)
        //{
        //    inputVec[2 + 2 * i] = side * playerTrans[i].localPosition.x / posRange[0];
        //    inputVec[2 + 2 * i + 1] = side * playerTrans[i].localPosition.z / posRange[1];
        //    //instr += " : " + inputVec[2 + 2 * i].ToString() + " : " + inputVec[2 + 2 * i + 1].ToString();
        //}
        //Debug.Log(instr);

        float[] outputArr = NeuralNetwork(inputVec);

        //string wb = "";
        //for(int i=0; i<=inputNodes; i++)
        //{
        //    for(int j=0; j<outputNodes; j++)
        //    {
        //        wb += weightsNBiases[0][j, i].ToString() +" ";
        //    }
        //    wb += "a\n";
        //}
        //Debug.Log(wb);

        //string ostr = "";
        for (int i = 0; i < 3; i++)
        {
            playerBrains[i].target = scalePos(outputArr[0 + 0 + 4 * i], outputArr[0 + 1 + 4 * i]);
            playerBrains[i].targetVel = scaleVel(outputArr[0 + 2 + 4 * i], outputArr[0 + 3 + 4 * i]);
            //ostr += outputArr[0 + 0 + 4 * i] + " " + outputArr[0 + 1 + 4 * i] + " " + outputArr[0 + 2 + 4 * i] + " " + outputArr[0 + 3 + 4 * i] + " ";

            float yrot = Vector2.Angle(Vector2.right, new Vector2(playerBrains[i].targetVel.x, playerBrains[i].targetVel.z));
            targetTrans[i].localPosition = playerBrains[i].target;
            targetTrans[i].Rotate(yrot, 0, 0);
        }
        //Debug.Log(ostr);
    }

    private void FixedUpdate()
    {
        timer += Time.deltaTime;

        if(timer > 0.5)
        {
            timer = 0;

            move();
        }
    }
}