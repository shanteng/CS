using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BuildCenterView : MonoBehaviour
    , IScollItemClickListener
{
    public List<UIToggle> _toggleList;
    public DataGrid _hGrid;
    private BuildingType _type = BuildingType.Economy;
    void Awake()
    {
        foreach (UIToggle item in this._toggleList)
        {
            item._param._value = UtilTools.ParseInt(item.gameObject.name);
            item.AddEvent(OnSelectToggle);
        }
    }

    private void OnSelectToggle(UIToggle btnSelf)
    {
        this._type = (BuildingType)btnSelf._param._value;
        this.SetList();
    }

    public void InitData()
    {
        _type = BuildingType.Economy;
        this.SetList();
    }

    private void SetList()
    {
        foreach (UIToggle toggle in this._toggleList)
        {
            toggle.IsOn = (this._type == (BuildingType)toggle._param._value);
        }

        Dictionary<int, BuildingConfig> dic =  BuildingConfig.Instance.getDataArray();
        _hGrid.Data.Clear();

        foreach (BuildingConfig config in dic.Values)
        {
            if (config.Condition == null || config.Condition.Length == 0 || config.Type != (int)this._type)
                continue;
            BuidItemData data = new BuidItemData(config);
            this._hGrid.Data.Add(data);
        }
        _hGrid.ShowGrid(this);
    }

    public void onClickScrollItem(ScrollData data)
    {
        if (data._Key.Equals("BuidItemRender"))
        {
            BuidItemData curData = (BuidItemData)data;
            VInt2 kv = WorldProxy._instance.GetBuildingMaxAndLimitCount(curData._config.ID);
            int count = WorldProxy._instance.GetBuildingCount(curData._config.ID);
            int max = kv.x;
            int limit = kv.y;
            if (count >= limit)
            {
                PopupFactory.Instance.ShowNotice(LanguageConfig.GetLanguage(LanMainDefine.LimitReach));
                return;
            }

            if (count >= max && count == 0)
            {
                string[] firstlist = curData._config.Condition[0].Split('|');
                int id = UtilTools.ParseInt(firstlist[0]);
                int level = UtilTools.ParseInt(firstlist[1]);
                int bdCount = UtilTools.ParseInt(firstlist[2]);
                BuildingConfig configNeed = BuildingConfig.Instance.GetData(id);
                string notice =  LanguageConfig.GetLanguage(LanMainDefine.BuildOpenCondition, configNeed.Name, level);
                PopupFactory.Instance.ShowNotice(notice);
                return;
            }

            if (count >= max && count > 0)
            {
                VInt2 needKv = WorldProxy._instance.GetBuildingNextOpenCondition(curData._config.ID);
                BuildingConfig configNeed = BuildingConfig.Instance.GetData(needKv.x);
                string notice = LanguageConfig.GetLanguage(LanMainDefine.BuildNextCondition, configNeed.Name, needKv.y);
                PopupFactory.Instance.ShowNotice(notice);
                return;
            }

            BuildingUpgradeConfig configLv = BuildingUpgradeConfig.GetConfig(curData._config.ID, 1);
            bool isCostEnough = RoleProxy._instance.IsDedutStisfy(configLv.Cost);
            if (isCostEnough == false)
            {
                return;
            }

            MediatorUtil.SendNotification(NotiDefine.TryBuildBuilding, curData._config.ID);
            MediatorUtil.HideMediator(MediatorDefine.BUILD_CENTER);
        }
    }

}
