using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using SMVC.Patterns;
using UnityEngine;


public class PathProxy : BaseRemoteProxy
{
    private Dictionary<string, PathData> _paths = new Dictionary<string, PathData>();
 
    public static PathProxy _instance;
    public PathProxy() : base(ProxyNameDefine.PATH)
    {
        _instance = this;
    }

    public Dictionary<string, PathData> AllPaths => this._paths;

    public void AddPath(PathData path)
    {
        this._paths[path.ID] = path;
        this.SendNotification(NotiDefine.PathAddNoti, path);
    }

    public void RemovePath(string pathID)
    {
        this._paths.Remove(pathID);
        this.SendNotification(NotiDefine.PathRemoveNoti,pathID);
    }
}//end class
