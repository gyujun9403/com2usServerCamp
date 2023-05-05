using System;

public enum ErrorCode : Int16
{
    // Generic Error : 0 ~ 10
    None = 0,
    AccountDbError = 1,
    GameSessionDbError = 2,
    ServerError = 3,
    InvalidBodyForm = 4,
    GameDbError = 5,

    // Auth 10 ~ 99
    //  ID : 10 ~ 19, Token : 20 ~ 29, Password : 30 ~ 39
    //  Version : 40 ~ 49
    AreadyLogin = 10,
    InvalidIdFormat = 11,
    DuplicatedId = 12,
    InvalidId = 13,
    InvalidToken = 20,
    InvalidPasswordFormat = 30,
    WorngPassword = 31,
    WorngClientVersion = 40,
    WorngMasterDataVersion = 41,

    // MAIL : 50 ~ 59
    InvalidMailId = 50,
    InvalidMailPage = 51,

    // Purchase : 60 ~ 69
    InvalidPurchaseToken = 60,
    InvalidPackageId = 61,
    DuplicatedPurchaseToken = 62,
}
