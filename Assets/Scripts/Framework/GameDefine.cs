public class NotiDefine
{
    public const string APP_START_UP = "APP_START_UP";
    public const string MVC_STARTED = "MVC_STARTED";

    public const string WINDOW_DO_SHOW = "WINDOW_DO_SHOW";
    public const string WINDOW_DO_HIDE = "WINDOW_DO_HIDE";
    public const string WINDOW_DO_DESTROY = "WINDOW_DO_DESTROY";
    public const string WINDOW_HAS_SHOW = "WINDOW_HAS_SHOW";
    public const string FULLSCREEN_WINDOW_SHOW = "FULLSCREEN_WINDOW_SHOW";
    public const string GAME_RESET = "GAME_RESET";

    public const string OPEN_ = "OPEN_";
    public const string CLOSE_ = "CLOSE_";

    public const string DoLoadScene = "DoLoadScene";
    public const string LoadSceneFinish = "LoadSceneFinish";

   
    public const string AddTimestepCallback = "AddTimestepCallback";
    public const string RemoveTimestepCallback = "RemoveTimestepCallback";//key，动态生成的唯一标识

    public const string TEST_CALLBACK_NOTI = "TEST_CALLBACK_NOTI";

    public const string CreateOneBuildingDo = "CreateOneBuildingDo";
    public const string CreateOneBuildingResp = "CreateOneBuildingResp";

    public const string UpgradeOneBuildingDo = "UpgradeOneBuildingDo";
    public const string UpgradeOneBuildingResp = "UpgradeOneBuildingResp";


    public const string BuildingExpireReachedNoti = "BuildingExpireReachedNoti";
    public const string BuildingStatusChanged = "BuildingStatusChanged";

    public const string BuildingRelocateDo = "BuildingRelocateDo";
    public const string BuildingRelocateResp = "BuildingRelocateResp";

    public const string TryBuildBuilding = "TryBuildBuilding";

    public const string BuildingCancelDo = "BuildingCancelDo";
    public const string BuildingSpeedUpDo = "BuildingSpeedUpDo";

    public const string BuildingCancelUpgradeNoti = "BuildingCancelUpgradeNoti";
    public const string BuildingRemoveNoti = "BuildingRemoveNoti";

    public const string ConfirmBuild = "ConfirmBuild";

    public const string GenerateMySpotDo = "GenerateMySpotDo";
    public const string GenerateMySpotResp = "GenerateMySpotResp";

    public const string GenerateMyBuildingDo = "GenerateMyBuildingDo";
    public const string GenerateMyBuildingResp = "GenerateMyBuildingResp";

    public const string ShowBuildingInfo = "ShowBuildingInfo";

    public const string LoadRoleDo = "LoadRoleDo";
    public const string LoadRoleResp = "LoadRoleResp";

}

public enum MediatorDefine
{
    NONE,
    DATA_CENTER,
    LOGIN,
    HOME_LAND,
    SCENE_LOADER,
    MAIN,
    BUILD_CENTER
}

class ProxyNameDefine
{
    public const string TIME_CENTER = "TIME_CENTER";
    public const string WORLD = "WORLD";
    public const string ROLE = "ROLE";
}

public class SceneDefine
{
    public const string World = "World";
    public const string Home = "Home";
    public const string GameIndex = "GameIndex";
}
