using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using org.mariuszgromada.math.mxparser;
using SMVC.Patterns;
using UnityEngine;



//时间戳回调管理
public class SkillProxy : BaseRemoteProxy
{
    public static SkillProxy _instance;
    public SkillProxy() : base(ProxyNameDefine.SKILL)
    {
        _instance = this;
    }

    public string GetSkillTypeName(int id)
    {
        SkillConfig config = SkillConfig.Instance.GetData(id);
        string key = UtilTools.combine("SkillType", config.Type);
        return LanguageConfig.GetLanguage(key);
    }

    public string GetSkillReleasDesc(int id)
    {
        SkillConfig config = SkillConfig.Instance.GetData(id);
        string key = UtilTools.combine("Release", config.ReleaseTerm);
        return LanguageConfig.GetLanguage(key);
    }


    public double CalculateExpresstionValue(string expression, params object[] paramName)
    {
        if (string.IsNullOrEmpty(expression))
            return 0;

        var len = paramName.Length;
        int i = 0;
        while (i + 1 < len)
        {
            expression = expression.Replace(paramName[i].ToString(), paramName[i + 1].ToString());
            i += 2;
        }

        Expression ex = new Expression(expression);
        return ex.calculate();
    }

    public string GetExpressionValueString(string expression, params object[] paramName)
    {
        double value = this.CalculateExpresstionValue(expression, paramName);
        return value.ToString();
    }

    public Dictionary<int, SKillEffectResult> GetBattleSkillAttackEffect(int id, int level)
    {
        Dictionary<int, SKillEffectResult> effects = new Dictionary<int, SKillEffectResult>();
        SkillConfig config = SkillConfig.Instance.GetData(id);
        foreach (int effectid in config.EffectIDs)
        {
            SKillEffectResult result = new SKillEffectResult();
            SkillEffectConfig configEffect = SkillEffectConfig.Instance.GetData(effectid);
            result.Config = configEffect;
            result.Value = SkillProxy._instance.CalculateExpresstionValue(configEffect.Value, "$level", level);
            result.Rate = SkillProxy._instance.CalculateExpresstionValue(configEffect.Rate, "$level", level);
            result.ActiveRate = SkillProxy._instance.CalculateExpresstionValue(configEffect.Active_Rate, "$level", level);

            effects[effectid] = result;
        }
        return effects;
    }

    public bool IsRateSuccess(double rate)
    {
        int RandomRate = UtilTools.RangeInt(0, 99);
        return (RandomRate > rate);
    }


    public bool ComputeBattleSKillEffect(BattlePlayer actionPl, SKillEffectResult result,out double Value)
    {
        Value = 0;
        if (IsRateSuccess(result.Rate))
            return false;//失败了不触发

        if (result.Config.Type.Equals(SkillEffectType.Demage))
            Value = Mathf.RoundToInt(actionPl.Attributes[AttributeDefine.Attack] * (float)result.Value * 0.01f);
        else
            Value = result.Value;
        return true;
    }

   

    public List<VInt2> GetRangeCordinate(string rangeID,VInt2 StartPosition,VInt2 RolePostion = null)
    {
        List<VInt2> cordinates;
        RangeFunctionConfig config = RangeFunctionConfig.Instance.GetData(rangeID);
        if (config.Function.Equals(RangeTypeDefine.Point))
        {
            this.ComputePointCordinate(StartPosition, out cordinates);
        }
        else if (config.Function.Equals(RangeTypeDefine.Cross))
        {
            this.ComputeCrossCordinate(StartPosition, out cordinates, config.ComputeParams[0], config.ComputeParams[1]);
        }
        else if (config.Function.Equals(RangeTypeDefine.Matrix))
        {
            this.ComputeMatrixCordinate(StartPosition, out cordinates, config.ComputeParams[0], config.ComputeParams[1]);
        }
        else if (config.Function.Equals(RangeTypeDefine.Line))
        {
            this.ComputeLineCordinate(StartPosition, out cordinates, config.ComputeParams[0], RolePostion);
        }
        else
        {
            cordinates = new List<VInt2>();
            cordinates.Add(new VInt2(StartPosition.x, StartPosition.y));
        }

        return cordinates;
    }

    public void ComputeLineCordinate(VInt2 attackPos, out List<VInt2> cordinates, int Lenght,VInt2 RolePostion)
    {
        cordinates = new List<VInt2>();
       
        if (RolePostion == null || (RolePostion.x == attackPos.x && RolePostion.y == attackPos.y))
        {
            //写死向上
            for (int i = 0; i < Lenght; ++i)
            {
                cordinates.Add(new VInt2(attackPos.x, attackPos.y + i));
            }
            return;
        }

        int YOffset = attackPos.y - RolePostion.y;
        int XOffset = attackPos.x - RolePostion.x;
        int absY = Mathf.Abs(YOffset);
        int absX = Mathf.Abs(XOffset);
        if (absY > absX)
        {
            //向上x不变，y递增减
            for (int i = 0; i < Lenght; ++i)
            {
                int addValue = YOffset > 0 ? i : -i;
                cordinates.Add(new VInt2(attackPos.x, attackPos.y + addValue));
            }
        }
        else if (absX > absY)
        {
            //向右y不变，x递增减
            for (int i = 0; i < Lenght; ++i)
            {
                int addValue = XOffset > 0 ? i : -i;
                cordinates.Add(new VInt2(attackPos.x + addValue, attackPos.y));
            }
        }
        else if(absX == absY)
        {
            //对角线
            for (int i = 0; i < Lenght; ++i)
            {
                int addValueX = XOffset > 0 ? i : -i;
                int addValueY = YOffset > 0 ? i : -i;
                cordinates.Add(new VInt2(attackPos.x + addValueX, attackPos.y+ addValueY));
            }
        }
    }

    public void ComputeMatrixCordinate(VInt2 centerPos, out List<VInt2> cordinates, int halfX, int halfY)
    {
        cordinates = new List<VInt2>();
        int startX = centerPos.x - halfX;
        int endX = centerPos.x + halfX;
        int startZ = centerPos.y - halfY;
        int endZ = centerPos.y + halfY;
        for (int row = startX; row <= endX; ++row)
        {
            int corX = row;
            for (int col = startZ; col <= endZ; ++col)
            {
                int corZ = col;
                cordinates.Add(new VInt2(corX, corZ));
            }
        }
    }

    public void ComputeCrossCordinate(VInt2 centerPos, out List<VInt2> cordinates,int halfX,int halfY)
    {
        cordinates = new List<VInt2>();
        cordinates.Add(new VInt2(centerPos.x, centerPos.y));
        //X横向范围
        int startX = centerPos.x - halfX;
        int endX = centerPos.x + halfX;
        for (int row = startX; row <= endX; ++row)
        {
            if (row == centerPos.x)
                continue;
            cordinates.Add(new VInt2(row, centerPos.y));
        }

        //Y纵向范围
        int startY = centerPos.y - halfY;
        int endY = centerPos.y + halfY;
        for (int col = startY; col <= endY; ++col)
        {
            if (col == centerPos.y)
                continue;
            cordinates.Add(new VInt2(centerPos.x, col));
        }
    }

    public void ComputePointCordinate(VInt2 startPos, out List<VInt2> cordinates)
    {
        cordinates = new List<VInt2>();
        cordinates.Add(new VInt2(startPos.x, startPos.y));
    }


    public SkillLevelConfig GetSkillLvConfig(int id, int level)
    {
        int lvid = id * 100 + level;
        return SkillLevelConfig.Instance.GetData(lvid);
    }
}//end class
