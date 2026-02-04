[System.Serializable]
public class CartItem
{
    public string itemName;
    public int itemPrice;

    public CartItem(string name, int price)
    {
        itemName = name;
        itemPrice = price;
    }
}
