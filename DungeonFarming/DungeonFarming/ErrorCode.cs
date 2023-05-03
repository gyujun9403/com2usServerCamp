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
    InvalidIdFormat = 10,
    DuplicatedId = 11,
    InvalidId = 12,
    InvalidToken = 20,
    InvalidPasswordFormat = 30,
    WorngPassword = 31,
    WorngClientVersion = 40,
    WorngMasterDataVersion = 41,

    // MAIL : 50 ~ 99
    InvalidMailId = 50,
    InvalidMailPage = 51,
}
