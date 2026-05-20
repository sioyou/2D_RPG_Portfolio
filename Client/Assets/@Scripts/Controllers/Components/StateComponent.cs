using System;
using Protocol;
using UnityEngine;

/// <summary>
/// CreatureState 비트 플래그 관리. stateFlags는 Move(1) | Attack(2) | Skill(4) 조합.
/// </summary>
public class StateComponent : MonoBehaviour
{
    [SerializeField]
    int _stateFlags;

    public int StateFlags
    {
        get => _stateFlags;
        set
        {
            if (_stateFlags == value)
                return;

            _stateFlags = value;
            OnStateFlagsChanged?.Invoke(_stateFlags);
        }
    }

    public event Action<int> OnStateFlagsChanged;

    public void ApplyNetworkFlags(int stateFlags)
    {
        StateFlags = stateFlags;
    }

    public bool HasState(CreatureState state)
    {
        return CreatureStateUtil.HasFlag(_stateFlags, state);
    }

    public void AddState(CreatureState state)
    {
        StateFlags = CreatureStateUtil.AddFlag(_stateFlags, state);
    }

    public void RemoveState(CreatureState state)
    {
        StateFlags = CreatureStateUtil.RemoveFlag(_stateFlags, state);
    }

    public void ClearActionFlags()
    {
        StateFlags = (int)CreatureState.None;
    }
}
