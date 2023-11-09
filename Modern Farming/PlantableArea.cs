using System.Collections.Generic;
using UnityEngine;

public class PlantableArea : MonoBehaviour
{
    [SerializeField] private List<PlantablePoint> plantablePoints = new List<PlantablePoint>();
    [Min(0.1f)]
    [SerializeField] private float countFactor;
    [SerializeField] private float distanceBetweenPlants;
    public Transform area;
    public Transform plantsParent;
    public GameObject obj;

    private void Awake()
    {
        CreatePoints();
    }

    private void CreatePoints()
    {
        if (area == null)
            return;

        int width = (int)(area.localScale.x / countFactor);
        int height = (int)(area.localScale.z / countFactor);
        float xFactor = distanceBetweenPlants / width;
        float zFactor = distanceBetweenPlants / height;
        PlantablePoint point;
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                GameObject go = Instantiate(obj);
                go.transform.SetParent(plantsParent, true);
                go.transform.localPosition = new Vector3(x * xFactor - xFactor * width * 0.5f + xFactor * 0.5f, 0, z * zFactor - zFactor * height * 0.5f + zFactor * 0.5f);
                point.point = new Vector3(x * xFactor - xFactor * width * 0.5f + xFactor * 0.5f, 0, z * zFactor - zFactor * height * 0.5f + zFactor * 0.5f);
                point.isEmpty = true;
                plantablePoints.Add(point);
            }
        }
        PlantsController.instance.CreatingPointsFinished(plantablePoints.Count);
    }

}

public struct PlantablePoint
{
    public Vector3 point;
    public bool isEmpty;
}