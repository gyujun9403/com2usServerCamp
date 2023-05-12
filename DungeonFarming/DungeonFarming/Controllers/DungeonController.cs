using DungeonFarming.DataBase.GameDb;
using DungeonFarming.DataBase.GameSessionDb;
using DungeonFarming.DTO;
using DungeonFarming.ReqResDTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DungeonFarming.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DungeonController : ControllerBase
    {
        readonly IGameDb _gameDb;
        readonly IGameSessionDb _gameSessionDb;
        readonly IMasterDataOffer _masterDataOffer;
        readonly Logger<DungeonController> _logger;

        public DungeonController(IGameDb gameDb, IGameSessionDb gameSessionDb, 
            Logger<DungeonController> logger, IMasterDataOffer masterDataOffer)
        {
            _gameDb = gameDb;
            _gameSessionDb = gameSessionDb;
            _logger = logger;
            _masterDataOffer = masterDataOffer;
        }

        //[HttpPost("GetStageList")]
        //public Task<GetStageListResponse> GetStage(AttendanceGetStackRequst request)
        //{
        //    // context에서 userId를 가져온다
        //    // 마스터 데이터에서 던전 정보(List)를 받아온다
        //    // 유저 데이터를 받아오고, 클리어 정보를 반영하여 리스트 제공.
        //}
    }
}
