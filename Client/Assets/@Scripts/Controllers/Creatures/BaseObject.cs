using Protocol;
using UnityEngine;

public class BaseObject : MonoBehaviour
{
    public ObjectInfo Info { get; private set; }

    public float MoveSpeed = 5f;

    public bool IsMyPlayer =>
        Info != null && Info.ObjectId == Managers.Game.MyObjectId;

    protected virtual void Awake() { }

    protected virtual void Start() { }

    protected virtual void Update() { }

    public void SetInfo(ObjectInfo info)
    {
        Info = info;
        gameObject.name = $"{info.ObjectType}_{info.ObjectId}";
        transform.position = new Vector3(info.PosX, info.PosY, 0f);
        OnInfoSet(info);
    }

    protected virtual void OnInfoSet(ObjectInfo info) { }
}
