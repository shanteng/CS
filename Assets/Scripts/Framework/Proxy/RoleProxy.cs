using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using SMVC.Patterns;
using UnityEngine;

public class RoleInfo
{
    public string UID;
    public string Name;
    public int Level;//
    public int Exp;//
    public List<CostData> ItemList = new List<CostData>();//属性道具
    public List<CostData> HourOutComes = new List<CostData>();//每小时产出值
}

public enum ItemKey
{
    Gold,
    Food,
    Stone,
    Wood,
    Metal,
    Count,
};

//时间戳回调管理
public class RoleProxy : BaseRemoteProxy
{
    public static Dictionary<ItemKey, string> ItemNameDic = new Dictionary<ItemKey, string>();
    private RoleInfo _role;
 
    public RoleProxy() : base(ProxyNameDefine.ROLE)
    {
        
    }

    public static string GetItemName(ItemKey key)
    {
        string name = "";
        if (ItemNameDic.TryGetValue(key, out name))
            return name;
        return "";
    }

    public void LoadOrGenerateRole()
    {
        string json = CloudDataTool.LoadFile(SaveFileDefine.Role);
        if (json.Equals(string.Empty) == false)
        {
            this._role = Newtonsoft.Json.JsonConvert.DeserializeObject<RoleInfo>(json);
        }
        else
        {
            this._role = new RoleInfo();
            this._role.UID = UIRoot.Intance._sdkCore.userId;
            this._role.Name = UIRoot.Intance._sdkCore.displayName;
            this._role.Level = 1;
            this._role.Exp = 0;
            ConstConfig config = ConstConfig.Instance.GetData(ConstDefine.InitRes);
            int count = config.StringValues.Length;
            for (int i = 0; i < count; ++i)
            {
                CostData data = new CostData();
                data.Init(config.StringValues[i]);
                this._role.ItemList.Add(data);
            }

            config = ConstConfig.Instance.GetData(ConstDefine.InitOutCome);
            count = config.StringValues.Length;
            for (int i = 0; i < count; ++i)
            {
                CostData data = new CostData();
                data.Init(config.StringValues[i]);
                this._role.HourOutComes.Add(data);
            }
            this.DoSaveRole(_role);
        }

        this._role.UID = UIRoot.Intance._sdkCore.userId;
        this._role.Name = UIRoot.Intance._sdkCore.displayName;
        this.SendNotification(NotiDefine.DoLoadScene, SceneDefine.Home);
    }//end func

    public void DoSaveRole(RoleInfo script)
    {
        //存储
        CloudDataTool.SaveFile(SaveFileDefine.Role, script);
    }


}//end class
