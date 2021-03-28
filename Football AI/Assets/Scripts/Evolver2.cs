using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Evolver2 : MonoBehaviour
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
    private int[] LayersNodes = { 6,2};
    private float[][][,] weightsNBiases;
    private float[][][,] nextWeightsNBiases;
    private Vector2 initiantionRange = new Vector2(-50000f, 50000f);

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
            objBrain.target = new Vector3(Random.Range(-spawnRadius, spawnRadius), 0.5f,Random.Range(-spawnRadius, spawnRadius));
            objBrain.targetVel = new Vector3(Random.Range(-targetSpeedRange, targetSpeedRange), 0, Random.Range(-targetSpeedRange, targetSpeedRange));
            objBrain.simTime = simDuration;

            population[i].transform.GetChild(2).transform.localPosition = objBrain.target;
            float yrot = Vector2.Angle(Vector2.right, new Vector2(objBrain.targetVel.x, objBrain.targetVel.z));
            population[i].transform.GetChild(2).transform.Rotate(yrot,0 , 0);
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
    }

    private void selection()
    {
        int avgFit = 0, sqsum = 0, numCrossed = 0;
        foreach (int item in scores) { avgFit += item; }
        avgFit /= populationSize;
        foreach (int item in scores) { sqsum += (item - avgFit)*(item - avgFit); }
        sqsum /= populationSize;
        foreach (GameObject item in population) { numCrossed += (item.transform.GetChild(0).GetComponent<Brain2>().reachedTarget) ? 1:0; }
        Debug.Log(generationNum.ToString() + " : " + avgFit.ToString() + " : " + sqsum.ToString() + " : " + numCrossed.ToString());

        System.Array.Sort(scores, weightsNBiases);

        nextWeightsNBiases = new float[populationSize][][,];

        for (int i = populationSize-1; i >= populationSize/2; i--)
        {
            //nextWeightsNBiases[i] = new float[hiddenLayersNodes.Length + 1][,];
            nextWeightsNBiases[populationSize - i - 1] = new float[hiddenLayersNodes.Length + 1][,];

            for (int l = 0; l <= hiddenLayersNodes.Length; l++)
            {
                int rows = LayersNodes[l + 1];
                int cols = LayersNodes[l] + 1;
                //nextWeightsNBiases[i][l] = new float[rows, cols];
                nextWeightsNBiases[populationSize - i - 1][l] = new float[rows, cols];

                for (int j = 0; j < rows; j++)
                {
                    for (int k = 0; k < cols; k++)
                    {
                        //nextWeightsNBiases[i][l][j, k] = weightsNBiases[i][l][j, k];
                        nextWeightsNBiases[populationSize - i - 1][l][j, k] = weightsNBiases[i][l][j, k];
                    }
                }
            }
        }
    }

    private void crossover()
    {
        for (int i = populationSize/2; i < populationSize; i++)
        {
            nextWeightsNBiases[i] = new float[hiddenLayersNodes.Length + 1][,];
            int par1 = Random.Range(0, populationSize/2 - 1), par2 = Random.Range(0, populationSize / 2 - 1);

            for (int l = 0; l <= hiddenLayersNodes.Length; l++)
            {
                int rows = LayersNodes[l + 1];
                int cols = LayersNodes[l] + 1;
                nextWeightsNBiases[i][l] = new float[rows, cols];

                for (int j = 0; j < rows; j++)
                {
                    for (int k = 0; k < cols; k++)
                    {
                        //float coeff = Random.Range(-1f, 1f);
                        //nextWeightsNBiases[i][l][j, k] = (0.5f - coeff) * nextWeightsNBiases[par1][l][j, k] + (0.5f + coeff) * nextWeightsNBiases[par2][l][j, k];

                        float coeff = Random.Range(0, 1);
                        nextWeightsNBiases[i][l][j, k] = coeff * nextWeightsNBiases[par1][l][j, k] + (1 - coeff) * nextWeightsNBiases[par2][l][j, k];
                    }
                }
            }
        }
    }

    private void randomise()
    {
        for (int i = 1; i < populationSize; i++)
        {
            nextWeightsNBiases[i] = new float[hiddenLayersNodes.Length + 1][,];

            for (int l = 0; l <= hiddenLayersNodes.Length; l++)
            {
                int rows = LayersNodes[l + 1];
                int cols = LayersNodes[l] + 1;
                nextWeightsNBiases[i][l] = new float[rows, cols];

                for (int j = 0; j < rows; j++)
                {
                    for (int k = 0; k < cols; k++)
                    {
                        nextWeightsNBiases[i][l][j, k] = Random.Range(initiantionRange.x, initiantionRange.y);
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

    private void mutation()
    {
        float dimentions = 0;
        for( int i=1; i<LayersNodes.Length; i++)
        {
            dimentions += (1 + LayersNodes[i - 1]) * LayersNodes[i];
        }
        float mutationProb = mutationNum / 100;
        //mutationNum = Mathf.Max(0.1f, mutationNum - 0.4f);

        for (int i = 0; i < populationSize; i++)
        {
            for (int l = 0; l <= hiddenLayersNodes.Length; l++)
            {
                int rows = LayersNodes[l + 1];
                int cols = LayersNodes[l] + 1;

                for (int j = 0; j < rows; j++)
                {
                    for (int k = 0; k < cols; k++)
                    {
                        if(Random.Range(0f,1f) <= mutationProb)
                        {
                            int par1 = Random.Range(0, populationSize);
                            int par2 = Random.Range(0, populationSize);
                            nextWeightsNBiases[i][l][j, k] += weightsNBiases[par1][l][j, k] - weightsNBiases[par2][l][j, k];
                        }
                    }
                }
            }
        }
    }

    private void FixedUpdate()
    {
        //Debug.Log(population[0].transform.GetChild(0).GetComponent<Brain2>().appliedForce);

        timer += Time.deltaTime;

        if (timer > simDuration)
        {
            timer = 0;
            calcScores();
            selection();
            crossover();
            mutation();

            weightsNBiases = nextWeightsNBiases;

            respawn();
            generationNum++;
        }
    }
}
