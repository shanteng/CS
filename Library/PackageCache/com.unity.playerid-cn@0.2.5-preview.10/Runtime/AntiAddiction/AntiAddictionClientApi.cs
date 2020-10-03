using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Text;
using System.Collections.Generic;

namespace UnityEngine.PlayerIdentity
{
    public class AntiAddictionClientApi : MonoBehaviour
    {
        public Action<string, string, string> OnMessage;
        public Action<string, string> OnKickOff;
        public Action<int> OnJudgePay;
        public Action<int> OnJudgeTime;
        
        private const string serverUrl = "https://gai-api.unity.cn";
        private const string contentType = "application/json";
        private AntiAddictionDevice device = new AntiAddictionDevice();
        private string userId = "";
        
        [SerializeField]
        private string configId=default;

        private void Start()
        {
            ((PlayerIdentityCore)PlayerIdentityManager.Current).onLogin.AddListener(JudgeLogin);
        }

        private void Update()
        {
            if (PlayerIdentityManager.Current.userInfo!=null)
            {
                if (PlayerIdentityManager.Current.userInfo.userId != userId)
                    userId = PlayerIdentityManager.Current.userInfo.userId;
            }
        }

        internal void JudgeLogin()
        {
            if (PlayerIdentityManager.Current.loginStatus != LoginStatus.LoggedIn)
            {
                Debug.LogError("用户尚未登录");
                return;
            }
            string extraCommand = "/api/judge/login";
            AntiAddictionJudgeLoginInput judgeLoginInput = new AntiAddictionJudgeLoginInput();
            AntiAddictionAuthInfo authInfo = new AntiAddictionAuthInfo();
            authInfo.authUserId = PlayerIdentityManager.Current.userInfo.userId;
            authInfo.authKey = "";
            authInfo.authUserType = 1102;
            judgeLoginInput.authInfo = authInfo;
            judgeLoginInput.device = device;
            judgeLoginInput.id = configId;
            judgeLoginInput.context = "";
            judgeLoginInput.userId = authInfo.authUserId;
            string dataJson = Utils.DeJson.Serialize.From(judgeLoginInput);
            SendCommand(extraCommand, 1, dataJson);
        }

        private void SendHeartbeat(int duration = 300, int factType = 10, string context = "")
        {
            string extraCommand = "/api/judge/timing";
            AntiAddictionJudgeTimingInput judgeTimingInput = new AntiAddictionJudgeTimingInput();
            judgeTimingInput.id = configId;
            judgeTimingInput.context = context;
            judgeTimingInput.factType = factType;
            judgeTimingInput.userId = userId;
            judgeTimingInput.device = device;
            judgeTimingInput.duration = duration;
            string dataJson = Utils.DeJson.Serialize.From(judgeTimingInput);
            SendCommand(extraCommand, 2, dataJson);
        }

        public void JudgePay(string context = "")
        {
            string extraCommand = "/api/judge/pay";
            AntiAddictionJudgePayInput judgePayInput = new AntiAddictionJudgePayInput();
            judgePayInput.userId = userId;
            judgePayInput.id = configId;
            judgePayInput.context = context;
            AntiAddictionAuthInfo authInfo = new AntiAddictionAuthInfo();
            authInfo.authUserId = userId;
            authInfo.authKey = "";
            authInfo.authUserType = 1102;
            judgePayInput.authInfo = authInfo;
            string dataJson = Utils.DeJson.Serialize.From(judgePayInput);
            SendCommand(extraCommand, 3, dataJson);
        }

        public void ReportExecution(string traceId, string ruleName, long execTime, string context = "")
        {
            string extraCommand = "/api/report/execution";
            AntiAddictionReportExecutionInput reportExecutionInput = new AntiAddictionReportExecutionInput();
            reportExecutionInput.ruleName = ruleName;
            reportExecutionInput.userId = userId;
            reportExecutionInput.traceId = traceId;
            reportExecutionInput.execTime = execTime;
            reportExecutionInput.id = configId;
            reportExecutionInput.context = context;
            string dataJson = Utils.DeJson.Serialize.From(reportExecutionInput);
            SendCommand(extraCommand, 4, dataJson);
        }

