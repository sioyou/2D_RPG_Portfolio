using System;
using System.Collections.Generic;
using Protocol;
using UnityEngine;

public class ObjectManager
{
    readonly Dictionary<int, GameObject> _objects = new Dictionary<int, GameObject>();

    #region Roots
    public Transform GetRootTransform(string name)
    {
        GameObject root = GameObject.Find(name);
        if (root == null)
            root = new GameObject { name = name };

        return root.transform;
    }
    
    public Transform PlayerRoot => GetRootTransform("@Players");
    public Transform MonsterRoot => GetRootTransform("@Monsters");
    public Transform ProjectileRoot => GetRootTransform("@Projectiles");
    public Transform EnvRoot => GetRootTransform("@Envs");
    public Transform EffectRoot => GetRootTransform("@Effects");
    public Transform NpcRoot => GetRootTransform("@Npc");
    public Transform ItemHolderRoot => GetRootTransform("@ItemHolders");
    #endregion

    public void SpawnAll(IEnumerable<ObjectInfo> spawns)
    {
        if (spawns == null)
            return;

        foreach (ObjectInfo info in spawns)
            Spawn(info);
    }

    public void Spawn(ObjectInfo info)
    {
        if (info == null)
            return;

        if (_objects.ContainsKey(info.ObjectId))
            return;

        Transform parent = info.ObjectType switch
        {
            GameObjectType.ObjectTypePlayer => PlayerRoot,
            GameObjectType.ObjectTypeMonster => MonsterRoot,
            _ => EnvRoot,
        };

        GameObject go = TryInstantiatePrefab(info, parent);
        if (go == null)
        {
            Debug.LogError($"Disable Prefab :{info.ObjectId}");
            return;
        }

        BaseObject obj = AttachObjectComponent(go, info);
        if (obj == null)
        {
            UnityEngine.Object.Destroy(go);
            return;
        }

        obj.SetInfo(info);
        _objects[info.ObjectId] = go;

        Debug.Log($"[ObjectManager] Spawn objectId={info.ObjectId} type={info.ObjectType} " +
                  $"lv={info.Level} hp={info.Hp}/{info.MaxHp} pos=({info.PosX},{info.PosY})");
    }

    public void Despawn(int objectId)
    {
        if (_objects.TryGetValue(objectId, out GameObject go) == false)
            return;

        if (Managers.Game.MyPlayer != null && go == Managers.Game.MyPlayer.gameObject)
            Managers.Game.MyPlayer = null;

        _objects.Remove(objectId);
        Managers.Resource.Destroy(go);
    }

    public GameObject FindById(int id)
    {
        _objects.TryGetValue(id, out GameObject go);
        return go;
    }

    public T Find<T>(int objectId) where T : BaseObject
    {
        GameObject go = FindById(objectId);
        if (go == null)
            return null;

        go.TryGetComponent(out T component);
        return component;
    }

    public GameObject Find(Func<GameObject, bool> condition)
    {
        foreach (GameObject obj in _objects.Values)
        {
            if (condition.Invoke(obj))
                return obj;
        }

        return null;
    }

    public void Clear()
    {
        foreach (GameObject obj in _objects.Values)
            Managers.Resource.Destroy(obj);

        _objects.Clear();
        Managers.Game.Reset();
    }

    static BaseObject AttachObjectComponent(GameObject go, ObjectInfo info)
    {
        switch (info.ObjectType)
        {
            case GameObjectType.ObjectTypePlayer:
            {
                if (go.TryGetComponent(out PlayerObject playerObject))
                    return playerObject;

                RemoveOtherObjectComponents<PlayerObject>(go);
                return go.AddComponent<PlayerObject>();
            }
            case GameObjectType.ObjectTypeMonster:
            {
                if (go.TryGetComponent(out MonsterObject monsterObject))
                    return monsterObject;

                RemoveOtherObjectComponents<MonsterObject>(go);
                return go.AddComponent<MonsterObject>();
            }
            default:
                Debug.LogWarning($"[ObjectManager] Unsupported object type: {info.ObjectType}");
                return null;
        }
    }

    static void RemoveOtherObjectComponents<T>(GameObject go) where T : BaseObject
    {
        BaseObject[] components = go.GetComponents<BaseObject>();
        foreach (BaseObject component in components)
        {
            if (component is T)
                continue;

            UnityEngine.Object.Destroy(component);
        }
    }

    GameObject TryInstantiatePrefab(ObjectInfo info, Transform parent)
    {
        string key = info.ObjectType switch
        {
            GameObjectType.ObjectTypePlayer => "Player",
            GameObjectType.ObjectTypeMonster => "Monster",
            _ => string.Empty,
        };

        if (string.IsNullOrEmpty(key))
            return null;

        if (Managers.Resource.CheckResource<GameObject>(key) == false)
            return null;

        return Managers.Resource.Instantiate(key, parent);
    }

}
