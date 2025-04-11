using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/ItemData")]
public class ItemData : ScriptableObject
{
	public Sprite itemIcon;
	public string itemName;
    public int width = 1;
    public int height = 1;
    public int cost;
}