﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITexts : UIBase
{
    public List<Text> _texts;

    public Text FirstLabel => this._texts[0];

}