        internal void StartHeartbeat(string context = null, int duration = 300)
        {
            SendHeartbeat(duration, 11, context);
        }
        public void ContinueHeartbeat(string context=null,int duration = 300)
        {
            SendHeartbeat(duration, 12, context);
        }

        public void StopHeartbeat(string context = null)
        {
            SendHeartbeat(100, 13, context);
        }

        private void SendCommand(string cmd, int type, string data)
        {
            StartCoroutine(Post(cmd, data, type));
        }

        IEnumerator Post(string cmd, string data, int type)
        {
            string result;
            byte[] body = Encoding.UTF8.GetBytes(data);
            UnityWebRequest request = new UnityWebRequest(serverUrl + cmd, "POST");
            request.uploadHandler = new UploadHandlerRaw(body);
            request.SetRequestHeader("Content-Type", "application/json;charset=utf-8");
            request.SetRequestHeader("Authorization", "Bearer " + PlayerIdentityManager.Current.accessToken);
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return request.SendWebRequest();
            if (request.isNetworkError)
            {
                if (OnJudgeActionRigistered(OnMessage,"OnMessage",3))
                    OnMessage("AntiAddictionNetworkError", request.error, "");
            } 
            else if (request.isHttpError)
            {
                if (OnJudgeActionRigistered(OnMessage,"OnMessage",3))
                    OnMessage("AntiAddictionServerError", request.error, "");
            }
            else
            {
                result = request.downloadHandler.text;
                DealWithResult(type,result);
            }
        }

        private void DealWithResult(int type, string result)
        {
            
            if (result.IndexOf("errorCode")>= 0)
            {
                AntiAddictionError antiAddictionError = Utils.DeJson.Deserialize.To<AntiAddictionError>(result);
                if (OnJudgeActionRigistered(OnMessage, "OnMessage", 3))
                    OnMessage("AntiAddictionServerError", "ErrorMessage:" + antiAddictionError.message + ";" + "Target:" + antiAddictionError.target, "");
            }
            else
            {
                switch (type)
                {
                    case (1):
                        AntiAddictionJudgeLoginReturn judgeLoginReturn = new AntiAddictionJudgeLoginReturn();
                        try
                        {
                            judgeLoginReturn = Utils.DeJson.Deserialize.To<AntiAddictionJudgeLoginReturn>(result);
                        }
                        catch (UnityException e)
                        {
                            Debug.Log(e);
                        }
                        finally
                        {
                            if (judgeLoginReturn.ret != 0)
                            {
                                if (OnJudgeActionRigistered(OnMessage, "OnMessage", 3))
                                    OnMessage("AntiAddictionServerError", "ErrorCode:" + judgeLoginReturn.ret.ToString(), judgeLoginReturn.context);
                            }   
                            else if (judgeLoginReturn.instructions != null)
                            {
                                for (int i = 0; i < judgeLoginReturn.instructions.Count; i++)
                                {
                                    DealWithInstruction(judgeLoginReturn.instructions[i], judgeLoginReturn.traceId, judgeLoginReturn.context);
                                }
                            }
                        }

                        break;
                    case (2):
                        AntiAddictionJudgeTimingReturn judgeTimingReturn = new AntiAddictionJudgeTimingReturn();
                        try
                        {
                            judgeTimingReturn = Utils.DeJson.Deserialize.To<AntiAddictionJudgeTimingReturn>(result);
                        }
                        catch (Exception e)
                        {
                            Debug.Log(e);
                        }
                        finally
                        {
                            if (judgeTimingReturn.ret != 0)
                            {
                                if (OnJudgeActionRigistered(OnMessage, "OnMessage", 3))
                                    OnMessage("AntiAddictionServerError", "ErrorCode:"+judgeTimingReturn.ret.ToString(), judgeTimingReturn.context);
                            } 
                            else
                            {
                                if (OnJudgeActionRigistered(OnJudgeTime, "OnJudgeTime", 1))
                                    OnJudgeTime(judgeTimingReturn.duration);
                                if (judgeTimingReturn.instructions != null)
                                {
                                    for (int i = 0; i < judgeTimingReturn.instructions.Count; i++)
                                    {
                                        DealWithInstruction(judgeTimingReturn.instructions[i], judgeTimingReturn.traceId, judgeTimingReturn.context);
                                    }
                                }
                            }
                        }

                        break;
                    case (3):
                        AntiAddictionJudgePayReturn judgePayReturn = new AntiAddictionJudgePayReturn();
                        try
                        {
                            judgePayReturn = Utils.DeJson.Deserialize.To<AntiAddictionJudgePayReturn>(result);
                        }
                        catch (Exception e)
                        {
                            Debug.Log(e);
                        }
                        finally
                        {
                            if (judgePayReturn.ret != 0)
                            {
                                if (OnJudgeActionRigistered(OnMessage, "OnMessage", 3))
                                    OnMessage("AntiAddictionServerError", "ErrorCode:" + judgePayReturn.ret.ToString(), "");
                            }
                            if (OnJudgeActionRigistered(OnJudgePay, "OnJudgePay", 3))
                                OnJudgePay(((judgePayReturn == null) || (judgePayReturn.certInfo.age < 0)) ? 18 : judgePayReturn.certInfo.age);
                        }

                        break;
                    case (4):
                        AntiAddictionReportExecutionReturn reportExecutionReturn = new AntiAddictionReportExecutionReturn();
                        try
                        {
                            reportExecutionReturn = Utils.DeJson.Deserialize.To<AntiAddictionReportExecutionReturn>(result);
                        }
                        catch (Exception e)
                        {
                            Debug.Log(e);
                        }
                        finally
                        {
                            if (reportExecutionReturn.ret != 0)
                            {
                                if (OnJudgeActionRigistered(OnMessage, "OnMessage", 3))
                                    OnMessage("AntiAddictionServerError", "ErrorCode:" + reportExecutionReturn.ret.ToString(), reportExecutionReturn.context);
                            }
                            else
                            {
                                if (OnJudgeActionRigistered(OnMessage, "OnMessage", 3))
                                    OnMessage("ReportExecution", reportExecutionReturn.msg, reportExecutionReturn.context);
                            }   
                        }
                        break;
                }
            }
        }

