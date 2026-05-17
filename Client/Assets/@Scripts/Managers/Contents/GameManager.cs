using UnityEngine;

public class GameManager
{
    public int MyObjectId { get; set; }
    public PlayerObject MyPlayer { get; set; }

    public void Reset()
    {
        MyObjectId = 0;
        MyPlayer = null;
    }

    public void SetMyObjectId(int objectId)
    {
        MyObjectId = objectId;

        GameObject go = Managers.Object.FindById(objectId);
        if (go != null && go.TryGetComponent(out PlayerObject playerObject))
            MyPlayer = playerObject;
    }
}
