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
        if (_animator == null)
            return;

        if (CreatureStateUtil.HasFlag(state, CreatureState.Dead))
            _animator.CrossFade(AnimName.DIE, 0);
        else if (CreatureStateUtil.HasFlag(state, CreatureState.Damaged))
            _animator.CrossFade(AnimName.HIT, 0);
        else if (CreatureStateUtil.HasFlag(state, CreatureState.Attack))
            _animator.CrossFade(AnimName.ATTACK, 0);
        else if (CreatureStateUtil.HasFlag(state, CreatureState.Move))
            _animator.CrossFade(AnimName.MOVE, 0);
        else if (CreatureStateUtil.HasFlag(state, CreatureState.None))
            _animator.CrossFade(AnimName.IDLE, 0);
    }

    public void SetFlip(bool right)
    {
        if (_renderer == null)
            return;

        _renderer.flipX = !right;
    }

    public float GetClipLength(string clipName)
    {
        if (_animator == null || string.IsNullOrEmpty(clipName))
            return 0f;

        RuntimeAnimatorController controller = _animator.runtimeAnimatorController;
        if (controller == null)
            return 0f;

        foreach (AnimationClip clip in controller.animationClips)
        {
            if (clip != null && clip.name == clipName)
                return clip.length;
        }

        return 0f;
    }
}
