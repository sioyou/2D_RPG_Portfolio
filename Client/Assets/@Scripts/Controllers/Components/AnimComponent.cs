using Protocol;
using UnityEngine;

public class AnimComponent : MonoBehaviour
{
    private Animator _animator;
    private SpriteRenderer _renderer;
    void Awake()
    {
        _animator = GetComponent<Animator>();
        _renderer = GetComponent<SpriteRenderer>();
    }

    public void PlayAnim(int state)
    {
        if (CreatureStateUtil.HasFlag(state, CreatureState.Damaged))
        {
            _animator.CrossFade(AnimName.HIT,0);
        }
        else if (CreatureStateUtil.HasFlag(state, CreatureState.Move))
        {
            _animator.CrossFade(AnimName.MOVE, 0);
        }
        else if (CreatureStateUtil.HasFlag(state, CreatureState.None))
        {
            _animator.CrossFade(AnimName.IDLE, 0);
        }
    }

    public void SetFlip(bool right)
    {
        if (_renderer == null)
            return;

        _renderer.flipX = !right;
    }
}
