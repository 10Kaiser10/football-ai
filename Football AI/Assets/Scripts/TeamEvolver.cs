using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamEvolver : MonoBehaviour
{
    float timer;
    public float simDuration = 10;
    int generationNum = 0;
    public float mutationNum = 10;

    int[] winners;

    public GameObject prefab;
    public GameObject parent;

    public int populationSize = 100;
    public int spawnRows = 5;
    GameObject[] population;

    [System.NonSerialized]
    public int inputNodes = 4;
    [System.NonSerialized]
    public int outputNodes = 12;
    [System.NonSerialized]
    public int[] hiddenLayersNodes = { };
    [System.NonSerialized]
    public int[] LayersNodes = { 4, 12 };
    private float[][][,] weightsNBiases;
    private float[][][,] nextWeightsNBiases;
    private Vector2 initiantionRange = new Vector2(-1f, 1f);

    public void Begin()
    {
        winners = new int[populationSize/2];
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

        population = new GameObject[populationSize/2];

        for (int i = 0; i < populationSize/2; i++)
        {
            int j = populationSize/2 + i;
            Vector3 spawnPos = new Vector3(20 * (i / spawnRows), 0, 25 * (i % spawnRows));
            population[i] = Instantiate(prefab, spawnPos, Quaternion.identity, parent.transform);

            TeamBrain objBrain = population[i].transform.GetChild(2).transform.GetChild(0).GetComponent<TeamBrain>();
            objBrain.ball = population[i].transform.GetChild(0).GetComponent<Transform>();
            objBrain.inputNodes = inputNodes;
            objBrain.outputNodes = outputNodes;
            objBrain.hiddenLayersNodes = hiddenLayersNodes;
            objBrain.LayersNodes = LayersNodes;
            objBrain.weightsNBiases = weightsNBiases[i];
            objBrain.side = 1;

            objBrain = population[i].transform.GetChild(3).transform.GetChild(0).GetComponent<TeamBrain>();
            objBrain.ball = population[i].transform.GetChild(0).GetComponent<Transform>();
            objBrain.inputNodes = inputNodes;
            objBrain.outputNodes = outputNodes;
            objBrain.hiddenLayersNodes = hiddenLayersNodes;
            objBrain.LayersNodes = LayersNodes;
            objBrain.weightsNBiases = weightsNBiases[j];
            objBrain.side = -1;
        }
    }

    private void Start()
    {
        PlayerNeuralNet.load();
        timer = 0;
        Begin();
    }

    void calcScores()
    {
        float goals = 0;
        float touches = 0;
        float scoresum = 0;

        for (int i = 0; i < populationSize / 2; i++)
        {
            BallScorer bs = population[i].transform.GetChild(0).GetComponent<BallScorer>();
            goals += bs.t1Goals + bs.t2Goals;
            touches += bs.t1Touches + bs.t2Touches;
            scoresum += 2f * bs.t1Goals + 0.5f * bs.t1Touches - 0.01f * (bs.mint1p1 + bs.mint1p2 + bs.mint1p3) + 2f * bs.t2Goals + 0.5f * bs.t2Touches - 0.01f * (bs.mint2p1 + bs.mint2p2 + bs.mint2p3);

            if (2 * bs.t1Goals + 0.5*bs.t1Touches - 0.01*(bs.mint1p1 + bs.mint1p2 + bs.mint1p3) >= 2 * bs.t2Goals + 0.5*bs.t2Touches - 0.01 * (bs.mint2p1 + bs.mint2p2 + bs.mint2p3))
            {
                winners[i] = 0;
            }
            else
            {
                winners[i] = 1;
            }

            if (i == 0)
            {
                //Debug.Log((bs.mint1p1 + bs.mint1p2 + bs.mint1p3).ToString() + " : " + (bs.mint2p1 + bs.mint2p2 + bs.mint2p3).ToString());
                //Debug.Log((0.01 * (bs.mint1p1 + bs.mint1p2 + bs.mint1p3)).ToString());
                Debug.Log("P1: "+(2 * bs.t1Goals + 0.5 * bs.t1Touches - 0.01 * (bs.mint1p1 + bs.mint1p2 + bs.mint1p3)).ToString());
            }
        }

        goals /= populationSize;
        touches /= populationSize;
        scoresum /= populationSize;

        string str = generationNum.ToString() + " : " + goals.ToString() + " : " + touches.ToString() + " : " + scoresum.ToString();

        Debug.Log(generationNum.ToString() + " : " + goals.ToString() + " : " + touches.ToString() + " : " + scoresum.ToString());
        System.IO.StreamWriter file = new System.IO.StreamWriter("Assets/Data/" + "teamTrain.txt", append: true);
        file.WriteLine(str);
        file.Close();
        

    //for (int i = 0; i < populationSize / 2; i++)
    //{
    //    BallScorer bs = population[i].transform.GetChild(0).GetComponent<BallScorer>();

    //    if (bs.mint1p1 + bs.mint1p2 + bs.mint1p3 <= bs.mint2p1 + bs.mint2p2 + bs.mint2p3)
    //    {
    //        winners[i] = 0;
    //    }
    //    else
    //    {
    //        winners[i] = 1;
    //    }

    //    if(i==0)
    //    {
    //        Debug.Log((bs.mint1p1 + bs.mint1p2 + bs.mint1p3).ToString() + " : " + (bs.mint2p1 + bs.mint2p2 + bs.mint2p3).ToString());
    //    }
    //}
}

    void selection()
    {
        nextWeightsNBiases = new float[populationSize][][,];

        for (int i = 0; i < populationSize / 2; i++)
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
                        nextWeightsNBiases[i][l][j, k] = weightsNBiases[winners[i]*(populationSize/2) + i][l][j, k];
                    }
                }
            }
        }
    }

    private void crossover()
    {
        for (int i = populationSize / 2; i < populationSize; i++)
        {
            nextWeightsNBiases[i] = new float[hiddenLayersNodes.Length + 1][,];
            int par1 = Random.Range(0, populationSize / 2 - 1), par2 = Random.Range(0, populationSize / 2 - 1);

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

    void mutation()
    {
        float dimentions = 0;
        for (int i = 1; i < LayersNodes.Length; i++)
        {
            dimentions += (1 + LayersNodes[i - 1]) * LayersNodes[i];
        }
        float mutationProb = mutationNum / 100;
        mutationNum = Mathf.Max(0.01f, mutationNum/1.0555f);

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
                        if (Random.Range(0f, 1f) <= mutationProb)
                        {
                            //int par1 = Random.Range(0, populationSize);
                            //int par2 = Random.Range(0, populationSize);
                            //nextWeightsNBiases[i][l][j, k] += 0.5f * (nextWeightsNBiases[par1][l][j, k] - nextWeightsNBiases[par2][l][j, k]);

                            nextWeightsNBiases[i][l][j, k] += Random.Range(0.5f * initiantionRange.x, 0.5f * initiantionRange.y);
                        }
                    }
                }
            }
        }
    }

    void respawn()
    {
        foreach (GameObject obj in population)
        {
            Destroy(obj);
        }

        population = new GameObject[populationSize / 2];

        for (int i = 0; i < populationSize / 2; i++)
        {
            int j = populationSize / 2 + i;
            Vector3 spawnPos = new Vector3(20 * (i / spawnRows), 0, 25 * (i % spawnRows));
            population[i] = Instantiate(prefab, spawnPos, Quaternion.identity, parent.transform);

            TeamBrain objBrain = population[i].transform.GetChild(2).transform.GetChild(0).GetComponent<TeamBrain>();
            objBrain.ball = population[i].transform.GetChild(0).GetComponent<Transform>();
            objBrain.inputNodes = inputNodes;
            objBrain.outputNodes = outputNodes;
            objBrain.hiddenLayersNodes = hiddenLayersNodes;
            objBrain.LayersNodes = LayersNodes;
            objBrain.weightsNBiases = weightsNBiases[i];
            objBrain.side = 1;

            objBrain = population[i].transform.GetChild(3).transform.GetChild(0).GetComponent<TeamBrain>();
            objBrain.ball = population[i].transform.GetChild(0).GetComponent<Transform>();
            objBrain.inputNodes = inputNodes;
            objBrain.outputNodes = outputNodes;
            objBrain.hiddenLayersNodes = hiddenLayersNodes;
            objBrain.LayersNodes = LayersNodes;
            objBrain.weightsNBiases = weightsNBiases[j];
            objBrain.side = -1;
        }


    }

    void weightsum()
    {
        float sum = 0;
        float sqsum = 0;
        int dim = 0;

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
                        float w = weightsNBiases[i][l][j, k];
                        sum += w;
                        sqsum += w * w;
                        dim++;
                    }
                }
            }
        }

        sum /= dim;
        sqsum /= dim;
        Debug.Log(sum.ToString() + " : " + sqsum.ToString());
    }

    private void FixedUpdate()
    {
        timer += Time.deltaTime;

        if (timer > simDuration)
        {
            timer = 0;
            calcScores();
            selection();
            crossover();
            mutation();

            weightsNBiases = nextWeightsNBiases;

            weightsum();

            respawn();
            generationNum++;
        }
    }
}
