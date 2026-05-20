using UnityEngine;

public class GameManager
{
    public int MyObjectId { get; set; }
    public MyPlayerObject MyPlayerObject { get; private set; }

    public void Reset()
    {
        MyObjectId = 0;
        MyPlayerObject = null;
    }

    public void SetMyObjectId(int objectId)
    {
        MyObjectId = objectId;
        TryBindExistingMyHero();
    }

    public void BindMyHero(MyPlayerObject myPlayerObject)
    {
        MyPlayerObject = myPlayerObject;
    }

    public void ClearMyHero()
    {
        MyPlayerObject = null;
    }

    public void TryBindExistingMyHero()
    {
        if (MyObjectId <= 0)
            return;

        MyPlayerObject existing = Managers.Object.Find<MyPlayerObject>(MyObjectId);
        if (existing != null)
            MyPlayerObject = existing;
    }
}
