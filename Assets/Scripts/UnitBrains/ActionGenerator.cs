using Model;
using Model.Runtime.ReadOnly;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

public class ActionGenerator
{
    private static ActionGenerator _actionGenerator;
    private IReadOnlyRuntimeModel _runtimeModel;
    private TimeUtil _timeUtil;
    private IReadOnlyUnit RecommendedPlayerTarget;
    private Vector2Int RecommendedPlayerStep;
    private IReadOnlyUnit RecommendedEnemyTarget;
    private Vector2Int RecommendedEnemyStep;
    private int _middleMapX;

    private ActionGenerator()
    {
        _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();
        _timeUtil = ServiceLocator.Get<TimeUtil>();
        _timeUtil.AddFixedUpdateAction(CalcRecomendedTarget);
        _timeUtil.AddFixedUpdateAction(CalcRecomendedStep);
        _middleMapX = _runtimeModel.RoMap.Width / 2;
    }

    public static ActionGenerator GetInstance()
    {
        if (_actionGenerator == null)
            _actionGenerator = new ActionGenerator();
        return _actionGenerator; 
    }

    public IReadOnlyUnit GetRecomendedTarget(bool IsPlayerUnitBrain)
    {
        return IsPlayerUnitBrain ? RecommendedPlayerTarget : RecommendedEnemyTarget;
    }

    public Vector2Int GetRecomendedStep(bool IsPlayerUnitBrain)
    {
        return IsPlayerUnitBrain ? RecommendedPlayerStep : RecommendedEnemyStep;
    }

    private void CalcRecomendedTarget(float fixedDeltaTime)
    {
        RecommendedPlayerTarget = CalcTarget(_runtimeModel.RoMap.Bases[RuntimeModel.PlayerId], _runtimeModel.RoBotUnits);
        RecommendedEnemyTarget = CalcTarget(_runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId], _runtimeModel.RoPlayerUnits);
    }

    private void CalcRecomendedStep(float fixedDeltaTime)
    {
        RecommendedPlayerStep = CalcStep(_runtimeModel.RoMap.Bases[RuntimeModel.PlayerId], _runtimeModel.RoBotUnits, _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId]);
        RecommendedPlayerStep = CalcStep(_runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId], _runtimeModel.RoPlayerUnits, _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId]);
    }

    private Vector2Int CalcStep(Vector2Int defenseBase, IEnumerable<IReadOnlyUnit> units, Vector2Int attackBase)
    {
        IReadOnlyUnit nearBaseEnemy = null;
        foreach (var unit in units)
        {
            if (defenseBase.x > _middleMapX && unit.Pos.x > _middleMapX)
            {
                return defenseBase + Vector2Int.right;
            }
            if (defenseBase.x < _middleMapX && unit.Pos.x < _middleMapX)
            {
                return defenseBase + Vector2Int.left;
            }
            if (nearBaseEnemy == null || Vector2Int.Distance(nearBaseEnemy.Pos, defenseBase) > Vector2Int.Distance(unit.Pos, defenseBase))
            {
                nearBaseEnemy = unit;
            }
        }
        return nearBaseEnemy == null ? attackBase : nearBaseEnemy.Pos;

    }

    private IReadOnlyUnit CalcTarget(Vector2Int defenseBase, IEnumerable<IReadOnlyUnit> units)
    {
        IReadOnlyUnit lowHealthEnemy = null;
        IReadOnlyUnit nearBaseEnemy = null;
        foreach (var unit in units)
        {
            if (lowHealthEnemy == null || lowHealthEnemy.Health > unit.Health)
            {
                lowHealthEnemy = unit;
            }
            if (defenseBase.x > _middleMapX && unit.Pos.x > _middleMapX || defenseBase.x < _middleMapX && unit.Pos.x < _middleMapX)
            {
                if (nearBaseEnemy == null)
                {
                    nearBaseEnemy = unit;
                }
                else
                {
                    nearBaseEnemy = Vector2Int.Distance(nearBaseEnemy.Pos, defenseBase) > Vector2Int.Distance(unit.Pos, defenseBase) ? unit : nearBaseEnemy;
                }
            }
        }
        return nearBaseEnemy == null ? lowHealthEnemy : nearBaseEnemy;
    }
    
}
