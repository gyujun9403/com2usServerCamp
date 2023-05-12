using DungeonFarming.DataBase.GameDb;
using DungeonFarming.ReqResDTO;

namespace DungeonFarming.DataBaseServices.GameDb.MasterDataDTO
{


    public class StageInfo
    {
        public Int64 stage_code { get; set; }
        public String stage_name { get; set; }
        public Int32 required_user_level { get; set; }
    }

    public class StageTotalInfo
    {
        public Int64 stageId { get; set; }

    }
}
