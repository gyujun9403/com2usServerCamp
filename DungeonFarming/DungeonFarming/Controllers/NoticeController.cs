using CloudStructures.Structures;
using CloudStructures;
using DungeonFarming.BodyData;
using DungeonFarming.DataBase.GameSessionDb;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DungeonFarming.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class NoticeController : ControllerBase
    {
        private IGameSessionDb _gameSessionDb;
        private ILogger<NoticeController> _logger;
        
        public NoticeController(IGameSessionDb gameSessionDb, ILogger<NoticeController> logger)
        {
            _gameSessionDb = gameSessionDb;
            _logger = logger;
        }

        [HttpPost]
        public async Task<NoticeResponse> Notice(NoticeRequest request)
        {
            //_gameSessionDb.
            NoticeResponse response = new NoticeResponse();
            response.notice = await _gameSessionDb.GetNotice();
            return response;
        }
    }
}
