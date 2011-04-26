using System.Collections.Generic;
using System.Text;
using Analytics.Data;
public class ProfileCollection : List<Item>
{
    // Methods
    public List<string> ToSimplifiedList()
    {
        List<string> list = new List<string>();
        foreach (Item item in this)
        {
            list.Add(item.Value);
        }
        return list;
    }

    public string ToSimplifiedString()
    {
        StringBuilder builder = new StringBuilder();
        foreach (Item item in this)
        {
            builder.Append(item.Key);
        }
        return builder.ToString();
    }

    public string ToString(int profileCounter)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append((base.Count > 0) ? "?ids=" : string.Empty);
        List<string> list = new List<string>();
        foreach (Item item in this)
        {
            if (!item.Key.Contains("ga:"))
            {
                list.Add("ga:" + item.Key);
            }
            else
            {
                list.Add(item.Key);
            }
        }
        builder.Append(list[profileCounter].ToString());
        return builder.ToString();
    }
}

