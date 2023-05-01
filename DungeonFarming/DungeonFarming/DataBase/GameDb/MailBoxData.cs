public class MailData
{
    public List<ItemInfo> currencys { get; set; } = new();
    public List<ItemInfo> items { get; set; } = new();
    public String? mailText { get; set;}
    public DateTime expiration { get; set; }
}

public interface IMail
{
    public MailData getMailData();
}

public class Mail : IMail
{
    public Int64 mail_id { get; set; }
    public Int64 user_id { get; set; }
    public Int16 currency_code { get; set; }
    public Int64 currency_count { get; set; }
    public Int16 item0_code { get; set; }
    public Int64 item0_count { get; set; }
    public Int16 item1_code { get; set; }
    public Int64 item1_count { get; set; }
    public Int16 item2_code { get; set; }
    public Int64 item2_count { get; set; }
    public String mail_text { get; set; }
    public DateTime expiration_date { get; set; }

    public MailData getMailData()
    {
        MailData mailDatas = new MailData
        {
            currencys = new List<ItemInfo> 
            { 
                new ItemInfo
                {
                    itemId = currency_code,
                    itemNum = currency_count
                }
            },
            items = new List<ItemInfo>
            {
                new ItemInfo { itemId = item0_code, itemNum = item0_count },
                new ItemInfo { itemId = item1_code, itemNum = item1_count },
                new ItemInfo { itemId = item2_code, itemNum = item2_count }
            },
            mailText = mail_text,
            expiration = expiration_date
        };
        return mailDatas;
    } 
}