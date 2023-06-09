﻿using DungeonFarming.DataBase.GameDb;
using DungeonFarming.DataBase.GameDb.GameDbModel;
using DungeonFarming.DataBase.GameSessionDb;
using DungeonFarming.DataBase.PurchaseDb;
using DungeonFarming.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZLogger;

namespace DungeonFarming.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PackagePurchaseController : ControllerBase
    {
        readonly ILogger<PackagePurchaseController> _logger;
        readonly IPurchaseDb _purchaseDb;
        readonly IGameDb _gameDb;
        readonly IMasterDataOffer _masterDataOffer;
        readonly GameSessionData _gameSessionData;

        public PackagePurchaseController(IHttpContextAccessor httpContextAccessor, ILogger<PackagePurchaseController> logger, 
            IPurchaseDb purchaseDb, IGameDb gameDb, IMasterDataOffer masterDataOffer)
        {
            _logger = logger;
            _purchaseDb = purchaseDb;
            _gameDb = gameDb;
            _masterDataOffer = masterDataOffer;
            _gameSessionData = httpContextAccessor.HttpContext.Items["userSession"] as GameSessionData;
        }

        [HttpPost]
        public async Task<PackagePurchaseResponse> PackagePurchase(PackagePurchaseRequest request)
        {
            PackagePurchaseResponse response = new PackagePurchaseResponse();
            response.packageListId = -1;
            // 영수증 유효성 확인
            if (CheckPurchaseValid(request.purchaseToken) == false)
            {
                response.errorCode = ErrorCode.InvalidPurchaseToken;
                _logger.ZLogWarningWithPayload(LogEventId.PackagePurchase, new { userAssignedId = request.userAssignedId, purchaseToken = request.purchaseToken }, "PackagePurchase purchase token INVALID");
                return response;
            }

            // 영수증 중복 확인
            response.errorCode = await _purchaseDb.CheckPurchaseDuplicated(request.purchaseToken);
            if (response.errorCode != ErrorCode.None)
            {
                return response;
            }

            // 패키지 번호 확인
            List<ItemBundle>? itemBundle = _masterDataOffer.getPackageItemBundles(request.packageCode);
            if (itemBundle == null)
            {
                response.errorCode = ErrorCode.InvalidPackageId;
                _logger.ZLogErrorWithPayload(LogEventId.PackagePurchase, new { userAssignedId = request.userAssignedId, packageCode = request.packageCode, purchaseToken = request.purchaseToken }, "PackagePurchase package id INVALID");
                return response;
            }

            // 구매 내역 입력
            response.errorCode = await _purchaseDb.WritePurchase(_gameSessionData.userId, request.purchaseToken, request.packageCode);
            if (response.errorCode != ErrorCode.None)
            {
                return response;
            }

            // 유저에게 메일로 전송
            response.errorCode = await _gameDb.SendMail(GeneratePackagePurchaseMail(_gameSessionData.userId, request.packageCode, itemBundle));
            if (response.errorCode != ErrorCode.None)
            {
                await _purchaseDb.DeletePurchase(_gameSessionData.userId, request.purchaseToken, request.packageCode); // rollback
                return response;
            }

            _logger.ZLogInformationWithPayload(LogEventId.PackagePurchase, new { userAssignedId = _gameSessionData.userAssignedId }, "package purchase SUCCESS");
            response.packageListId = request.packageCode;
            return response;
        }

        private bool CheckPurchaseValid(String purchaseToken)
        {
            // purchaseToken 포맷 검사
            // 구매 플랫폼으로 부터 purchaseToken이 유효한지 검사

            return true;
        }

        private Mail GeneratePackagePurchaseMail(Int64 userId, Int16 PackageCode, List<ItemBundle>? itemBundle)
        {
            var mail = new Mail();
            mail.user_id = userId;
            if (itemBundle != null && itemBundle.Count > 0)
            {
                mail.item0_code = itemBundle[0].itemCode;
                mail.item0_count = itemBundle[0].itemCount;
                if (itemBundle.Count > 1)
                {
                    mail.item1_code = itemBundle[1].itemCode;
                    mail.item1_count = itemBundle[1].itemCount;
                    if (itemBundle.Count > 2)
                    {
                        mail.item2_code = itemBundle[2].itemCode;
                        mail.item2_count = itemBundle[2].itemCount;
                    }
                    if (itemBundle.Count > 3)
                    {
                        mail.item3_code = itemBundle[3].itemCode;
                        mail.item3_count = itemBundle[3].itemCount;
                    }
                }
            }
            mail.mail_title = $"패키지 구매";
            mail.mail_text = $"구매하신 {PackageCode}번 패키지 아이템 입니다";
            mail.recieve_date = DateTime.Now;
            mail.expiration_date = new DateTime(9999, 12, 31, 23, 59, 59);
            return mail;
        }
    }
}
