﻿
public class MailPreview
{
    public Int64 mail_id { get; set; }
    public Int16 item0_code { get; set; }
    public Int64 item0_count { get; set; }
    public String mail_title { get; set; }
    public DateTime? expiration_date { get; set; }
}
public class Mail
{
    public Int64 mail_id { get; set; }
    public Int64 user_id { get; set; }
    public Int16 item0_code { get; set; }
    public Int64 item0_count { get; set; }
    public Int16 item1_code { get; set; }
    public Int64 item1_count { get; set; }
    public Int16 item2_code { get; set; }
    public Int64 item2_count { get; set; }
    public Int16 item3_code { get; set; }
    public Int64 item3_count { get; set; }
    public String mail_title { get; set; }
    public String mail_text { get; set; }
    public DateTime recieve_date { get; set; }
    public DateTime? expiration_date { get; set; }
    public Int16 is_deleted { get; set; }
}