using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreateView : MonoBehaviour
{
    public List<CreateRoleItem> _toggleList;
    public InputField _nameTxt;
    public UIButton _btnStart;
    public SkillRangeUi _ui;
    private int _id;

    void Awake()
    {
        foreach (CreateRoleItem item in this._toggleList)
        {
            item.AddEvent(OnSelectToggle);
            item.SetData();
        }
        this._btnStart.AddEvent(this.OnClickStart);

        if (this._ui != null && this._ui.gameObject.activeSelf)
        {
            this._nameTxt.characterLimit = 100;
        }
    }

    public void InitData()
    {
        this.SetData(1);
    }
    private void OnClickStart(UIButton btn)
    {
        if (this._ui != null && this._ui.gameObject.activeSelf)
        {
            this._ui.SetData(this._nameTxt.text);
            return;
        }
        Dictionary<string, object> vo = new Dictionary<string, object>();
        vo["name"] = this._nameTxt.text;
        vo["head"] = (int)this._id;
        MediatorUtil.SendNotification(NotiDefine.CreateRoleDo, vo);
    }

    private void OnSelectToggle(UIToggle btnSelf)
    {
        CreateRoleItem item = (CreateRoleItem)btnSelf;
        this.SetData((int)item.ID);
    }


    private void SetData(int id)
    {
        foreach (CreateRoleItem toggle in this._toggleList)
        {
            toggle.IsOn = id == toggle.ID;
        }
        this._id = id;  
    }
}
