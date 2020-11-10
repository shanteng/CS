using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using AnimationState = Spine.AnimationState;


public class SpineUiPlayer : MonoBehaviour
    ,IPointerClickHandler
{
    public static string STATE_IDLE = "idle";
    public static string STATE_ATTACK = "shifa";
    public static string STATE_WALK = "walk";

    private AnimationState _aniState;
    private SkeletonGraphic Player;

    public UnityAction<string> CallBackFun;
    public UnityAction ClickFun;
    private string _currentState = "idle";
    void Awake()
    {
        this.Player = this.gameObject.GetComponent<SkeletonGraphic>();
    }
    //public SUiEvent.NoParamEvent OnClick;

    private void Init()
    {
        if (null == _aniState)
        {
            if (Player)
            {
                Player.raycastTarget = true;
                _aniState = Player.AnimationState;
                if (null != _aniState)
                {
                    _aniState.Complete += OnAnimationEnd;
                }
                Player.raycastTarget = true;
            }
        }
    }

    void Start()
    {
        Init();
    }

    public void AddEvent(UnityAction<string> func,UnityAction click)
    {
        this.CallBackFun = func;
        this.ClickFun = click;
    }

    public void Play(string behaviour, bool loop)
    {
        Init();
        this._currentState = behaviour;
        _aniState?.SetAnimation(0, behaviour, loop);
    }

    private void OnAnimationEnd(TrackEntry trackEntry)
    {
        if (this.CallBackFun != null)
            CallBackFun.Invoke(this._currentState);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (this.ClickFun != null)
            this.ClickFun.Invoke();
    }

}
