using Protocol;
using UnityEngine;

public abstract class BaseObject : MonoBehaviour
{
    public ObjectInfo Info { get; private set; }

    public bool IsMyPlayer =>
        Info != null && Info.ObjectId == Managers.Game.MyObjectId;

    public void SetInfo(ObjectInfo info)
    {
        Info = info;
        gameObject.name = $"{info.ObjectType}_{info.ObjectId}";
        transform.position = new Vector3(info.PosX, info.PosY, 0f);
        OnInfoSet(info);
    }

    protected virtual void OnInfoSet(ObjectInfo info) { }
}
