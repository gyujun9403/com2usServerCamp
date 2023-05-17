using DungeonFarming.DataBase.GameSessionDb;
using Microsoft.AspNetCore.Mvc;
using ZLogger;

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
            (response.errorCode, response.notice) = await _gameSessionDb.GetNotice();

            return response;
        }
    }
}
