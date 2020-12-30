using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class QuestionView : MonoBehaviour
{
    public UIClickHandler _click;
    public List<UIButton> _btnList;
    public UITexts _questionTxt;
    public Text _AnswerTxt;

    public Image _icon;

    public Text _CloseTxt;
    public int _orignWidth = 1500;
    private string _rightAnswer = "";
    private bool _isCorrect = false;
    private bool _isSelect = false;
    private int _heroID;
    void Start()
    {
        foreach (UIButton item in this._btnList)
        {
            item.Label.text = item.gameObject.name;
            item.AddEvent(OnSelectAnswer);
        }
        _click.AddListener(OnEndClick);
    }

    private void OnEndClick(object param)
    {
        if (this._isSelect)
        {
            MediatorUtil.HideMediator(MediatorDefine.QUESTION);
        }
    }

    private void AdjustQuestionBG()
    {
        StartCoroutine(WaitTextFrame());
    }

    IEnumerator WaitTextFrame()
    {
        yield return new WaitForFixedUpdate();
        Vector2 size = this._questionTxt.GetComponent<RectTransform>().sizeDelta;
        float width = this._questionTxt.FirstLabel.preferredWidth;
        if (width < 1400)
        {
            size.x = width + 50;
        }
        else
        {
            size.x = _orignWidth;
        }
        this._questionTxt.GetComponent<RectTransform>().sizeDelta = size;
        LayoutRebuilder.ForceRebuildLayoutImmediate(this._questionTxt.gameObject.GetComponent<RectTransform>());
    }

    private void OnSelectAnswer(UIButton btnSelf)
    {
        string answer = btnSelf.gameObject.name;
        _isCorrect = answer.Equals(this._rightAnswer);
        string str = _isCorrect ? LanguageConfig.GetLanguage(LanMainDefine.GoodAnswer) : LanguageConfig.GetLanguage(LanMainDefine.BadAnswer);
        this._questionTxt.FirstLabel.text = str;
        
        this._isSelect = true;
        _CloseTxt.gameObject.SetActive(true);
        _AnswerTxt.gameObject.SetActive(false);
        this.AdjustQuestionBG();
    }

    public bool Correct => this._isCorrect;

    public void SetRandomQuestion(int heroid)
    {
        _CloseTxt.gameObject.SetActive(false);
        _AnswerTxt.gameObject.SetActive(true);
        this._heroID = heroid;
        this._icon.sprite = ResourcesManager.Instance.GetHeroCardSprite(this._heroID);

        this._isSelect = false;
        this._isCorrect = false;
        _CloseTxt.gameObject.SetActive(false);
        Dictionary<int, QuestionConfig> dic = QuestionConfig.Instance.getDataArray();
        int count = dic.Count;
        int id = UtilTools.RangeInt(1, count + 1);
        QuestionConfig config = dic[id];
        this._questionTxt.FirstLabel.text = config.Question;
        this._AnswerTxt.text = config.Selection;
        this._rightAnswer = config.Answer;
        this.AdjustQuestionBG();

    }
}