        private void DealWithInstruction(AntiAddictionInstruction instruction,string traceId,string context)
        {
            switch (instruction.type)
            {
                case (1):
                    if (OnJudgeActionRigistered(OnMessage, "OnMessage", 3))
                        OnMessage(instruction.title, instruction.msg, context);
                    break;
                case (2):
                    if (OnJudgeActionRigistered(OnMessage, "OnMessage", 3))
                        OnMessage(instruction.title, instruction.msg, context);
                    if (OnJudgeActionRigistered(OnKickOff, "OnKickOff", 3))
                        OnKickOff(traceId,instruction.ruleName);
                    break;
                case (3):
                    if (OnJudgeActionRigistered(OnMessage, "OnMessage", 3))
                        OnMessage(instruction.title, instruction.msg, context);
                    Application.OpenURL(instruction.url);
                    if (OnJudgeActionRigistered(OnKickOff, "OnKickOff", 3))
                        OnKickOff(traceId, instruction.ruleName);
                    break;
                default:
                    break;
            }
        }

        private bool OnJudgeActionRigistered(object action, string actionName, int logType)
        {
            if (action == null)
            {
                switch (logType)
                {
                    case (1):
                        Debug.Log("Action " + actionName + " is null.Please rigister " + actionName + " first.");
                        break;
                    case (2):
                        Debug.LogWarning("Action " + actionName + " is null.Please rigister " + actionName + " first.");
                        break;
                    case (3):
                        Debug.LogError("Action " + actionName + " is null.Please rigister " + actionName + " first.");
                        break;
                }
                return false;
            }
            return true;

        }
    }

    internal class AntiAddictionAuthInfo
    {
        public int authUserType; //鉴权类型AuthType
        public string authAppid; //鉴权平台中该应用的appid
        public string authUserId; //鉴权平台中该用户的id
        public string authKey; //鉴权平台中该用户的登录凭证,例如 skey 或 access token
        public string authExt; //用户的额外的登录凭证, 一般不需要填写
    }

    internal class AntiAddictionFcmZoneInfo
    {
        public int area; //游戏大区信息
        public int platid; //游戏平台信息
        public int partition; //游戏小区信息
        public string characterid; //角色id
    }

