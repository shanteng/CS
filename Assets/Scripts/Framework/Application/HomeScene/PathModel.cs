using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
public class PathModel : MonoBehaviour
{
    public float YOffset = 0.51f;
    public Transform _modelRoot;
    public CountDownText _cdTxt;
    public Transform _Flag;

    public Transform _PatrolRoot;
    public GameObject _patrolSpotPrefab;


    private LineRenderer _pathLine;
    private LineRenderer _pathPassLine;
    private GameObject _model;

    private Vector3 _startPos;
    private Vector3 _endPos;
    private Vector3 _dir;

    private double _curTime;
    private long _startTime;
    private double _timeRealFromStartUp;
    private long _totleSces;
    private bool _isStart = false;
    private Transform _camTran;
  

    private PathData _data;
    private void Awake()
    {

    }

    public void DoPath(PathData data)
    {
        this._data = data;
        this._startPos = new Vector3(_data.Start.x, YOffset, _data.Start.y);
        this._endPos = new Vector3(_data.Target.x, YOffset, _data.Target.y);
        this._dir = (this._endPos - this._startPos).normalized;

        this._Flag.position = _endPos;
        this.transform.position = this._startPos;

        this._PatrolRoot.gameObject.SetActive(data.Type == PathData.TYPE_PATROL);
        if (data.Type == PathData.TYPE_PATROL)
        {
            PatrolData patrolData = (PatrolData)data.Param;
            int halfRange = patrolData.Range;

            for (int row = -halfRange; row <= halfRange; ++row)
            {
                int corX = _data.Target.x + row;
                for (int col = -halfRange; col <= halfRange; ++col)
                {
                    int corZ = _data.Target.y +col;
                    bool isVisible = WorldProxy._instance.IsSpotVisible(corX, corZ);
                    if (isVisible == false)
                    {
                        //创建一个高亮格子
                        GameObject spot = GameObject.Instantiate(this._patrolSpotPrefab, Vector3.zero, Quaternion.identity, this._PatrolRoot);
                        spot.transform.localPosition = new Vector3(corX, 0, corZ);
                        spot.transform.localScale = Vector3.one;
                        spot.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));
                    }
                }
            }//end for
        }

        GameObject prefab = ResourcesManager.Instance.LoadModel(data.Model);
        this._model = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity, this._modelRoot);
        this._model.transform.localPosition = new Vector3(0, 0, 0);
        this._model.transform.localScale = Vector3.one;
        this._model.transform.localRotation = Quaternion.Euler(Vector3.zero);

        //pathLine
        prefab = ResourcesManager.Instance.LoadLandRes("PathLine");
        GameObject obj = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity, this.transform);
        _pathLine = obj.GetComponent<LineRenderer>();

        prefab = ResourcesManager.Instance.LoadLandRes("PathPassLine");
        obj = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity, this.transform);
        _pathPassLine = obj.GetComponent<LineRenderer>();
        //-end pathline

        this._totleSces = (data.ExpireTime - data.StartTime);
        this._startTime = GameIndex.ServerTime;
        this._timeRealFromStartUp = Time.realtimeSinceStartup;

        Vector3 lookAt = new Vector3(_endPos.x, this._modelRoot.position.y, _endPos.z);
        this._modelRoot.LookAt(lookAt);

        this._camTran = Camera.main.transform;

        this._cdTxt.DoCountDown(this._data.ExpireTime);
        Vector3 targetPos = this._cdTxt.transform.position + _camTran.rotation * Vector3.forward;
        Vector3 targetOrientation = _camTran.rotation * Vector3.up;
        this._cdTxt.transform.LookAt(targetPos, targetOrientation);
        this._isStart = true;
    }

    private void Update()
    {
        if (this._isStart)
        {
            double pass = (double)Time.realtimeSinceStartup - this._timeRealFromStartUp;
            this._curTime = this._startTime + pass;
            double lerpValue = (this._curTime - this._data.StartTime) / (double)this._totleSces;
            Vector3 posLerp = Vector3.Lerp(this._startPos, this._endPos, (float)lerpValue);
            this._modelRoot.position = posLerp;

            Vector3 curPos = this._modelRoot.position;
            curPos.y = YOffset;
            this._pathLine.SetPositions(new Vector3[] { curPos, this._endPos });
            this._pathPassLine.SetPositions(new Vector3[] { curPos, this._startPos });
        }
    }
}
