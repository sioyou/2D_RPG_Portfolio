using UnityEngine;

public abstract class BaseScene : MonoBehaviour
{
    public Define.EScene SceneType { get; protected set; } = Define.EScene.Unknown;

    protected virtual void Awake() { }

	protected virtual void Start() { }
	protected virtual void Update() { }

	public abstract void Clear();
}
