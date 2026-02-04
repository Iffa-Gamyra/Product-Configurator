using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CartStore", menuName = "Cart/CartStore")]
public class CartStore : ScriptableObject
{
    public List<CartItem> cartItems = new List<CartItem>();
    public int totalPrice;

    // Old behavior preserved: "Add only if missing"
    public void AddItem(string itemName, int itemPrice)
    {
        var item = cartItems.Find(x => x.itemName == itemName);
        if (item == null)
        {
            item = new CartItem(itemName, itemPrice);
            cartItems.Add(item);
            totalPrice += itemPrice;
        }
    }

    // NEW: Upsert / Set price for an item
    public void SetItem(string itemName, int itemPrice)
    {
        var item = cartItems.Find(x => x.itemName == itemName);
        if (item == null)
        {
            cartItems.Add(new CartItem(itemName, itemPrice));
        }
        else
        {
            item.itemPrice = itemPrice;
        }

        RecalculateTotal();
    }

    public void RemoveItem(string itemName)
    {
        var item = cartItems.Find(x => x.itemName == itemName);
        if (item != null)
        {
            cartItems.Remove(item);
            RecalculateTotal();
        }
    }


    public void ClearCart()
    {
        cartItems.Clear();
        totalPrice = 0;
    }

    public void RecalculateTotal()
    {
        int sum = 0;
        for (int i = 0; i < cartItems.Count; i++)
            sum += cartItems[i].itemPrice;

        totalPrice = sum;
    }
}
