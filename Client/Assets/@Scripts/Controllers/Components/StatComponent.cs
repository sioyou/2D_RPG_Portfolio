using System;
using Protocol;
using UnityEngine;

[Flags]
public enum StatChangeFlags
{
    None = 0,
    Hp = 1 << 0,
    MaxHp = 1 << 1,
    Level = 1 << 2,
    MoveSpeed = 1 << 3,
}

public class StatComponent : MonoBehaviour
{
    [SerializeField]
    int _maxHp = 100;

    [SerializeField]
    int _hp = 100;

    [SerializeField]
    int _level = 1;

    [SerializeField]
    float _moveSpeed = 5f;

    public int MaxHp => _maxHp;

    public int Hp => _hp;

    public int Level => _level;

    public float HpRatio => _maxHp > 0 ? (float)_hp / _maxHp : 0f;

    public float MoveSpeed => _moveSpeed;

    public event Action<StatChangeFlags> OnStatChanged;

    public void ApplyFrom(ObjectInfo info)
    {
        if (info == null)
            return;

        StatChangeFlags flags = StatChangeFlags.None;

        if (_level != info.Level)
        {
            _level = info.Level;
            flags |= StatChangeFlags.Level;
        }

        int newMaxHp = Mathf.Max(1, info.MaxHp);
        if (_maxHp != newMaxHp)
        {
            _maxHp = newMaxHp;
            flags |= StatChangeFlags.MaxHp;
        }

        int newHp = Mathf.Clamp(info.Hp, 0, _maxHp);
        if (_hp != newHp)
        {
            _hp = newHp;
            flags |= StatChangeFlags.Hp;
        }

        RaiseChanged(flags);
    }

    public void SetHpFromServer(int hp)
    {
        SetHp(hp);
    }

    public void SetMoveSpeed(float moveSpeed)
    {
        float clamped = Mathf.Max(0f, moveSpeed);
        if (Mathf.Approximately(_moveSpeed, clamped))
            return;

        _moveSpeed = clamped;
        RaiseChanged(StatChangeFlags.MoveSpeed);
    }

    void SetHp(int hp)
    {
        int clamped = Mathf.Clamp(hp, 0, _maxHp);
        if (_hp == clamped)
            return;

        _hp = clamped;
        RaiseChanged(StatChangeFlags.Hp);
    }

    void RaiseChanged(StatChangeFlags flags)
    {
        if (flags == StatChangeFlags.None)
            return;

        OnStatChanged?.Invoke(flags);
    }
}
