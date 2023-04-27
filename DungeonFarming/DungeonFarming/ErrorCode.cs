﻿using System;

namespace DungeonFarming
{
    public enum ErrorCode : Int16
    {
        // Generic Error : 0 ~ 10
        ErrorNone = 0,
        AccountDbError = 1,
        GameSessionDbError = 2,

        // Auth 10 ~ 99
        //  ID : 10 ~ 19, Token : 20 ~ 29, Password : 30 ~ 39
        InvalidIdFormat = 10,
        DuplicatedId = 11,
        InvalidId = 12,
        InvalidToken = 20,
        InvalidPasswordFormat = 30,
        InvalidPassword = 31,


    }
}
