using UnityEngine;

public class MonsterObject : CreatureObject
{
    protected override void Awake()
    {
        base.Awake();
        GetComponent<SpriteRenderer>().sortingOrder = SortingLayers.MONSTER;
    }
}
