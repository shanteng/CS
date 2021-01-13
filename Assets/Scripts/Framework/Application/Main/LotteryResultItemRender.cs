using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class LotteryResultItemData : ScrollData
{
    public LotteryRoundConfig _config;
    public List<LotteryResultPlayer> players;
    public RectTransform _rect;
   
    
    public LotteryResultItemData(LotteryRoundConfig index, List<LotteryResultPlayer> pls)
    {
        this._config = index;
        this.players = pls;
        this._Key = "Log";
    }
}

public class LotteryResultItemRender : ItemRender
{
    public Text Title;
    public Text Award;
    public List<UITexts> _PlayerUIs;
    public RectTransform _rect;
  

    private void Start()
    {
      
    }

 
    protected override void setDataInner(ScrollData dataScroll)
    {
        LotteryResultItemData data = (LotteryResultItemData)dataScroll;
        this.Title.text = UtilTools.combine(data._config.ID,"、", data._config.Title,"("+data._config.Count,"人)");
        this.Award.text = data._config.Name;
        int count = data.players.Count;
        int uicount = this._PlayerUIs.Count;
        for (int i = 0; i < uicount; ++i)
        {
            if (i >= count)
            {
                this._PlayerUIs[i].gameObject.SetActive(false);
                continue;
            }
          
            string nameStr = data.players[i].Name;
            int index = nameStr.IndexOf('(');
            string english = nameStr.Substring(0, index);
            Sprite sp = ResourcesManager.GetPlayerPicture(english);
            this._PlayerUIs[i]._icon.sprite = sp;

            this._PlayerUIs[i]._icon.gameObject.SetActive(sp != null);
            this._PlayerUIs[i]._texts[1].gameObject.SetActive(sp == null);


            this._PlayerUIs[i].FirstLabel.text = english;
            this._PlayerUIs[i].gameObject.SetActive(true);
        }//end for

        LayoutRebuilder.ForceRebuildLayoutImmediate(this._rect);
        LayoutRebuilder.ForceRebuildLayoutImmediate(this.GetComponent<RectTransform>());
    }

}


