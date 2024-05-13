using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "Level", menuName = "Level/New Level")]
public class Level : ScriptableObject
{
    public int Rows;
    public int Cols;
    public GameObject environment;
    public List<VehicleValues> nodeData = new List<VehicleValues>();

    // Helper function to get/set nodes by row/col
    public VehicleValues GetNode(int row, int col)
    {
        int index = row * Cols + col;
        if (index < 0 || index >= nodeData.Count) return null;
        return nodeData[index];
    }

    public void SetNode(int row, int col, VehicleValues node)
    {
        int index = row * Cols + col;
        if (index < 0 || index >= nodeData.Count) return;
        nodeData[index] = node;
    }
}