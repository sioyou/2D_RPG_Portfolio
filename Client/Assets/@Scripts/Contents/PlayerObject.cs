using Protocol;
using UnityEngine;

public class PlayerObject : BaseObject
{
    protected override void OnInfoSet(ObjectInfo info)
    {
        if (IsMyPlayer)
            Managers.Game.MyPlayer = this;
    }
}
