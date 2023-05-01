﻿using DungeonFarming.DataBase.GameSessionDb;
using Microsoft.AspNetCore.Mvc;

namespace DungeonFarming.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class NoticeController : ControllerBase
    {
        private readonly IGameSessionDb _gameSessionDb;
        private readonly ILogger<NoticeController> _logger;
        

        public NoticeController(IGameSessionDb gameSessionDb, ILogger<NoticeController> logger)
        {
            _gameSessionDb = gameSessionDb;
            _logger = logger;
        }

        [HttpPost]
        public async Task<NoticeResponse> Notice(NoticeRequest request)
        {
            NoticeResponse response = new NoticeResponse();
            response.notice = await _gameSessionDb.GetNotice();
            return response;
        }
    }
}
