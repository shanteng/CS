using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UIMeshTexts : UIBase
{
    public List<TextMeshProUGUI> _texts;

    public TextMeshProUGUI FirstLabel => this._texts[0];

}
