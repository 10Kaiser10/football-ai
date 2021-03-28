using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PSOEvolver : MonoBehaviour
{
    //timer
    private float timer;
    public float simDuration = 10;
    public int generationsPerCycle = 8;
    private int generationNum = 1;

    //evolution
    //public float groundTouchWieght = 1;
    //public float distanceWieght = 1;
    public float mutationNum = 10;

    //population
    public int populationSize = 16;
    private GameObject[] population;
    private float[] scores;

    //spawning
    public int spawnRows = 20;
    public GameObject prefab;
    public GameObject parent;
    public float ydefault = 0.5f;
    public float spawnRadius = 15;
    public float targetSpeedRange = 10;

    //neural network
    private int inputNodes = 6;
    private int outputNodes = 2;
    private int[] hiddenLayersNodes = { };
    private int[] LayersNodes = { 6, 2 };
    private float[][][,] weightsNBiases;
    private float[][][,] nextWeightsNBiases;
    private Vector2 initiantionRange = new Vector2(-5000f, 5000f);

    //maximums arrays
    float globalMax = -100000;
    private float[][,] globalMaxWnB;
    float[] individualMax;
    private float[][][,] individualMaxWnB;

    //pso
    float[][][,] velocity;
    float w = 0.729f, c1 = 2.05f, c2 = 2.05f;

    public void Begin()
    {
        scores = new float[populationSize];
        weightsNBiases = new float[populationSize][][,];

        for (int i = 0; i < populationSize; i++)
        {
            weightsNBiases[i] = new float[hiddenLayersNodes.Length + 1][,];

            for (int l = 0; l <= hiddenLayersNodes.Length; l++)
            {
                int rows = LayersNodes[l + 1];
                int cols = LayersNodes[l] + 1;
                weightsNBiases[i][l] = new float[rows, cols];

                for (int j = 0; j < rows; j++)
                {
                    for (int k = 0; k < cols; k++)
                    {
                        weightsNBiases[i][l][j, k] = Random.Range(initiantionRange.x, initiantionRange.y);
                    }
                }
            }
        }

        population = new GameObject[populationSize];

        for (int i = 0; i < populationSize; i++)
        {
            Vector3 spawnPos = new Vector3(10 * (i / spawnRows) + Random.Range(-spawnRadius, spawnRadius), 0, 10 * (i % spawnRows) + Random.Range(-spawnRadius, spawnRadius));
            Vector3 spawnVel = new Vector3(Random.Range(-targetSpeedRange, targetSpeedRange), 0, Random.Range(-targetSpeedRange, targetSpeedRange));
            population[i] = Instantiate(prefab, spawnPos, Quaternion.identity, parent.transform);
            population[i].transform.GetChild(1).GetComponent<Rigidbody>().velocity = spawnVel;
            Brain2 objBrain = population[i].transform.GetChild(0).GetComponent<Brain2>();
            objBrain.inputNodes = inputNodes;
            objBrain.outputNodes = outputNodes;
            objBrain.hiddenLayersNodes = hiddenLayersNodes;
            objBrain.LayersNodes = LayersNodes;
            objBrain.weightsNBiases = weightsNBiases[i];
            objBrain.target = new Vector3(Random.Range(-spawnRadius, spawnRadius), 0.5f, Random.Range(-spawnRadius, spawnRadius));
            objBrain.targetVel = new Vector3(Random.Range(-targetSpeedRange, targetSpeedRange), 0, Random.Range(-targetSpeedRange, targetSpeedRange));
            objBrain.simTime = simDuration;

            population[i].transform.GetChild(2).transform.localPosition = objBrain.target;
            float yrot = Vector2.Angle(Vector2.right, new Vector2(objBrain.targetVel.x, objBrain.targetVel.z));
            population[i].transform.GetChild(2).transform.Rotate(yrot, 0, 0);
        }

        individualMaxWnB = new float[populationSize][][,];
        for (int i = 0; i < populationSize; i++)
        {
            individualMaxWnB[i] = new float[hiddenLayersNodes.Length + 1][,];

            for (int l = 0; l <= hiddenLayersNodes.Length; l++)
            {
                int rows = LayersNodes[l + 1];
                int cols = LayersNodes[l] + 1;
                individualMaxWnB[i][l] = new float[rows, cols];

                for (int j = 0; j < rows; j++)
                {
                    for (int k = 0; k < cols; k++)
                    {
                        individualMaxWnB[i][l][j,k] = weightsNBiases[i][l][j, k];
                    }
                }
            }
        }

        individualMax = new float[populationSize];
        for (int i = 0; i < populationSize; i++)
        {
            individualMax[i] = -100000;
        }

        globalMaxWnB = new float[hiddenLayersNodes.Length + 1][,];

        for (int l = 0; l <= hiddenLayersNodes.Length; l++)
        {
            int rows = LayersNodes[l + 1];
            int cols = LayersNodes[l] + 1;
            globalMaxWnB[l] = new float[rows, cols];

            for (int j = 0; j < rows; j++)
            {
                for (int k = 0; k < cols; k++)
                {
                    globalMaxWnB[l][j, k] = weightsNBiases[0][l][j, k];
                }
            }
        }

        velocity = new float[populationSize][][,];

        for (int i = 0; i < populationSize; i++)
        {
            velocity[i] = new float[hiddenLayersNodes.Length + 1][,];

            for (int l = 0; l <= hiddenLayersNodes.Length; l++)
            {
                int rows = LayersNodes[l + 1];
                int cols = LayersNodes[l] + 1;
                velocity[i][l] = new float[rows, cols];

                for (int j = 0; j < rows; j++)
                {
                    for (int k = 0; k < cols; k++)
                    {
                        velocity[i][l][j, k] = Random.Range(0.1f*initiantionRange.x, 0.1f * initiantionRange.y);
                    }
                }
            }
        }
    }

    private void respawn()
    {
        foreach (GameObject obj in population)
        {
            Destroy(obj);
        }

        population = new GameObject[populationSize];

        for (int i = 0; i < populationSize; i++)
        {
            Vector3 spawnPos = new Vector3(10 * (i / spawnRows) + Random.Range(-spawnRadius, spawnRadius), 0, 10 * (i % spawnRows) + Random.Range(-spawnRadius, spawnRadius));
            Vector3 spawnVel = new Vector3(Random.Range(-targetSpeedRange, targetSpeedRange), 0, Random.Range(-targetSpeedRange, targetSpeedRange));
            population[i] = Instantiate(prefab, spawnPos, Quaternion.identity, parent.transform);
            population[i].transform.GetChild(1).GetComponent<Rigidbody>().velocity = spawnVel;
            Brain2 objBrain = population[i].transform.GetChild(0).GetComponent<Brain2>();
            objBrain.inputNodes = inputNodes;
            objBrain.outputNodes = outputNodes;
            objBrain.hiddenLayersNodes = hiddenLayersNodes;
            objBrain.LayersNodes = LayersNodes;
            objBrain.weightsNBiases = weightsNBiases[i];
            objBrain.target = new Vector3(Random.Range(-spawnRadius, spawnRadius), 0.5f,Random.Range(-spawnRadius, spawnRadius));
            objBrain.targetVel = new Vector3(Random.Range(-targetSpeedRange, targetSpeedRange), 0, Random.Range(-targetSpeedRange, targetSpeedRange));
            objBrain.simTime = simDuration;

            population[i].transform.GetChild(2).transform.localPosition = objBrain.target;
            float yrot = Vector2.Angle(Vector2.right, new Vector2(objBrain.targetVel.x, objBrain.targetVel.z));
            population[i].transform.GetChild(2).transform.Rotate(yrot, 0, 0);
        }
    }

    private void Start()
    {
        timer = 0;
        Begin();
    }

    private void calcScores()
    {
        for (int i = 0; i < populationSize; i++)
        {
            scores[i] = population[i].transform.GetChild(0).GetComponent<Brain2>().getScore();
        }


        int avgFit = 0, sqsum = 0, numCrossed = 0;
        foreach (int item in scores) { avgFit += item; }
        avgFit /= populationSize;
        foreach (int item in scores) { sqsum += (item - avgFit) * (item - avgFit); }
        sqsum /= populationSize;
        foreach (GameObject item in population) { numCrossed += (item.transform.GetChild(0).GetComponent<Brain2>().reachedTarget) ? 1 : 0; }
        Debug.Log(generationNum.ToString() + " : " + avgFit.ToString() + " : " + sqsum.ToString() + " : " + numCrossed.ToString());
    }

    void updateMaximas()
    {
        for(int i=0; i<populationSize; i++)
        {
            if(individualMax[i] < scores[i])
            {
                individualMax[i] = scores[i];

                individualMaxWnB[i] = new float[hiddenLayersNodes.Length + 1][,];

                for (int l = 0; l <= hiddenLayersNodes.Length; l++)
                {
                    int rows = LayersNodes[l + 1];
                    int cols = LayersNodes[l] + 1;
                    individualMaxWnB[i][l] = new float[rows, cols];

                    for (int j = 0; j < rows; j++)
                    {
                        for (int k = 0; k < cols; k++)
                        {
                            individualMaxWnB[i][l][j, k] = weightsNBiases[i][l][j, k];
                        }
                    }
                }
            }
        }

        int maxInd = 0;
        float maxValArr = scores[maxInd];
        for(int i=0; i<populationSize; i++)
        {
            if(scores[i] > maxValArr)
            {
                maxInd = i;
                maxValArr = scores[i];
            }
        }

        if(maxValArr > globalMax)
        {
            globalMax = maxValArr;
            globalMaxWnB = new float[hiddenLayersNodes.Length + 1][,];

            for (int l = 0; l <= hiddenLayersNodes.Length; l++)
            {
                int rows = LayersNodes[l + 1];
                int cols = LayersNodes[l] + 1;
                globalMaxWnB[l] = new float[rows, cols];

                for (int j = 0; j < rows; j++)
                {
                    for (int k = 0; k < cols; k++)
                    {
                        globalMaxWnB[l][j, k] = weightsNBiases[maxInd][l][j, k];
                    }
                }
            }
        }
    }

    void recalculateSpeeds()
    {
        float sumPara = 0, sumPara1 = 0, sumPara2 = 0, sumPara3 = 0;
        int numPara = 0;
        for (int i = 0; i < populationSize; i++)
        {
            velocity[i] = new float[hiddenLayersNodes.Length + 1][,];
            float r1 = Random.Range(0f, 2f);
            float r2 = Random.Range(0f, 2f);

            for (int l = 0; l <= hiddenLayersNodes.Length; l++)
            {
                int rows = LayersNodes[l + 1];
                int cols = LayersNodes[l] + 1;
                velocity[i][l] = new float[rows, cols];

                for (int j = 0; j < rows; j++)
                {
                    for (int k = 0; k < cols; k++)
                    {
                        velocity[i][l][j, k] = w * velocity[i][l][j, k] + r1 * c1 * (individualMaxWnB[i][l][j, k] - weightsNBiases[i][l][j, k]) + r2 * c2 * (globalMaxWnB[l][j, k] - weightsNBiases[i][l][j, k]);
                        weightsNBiases[i][l][j, k] = weightsNBiases[i][l][j, k] + velocity[i][l][j, k];
                        sumPara += weightsNBiases[i][l][j, k];
                        sumPara1 += w * velocity[i][l][j, k];
                        sumPara2 += r1 * c1 * (individualMaxWnB[i][l][j, k] - weightsNBiases[i][l][j, k]);
                        sumPara3 += r2 * c2 * (globalMaxWnB[l][j, k] - weightsNBiases[i][l][j, k]);
                        numPara++;
                    }
                }
            }
        }
        //Debug.Log(sumPara / numPara);
        //Debug.Log(sumPara1 / numPara);
        //Debug.Log(sumPara2 / numPara);
        //Debug.Log(sumPara3 / numPara);
    }

    private void FixedUpdate()
    {
        timer += Time.deltaTime;

        if (timer > simDuration)
        {
            timer = 0;
            calcScores();
            updateMaximas();
            recalculateSpeeds();

            respawn();
            generationNum++;
        }
    }
}
