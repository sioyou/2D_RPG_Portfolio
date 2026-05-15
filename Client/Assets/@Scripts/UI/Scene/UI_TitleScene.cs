using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using static Define;
using Object = UnityEngine.Object;

public class UI_TitleScene : UI_Scene
{
	private enum GameObjects
	{
		StartButton,
	}

	private enum Texts
	{
		StatusText,
	}

	private enum TitleSceneState
	{
		None,
		AssetLoading,
		AssetLoaded,
		ConnectingToServer,
		ConnectedToServer,
		FailedToConnectToServer,
	}

	TitleSceneState _state = TitleSceneState.None;
	TitleSceneState State
	{
		get { return _state; }
		set
		{
			_state = value;
			switch (value)
			{
				case TitleSceneState.None:
					break;
				case TitleSceneState.AssetLoading:
					GetText((int)Texts.StatusText).text = $"TODO 로딩중";
					break;
				case TitleSceneState.AssetLoaded:
					GetText((int)Texts.StatusText).text = "TODO 로딩 완료";
					break;
				case TitleSceneState.ConnectingToServer:
					GetText((int)Texts.StatusText).text = "TODO 서버 접속중";
					break;
				case TitleSceneState.ConnectedToServer:
					GetText((int)Texts.StatusText).text = "TODO 서버 접속 성공";
					break;
				case TitleSceneState.FailedToConnectToServer:
					GetText((int)Texts.StatusText).text = "TODO 서버 접속 실패";
					break;
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();

		BindObjects(typeof(GameObjects));
		BindTexts(typeof(Texts));

		GetObject((int)GameObjects.StartButton).BindEvent((evt) =>
		{
			Debug.Log("OnClick");
			Managers.Scene.LoadScene(EScene.GameScene);
		});

		GetObject((int)GameObjects.StartButton).gameObject.SetActive(false);
	}

	void OnEnable()
	{
		Managers.Network.OnGameServerConnected += HandleConnected;
		Managers.Network.OnGameServerConnectFailed += HandleConnectFailed;
	}

	void OnDisable()
	{
		if (Managers.Initialized == false)
			return;

		Managers.Network.OnGameServerConnected -= HandleConnected;
		Managers.Network.OnGameServerConnectFailed -= HandleConnectFailed;
	}

	protected override void Start()
	{
		base.Start();
		BeginFlow();
	}

	void BeginFlow()
	{
		// Addressables Preload는 포트폴리오 리소스 전략 확정 후 연동
		OnAssetLoaded();
	}

	void OnAssetLoaded()
	{
		State = TitleSceneState.AssetLoaded;
		Managers.Data.Init();

		Debug.Log("Connecting To Server");
		State = TitleSceneState.ConnectingToServer;
		Managers.Network.ConnectGameServer();
	}

	void HandleConnected()
	{
		State = TitleSceneState.ConnectedToServer;
		GetObject((int)GameObjects.StartButton).gameObject.SetActive(true);
	}

	void HandleConnectFailed()
	{
		State = TitleSceneState.FailedToConnectToServer;
	}

}
