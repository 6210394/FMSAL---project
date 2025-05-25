using UnityEngine;
using UnityEngine.Events;

public class InventoryScript : MonoBehaviour
{
    [Header("Inventory Settings")]
    public int inventorySize = 20;
    
    [Header("Events")]
    public UnityEvent<int> OnInventoryChanged;
    
    private IObjectType[] items;
    
    void Awake()
    {
        InitializeInventory();
    }
    
    void InitializeInventory()
    {
        items = new IObjectType[inventorySize];
    }
    
    public bool AddItem(IObjectType item)
    {
        // Find first empty slot
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                items[i] = item;
                OnInventoryChanged?.Invoke(i);
                return true;
            }
        }
        
        Debug.Log("Inventory is full!");
        return false;
    }
    
    public bool RemoveItem(IObjectType item)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == item)
            {
                items[i] = null;
                OnInventoryChanged?.Invoke(i);
                return true;
            }
        }
        
        return false;
    }
    
    public bool RemoveItemAt(int index)
    {
        if (index >= 0 && index < items.Length && items[index] != null)
        {
            items[index] = null;
            OnInventoryChanged?.Invoke(index);
            return true;
        }
        
        return false;
    }
    
    public void UseItem(int index)
    {
        if (index >= 0 && index < items.Length && items[index] != null)
        {
            IObjectType item = items[index];
            item.Use();
            
            if (item.isConsumable)
            {
                RemoveItemAt(index);
            }
        }
    }
    
    public IObjectType GetItem(int index)
    {
        if (index >= 0 && index < items.Length)
        {
            return items[index];
        }
        return null;
    }
    
    public int GetItemCount(IObjectType item)
    {
        int count = 0;
        foreach (IObjectType inventoryItem in items)
        {
            if (inventoryItem == item)
                count++;
        }
        return count;
    }
    
    public bool HasItem(IObjectType item)
    {
        return GetItemCount(item) > 0;
    }
    
    public IObjectType[] GetAllItems()
    {
        return items;
    }
    
    public int GetFirstEmptySlot()
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
                return i;
        }
        return -1; // No empty slots
    }
    
    public bool IsFull()
    {
        return GetFirstEmptySlot() == -1;
    }
}