    internal class AntiAddictionDevice
    {
        public string outerIp; //客户端的外网ip地址。优先获取ipv4地址， 其次获取ipv6地址。 不要填写内网地址。此字段的值会用于判断是否海外用户，因涉及海外用户不需要实名
        public string osSystem; //操作系统，ios android windows mac linux等,全小写
        public string language; //设备语言，支持 zh_CN，en_US
        public int scene;//场景；视频青少年模式场景:257
        public string deviceId; //设备的唯一标识，只能包含数字、字母、- _符号
        public string deviceModal; //设备型号， 如 switch， iphone，huawei等
        public string deviceVersion; //设备版本， 如9.1.1
        public string deviceManufacturer; //设备的制造商， 如Apple， Nintendo
        public AntiAddictionFcmZoneInfo fcmZoneInfo; //游戏自定义的大小区信息
    }

    internal class AntiAddictionInstruction
    {
        public int type; //指令类型
        public string title; //标题
        public string msg; //内容
        public string url; //打开的网页地址
        public int modal;//1表示模态窗口，关闭则会退出登录；默认为0，允许关闭
        public string data; //	json格式
        public double ratio;//变化比例，只有type=5或6时才有效
        public string ruleName; //命中的规则名称,仅透传，用于执行上报接口
    }

    internal enum AntiAddictionInstructionType
    {
        InstructionType_Undefined = 0, //未定义
        InstructionType_Tips = 1, //弹提示
        InstructionType_Logout = 2, //强制下线
        InstructionType_OpenUrl = 3, //打开网页窗口
        InstructionType_UserDefined = 4, //用户自定义，data传json内容
        InstructionType_Income = 5, //收益，不弹提示， data和ratio中传入比例
        InstructionType_Income_Tips = 6, //收益且弹提示， data和ratio中传入比例
        InstructionType_Stop = 7 //停止操作
    }

    internal class AntiAddictionPayInfo
    {
        public string userId; //获得充值收益的帐号
        public int payAmount; //充值的金额, 精确到最小货币单位.如人民币, 则为1分.(不是游戏内的货币!!)
        public long payTimestamp; //充值的时间戳
        public string payOrderId; //充值的唯一订单号
        public string payChannel; //充值渠道
    }

    internal class AntiAddictionCertInfo
    {
        public int adultType;//成年状态，0-未知 1-未成年 2- 已成年
        public int age;
        public string hopeToken;
        public int isRealName;//0-未实名认证 1-已实名认证
    }


    internal class AntiAddictionJudgeLoginInput
    {
        public string id;
        public string context;
        public string clientId;
        public string userId;
        public AntiAddictionAuthInfo authInfo;
        public AntiAddictionDevice device;
    }

    internal class AntiAddictionJudgeLoginReturn
    {
        public int ret;
        public string msg;
        public string context;
        public string traceId;
        public List<AntiAddictionInstruction> instructions;
    }

    internal class AntiAddictionJudgeTimingInput
    {
        public string id;
        public string context;
        public int factType;
        public string userId;
        public int duration;
        public AntiAddictionDevice device;
    }

    internal class AntiAddictionJudgeTimingReturn
    {
        public int ret;
        public string context;
        public string msg;
        public string traceId;
        public List<AntiAddictionInstruction> instructions;
        public int duration;
    }

    internal class AntiAddictionJudgePayInput
    {
        public string userId;
        public string context;
        public string id;
        public AntiAddictionAuthInfo authInfo;
    }

    internal class AntiAddictionJudgePayReturn
    {
        public int ret;
        public string msg;
        public string traceId;
        public AntiAddictionCertInfo certInfo;
    }

    internal class AntiAddictionReportExecutionInput
    {
        public string id;
        public string context;
        public string ruleName;
        public string userId;
        public string traceId;
        public long execTime;
    }

    internal class AntiAddictionReportExecutionReturn
    {
        public int ret;
        public string context;
        public string msg;
    }

    internal class AntiAddictionReportPayInput
    {
        public string id;
        public string context;
        public AntiAddictionPayInfo payInfo;
    }

    internal class AntiAddictionReportPayReturn
    {
        public int ret;
        public string context;
        public string msg;
        public string traceId;
    }

    internal class AntiAddictionError
    {
        public string errorCode;
        public string message;
        public string target;
    }
}
