using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

public class SaveNN : MonoBehaviour
{
    public bool forSaving = true;
    public bool forPlaying = true;
    public int indexToUse = 0;

    NNwnb bestNN = new NNwnb();
    Brain2 playerBrain;
    Vector2 spawnPos = new Vector2(6, 9);

    [System.Serializable]
    public struct NNwnb
    {
        public int inputNodes;
        public int outputNodes;
        public int[] hiddenLayersNodes;
        public int[] LayersNodes;
        public float[][][,] weightsNBiases;
    };

    public void save()
    {
        NNwnb bestNN = new NNwnb();
        bestNN.weightsNBiases = gameObject.GetComponent<Evolver2>().returnBest();
        bestNN.inputNodes = gameObject.GetComponent<Evolver2>().inputNodes;
        bestNN.outputNodes = gameObject.GetComponent<Evolver2>().outputNodes;
        bestNN.hiddenLayersNodes = gameObject.GetComponent<Evolver2>().hiddenLayersNodes;
        bestNN.LayersNodes = gameObject.GetComponent<Evolver2>().LayersNodes;

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream saveFile = File.Create("Assets/SavedNetworks/" + "playerMotion.xml");
        formatter.Serialize(saveFile, bestNN);
        saveFile.Close();
    }

    public void load()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream saveFile = File.OpenRead("Assets/SavedNetworks/" + "playerMotion.xml");
        bestNN = (NNwnb)formatter.Deserialize(saveFile);
        saveFile.Close();

        playerBrain.inputNodes = bestNN.inputNodes;
        playerBrain.outputNodes = bestNN.outputNodes;
        playerBrain.hiddenLayersNodes = bestNN.hiddenLayersNodes;
        playerBrain.LayersNodes = bestNN.LayersNodes;
        playerBrain.weightsNBiases = bestNN.weightsNBiases[indexToUse];

        //Debug.Log(bestNN.inputNodes);
        //Debug.Log(bestNN.outputNodes);
    }

    public void setTarget()
    {
        playerBrain.target = new Vector3(Random.Range(-spawnPos.x, spawnPos.x), 0.5f, Random.Range(-spawnPos.y, spawnPos.y));
        playerBrain.targetVel = new Vector3(Random.Range(-8, 8), 0, Random.Range(-8, 8));
        playerBrain.simTime = 10;

        gameObject.transform.parent.transform.GetChild(2).transform.localPosition = playerBrain.target;
        float yrot = Vector2.Angle(Vector2.right, new Vector2(playerBrain.targetVel.x, playerBrain.targetVel.z));
        gameObject.transform.parent.transform.GetChild(2).transform.Rotate(yrot, 0, 0);
    }

    public void loadUp()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream saveFile = File.OpenRead("Assets/SavedNetworks/" + "playerMotion.xml");
        bestNN = (NNwnb)formatter.Deserialize(saveFile);
        saveFile.Close();
    }

    public int getInputNodes()
    {
        return bestNN.inputNodes;
    }

    public int getoutputNodes()
    {
        return bestNN.outputNodes;
    }

    public int[] getHiddenNodes()
    {
        return bestNN.hiddenLayersNodes;
    }

    public int[] getLayerNodes()
    {
        return bestNN.LayersNodes;
    }

    public float[][,] getwnbs()
    {
        return bestNN.weightsNBiases[indexToUse];
    }

    private void Start()
    {
        if(forPlaying == true)
        {
            return;
        }
        if(forSaving == false)
        {
            playerBrain = gameObject.GetComponent<Brain2>();
            load();
            setTarget();
        }
    }
}
