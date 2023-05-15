
using DungeonFarming.DataBase.GameDb;

public class MailPreview
{
    public Int64 mail_id { get; set; }
    public Int64 item0_code { get; set; }
    public Int64 item0_count { get; set; }
    public String mail_title { get; set; }
    public DateTime? expiration_date { get; set; }
}
public class Mail
{
    public Int64 mail_id { get; set; }
    public Int64 user_id { get; set; }
    public Int64 item0_code { get; set; } = -1;
    public Int64 item0_count { get; set; } = -1;
    public Int64 item1_code { get; set; } = -1;
    public Int64 item1_count { get; set; } = -1;
    public Int64 item2_code { get; set; } = -1;
    public Int64 item2_count { get; set; } = -1;
    public Int64 item3_code { get; set; } = -1;
    public Int64 item3_count { get; set; } = -1;
    public String mail_title { get; set; }
    public String mail_text { get; set; }
    public DateTime recieve_date { get; set; }
    public DateTime? expiration_date { get; set; }
    public Int16 is_deleted { get; set; } = 0;

    public List<ItemBundle>? AttachedItemBundle()
    {
        List<ItemBundle>? itemBundles = new List<ItemBundle>();

        if (item0_code != -1 && item0_count != -1)
        {
            ItemBundle itemBundle = new ItemBundle
            {
                itemCode = item0_code,
                itemCount = item0_count
            };
            itemBundles.Add(itemBundle);
        }

        if (item1_code != -1 && item1_count != -1)
        {
            ItemBundle itemBundle = new ItemBundle
            {
                itemCode = item1_code,
                itemCount = item1_count
            };
            itemBundles.Add(itemBundle);
        }

        if (item2_code != -1 && item2_count != -1)
        {
            ItemBundle itemBundle = new ItemBundle
            {
                itemCode = item2_code,
                itemCount = item2_count
            };
            itemBundles.Add(itemBundle);
        }

        if (item3_code != -1 && item3_count != -1)
        {
            ItemBundle itemBundle = new ItemBundle
            {
                itemCode = item3_code,
                itemCount = item3_count
            };
            itemBundles.Add(itemBundle);
        }

        if (itemBundles.Count == 0)
        {
            return null;
        }
        return itemBundles;
    }
}