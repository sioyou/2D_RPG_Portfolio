using UnityEngine;

public class PlayerObject : CreatureObject
{
    protected override void Awake()
    {
        base.Awake();
        GetComponent<SpriteRenderer>().sortingOrder = SortingLayers.PLAYER;
    }
}
