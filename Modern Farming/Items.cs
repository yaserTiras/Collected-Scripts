using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "ItemsList", menuName = "New Item List")]
public class Items : ScriptableObject
{
    [SerializeField] private ItemValues[] itemsList;

    public Sprite GetPlantSprite(PlantType plantType)
    {
        for (int i = 0; i < itemsList.Length; i++)
        {
            if (itemsList[i].type == plantType)
                return itemsList[i].sprite;
        }
        Debug.Log("Not Found");
        return null;
    }
}

[System.Serializable]
public class ItemValues
{
    public PlantType type;
    public Sprite sprite;
}
