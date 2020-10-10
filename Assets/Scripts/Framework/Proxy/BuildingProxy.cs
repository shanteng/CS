using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using SMVC.Patterns;
using UnityEngine;


[Serializable]
public class BuildingData
{
    public enum BuildingStatus
    {
        INIT = 0,//等待创建的状态
        NORMAL,
        BUILD,
        UPGRADE,
    }

    public int _id;
    public string _key;
    public BuildingConfig _config;
    public BuildingUpgradeConfig _configLevel;//当前等级配置
    public int _UpgradeSecs = 0;//下一个等级所需时间
    public Vector2Int _cordinate = Vector2Int.zero;//左下角的坐标
    public List<Vector2Int> _occupyCordinates;//占领的全部地块坐标

    public int _level = 0;
    public BuildingStatus _status = BuildingStatus.INIT;//建筑的状态
    public long _expireTime;//建造或者升级的到期时间
    public int _durability;//当前耐久度

    public void Create(int id,int x,int z)
    {
        this._key =  UtilTools.GenerateUId();
        this._id = id;
        this._config = BuildingConfig.Instance.GetData(id);
        this._occupyCordinates = new List<Vector2Int>();
        this.SetCordinate(x, z);
    }

    public void SetStatus(BuildingStatus state)
    {
        this._status = state;
        this._expireTime = 0;
        _UpgradeSecs = 0;
        if (state == BuildingStatus.BUILD || state == BuildingStatus.UPGRADE)
        {
            BuildingUpgradeConfig nextConfig = BuildingUpgradeConfig.GetConfig(this._id, this._level + 1);
            _UpgradeSecs = nextConfig == null ? 0 : nextConfig.NeedTime;
            this._expireTime = UtilTools.GetExpireTime(_UpgradeSecs);
        }
    }

    public void SetLevel(int newLevel)
    {
        this._level = newLevel;
        _configLevel = BuildingUpgradeConfig.GetConfig(this._id, this._level);
        if (this._configLevel != null)
            this._durability = _configLevel.Durability;
    }

    public void SetCordinate(int x, int z)
    {
        this._cordinate.x = x;
        this._cordinate.y = z;
        this._occupyCordinates.Clear();
        for (int row = 0; row < this._config.RowCount; ++row)
        {
            int curX = this._cordinate.x + row;
            for (int col = 0; col < this._config.ColCount; ++col)
            {
                int curZ = this._cordinate.y + col;
                Vector2Int corNow = new Vector2Int(curX, curZ);
                this._occupyCordinates.Add(corNow);
            }
        }
    }
}//end class

//时间戳回调管理
public class BuildingProxy : BaseRemoteProxy
{
    private Dictionary<string, BuildingData> _datas = new Dictionary<string, BuildingData>();

    public BuildingProxy() : base(ProxyNameDefine.BUILDING)
    {
        
    }

    public BuildingData GetBuilding(string key)
    {
        BuildingData data = null;
        if (this._datas.TryGetValue(key, out data))
        {
            return data;
        }
        return null;
    }

    public void Create(Dictionary<string, object> vo)
    {
        int id = (int)vo["configid"];
        int x = (int)vo["x"];
        int z = (int)vo["z"];

        BuildingData data = new BuildingData();
        data.Create(id, x, z);
        data.SetLevel(0);
        data.SetStatus(BuildingData.BuildingStatus.BUILD);
        this._datas.Add(data._key, data);
        //通知时间中心添加一个监听
        TimeCallData dataTime = new TimeCallData();
        dataTime._notifaction = NotiDefine.BuildingExpireReachedNoti;
        dataTime.TimeStep = data._expireTime;
        dataTime._param = data._key;
        MediatorUtil.SendNotification(NotiDefine.AddTimestepCallback, dataTime);

        MediatorUtil.SendNotification(NotiDefine.CreateOneBuildingResp, data);
    }

    public void Relocate(Dictionary<string, object> vo)
    {
        string key = (string)vo["key"];
        int x = (int)vo["x"];
        int z = (int)vo["z"];
        BuildingData data = this.GetBuilding(key);
        if (data != null)
        {
            data.SetCordinate(x, z);
            MediatorUtil.SendNotification(NotiDefine.BuildingRelocateResp, key);
        }
    }

    public void OnExpireFinsih(string key)
    {
        BuildingData data = this.GetBuilding(key);
        if (data == null)
            return;
        if (data._status == BuildingData.BuildingStatus.UPGRADE ||
            data._status == BuildingData.BuildingStatus.BUILD)
        {
            data.SetLevel(data._level + 1);//升级完成
        }
        
        data.SetStatus(BuildingData.BuildingStatus.NORMAL);
        this.SendNotification(NotiDefine.BuildingStatusChanged, key);
    }


}//end class
