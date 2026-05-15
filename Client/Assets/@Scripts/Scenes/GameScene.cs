using UnityEngine;
using static Define;

public class GameScene : BaseScene
{
    protected override void Awake()
    {
        base.Awake();

        Debug.Log("@>> GameScene Init()");
        SceneType = EScene.GameScene;
    }

    protected override void Start()
    {
        base.Start();
        Managers.Init();
    }

	public override void Clear()
    {
    }
	
	void OnApplicationQuit()
	{
		Managers.Network.GameServer.Disconnect();
	}
}