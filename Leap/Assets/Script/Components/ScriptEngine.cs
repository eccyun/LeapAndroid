﻿using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.UI;

public class ScriptEngine : SingletonMonoBehaviour<ScriptEngine> {
	public  delegate void Delegate();

	public  List<string>      listScript;  // スクリプトファイルから読み込んだ命令群を格納
	public  int               cnt;         // スクリプトのカウンタ
	public  int               loadCnt;     // ロードした時に進めたいカウンタ
	public  GameDataComponent gameDataComponent;
	public  List<string[]>    textLogs;
	public  int               chapter;     // 章情報
	public  GameObject        stillPrefab; // スチルのプレハブ
	public  bool              stop_flg;    // テキストの読みこみを止めるかを判定 trueなら止める
	public  bool              load_flg;
	public  string            moveLoadedSceneName;
	public  GameObject[]      animationObjects;
	public  BGMComponent      bgm;

	public void Awake(){
        if(this != Instance){
            Destroy(this);
            return;
        }
        DontDestroyOnLoad(this.gameObject);
    }

	public void Start () {
		textLogs          = new List<string[]>();
		gameDataComponent = GameObject.Find("GameDataComponent").GetComponent<GameDataComponent> ();
		bgm               = GameObject.Find("BGM").GetComponent<BGMComponent>();
	}

	public void initGameScene(int currentCnt, int currentChapter=0){
		cnt = currentCnt;

		if(currentChapter==0){
			chapter++;
		}else{
			chapter = currentChapter;
		}
	}

	IEnumerator readScenarioFileAndroid(string fileName){
		string txtBuffer = "";
		string path      = "jar:file://" + Application.dataPath + "!/assets" + "/Scenario/"+fileName;
		WWW    www       = new WWW(path);
		yield return www;

		TextReader txtReader = new StringReader(www.text);
		while ((txtBuffer = txtReader.ReadLine()) != null){
			listScript.Add(txtBuffer);
		}
	}

	public void readScenarioFile(){
		// 変数の初期化
		listScript = new List<string>();

		string fileName  = "script"+chapter.ToString("0,0")+".txt";
		string stillName = "Prefab/Still-"+chapter.ToString("0,0");

		// スチルデータのセット
		stillPrefab = (GameObject)Resources.Load (stillName);
		if(stillPrefab != null){
			stillPrefab = (GameObject)Instantiate(stillPrefab, Vector3.zero, Quaternion.identity);
			// アニメーションのオブジェクトを取得する
			animationObjects = GameObject.FindGameObjectsWithTag("Animation");
		}

#if UNITY_EDITOR
		FileInfo f = new FileInfo(Application.streamingAssetsPath+"/Scenario/"+fileName);
#elif UNITY_IOS
		FileInfo f = new FileInfo(Application.dataPath+"/Raw/Scenario/"+fileName);
#elif UNITY_ANDROID
		StartCoroutine(readScenarioFileAndroid(fileName));
#endif

#if UNITY_EDITOR || UNITY_IOS
		try{
			// 一行毎読み込み
			using (StreamReader sr = new StreamReader(f.OpenRead(), Encoding.UTF8)){
				while(sr.Peek() >= 0){
					listScript.Add(sr.ReadLine());
				}
				sr.Close();
			}
		}catch (Exception e){
			// エラー処理
		}
#endif
	}

	public byte[] displayCapture(){
		Texture2D texture = new Texture2D(Screen.width, Screen.height);
        texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        texture.Apply();

		return texture.EncodeToPNG();
	}

	public void setActiveScreenShot(){
		// byteを文字列になおして、入れる
		byte[] bytes = displayCapture();
		gameDataComponent.activeData.setActiveScreenShot(Convert.ToBase64String(bytes));
	}

	public void setAbridgeText(string abridgeText){
		gameDataComponent.activeData.setAbridgeText(abridgeText);
	}

	public string[] readScript(){
		if(stop_flg) return null;

		string ret = listScript[cnt];
		cnt++;

		if(ret == "STOP;"){
			gameDataComponent.activeData.setActiveData(chapter, cnt);

			if(load_flg){
				// ロード中の場合ストップさせない、カウンタが追いつくまでやる
				if(cnt >= loadCnt){
					// 再生
					bgm.play_();

					// ロゴの非表示
					GameObject loadLogo = GameObject.Find("LoadLogo");
					Image      renderer = loadLogo.GetComponent<Image>();
					renderer.enabled    = false;

					PanelComponent panelComponent = GameObject.Find("Panel").GetComponent<PanelComponent> ();
					panelComponent.isFade         = true;
					panelComponent.orderEdit(0);

					load_flg = false;
					stop_flg = true;
				}
			}else{
				stop_flg = true;
				// STOP判定が来たらクイックセーブ
				gameDataComponent._save(999);
			}
			return null;
		}

		return ret.Split(':');
	}

	public bool setLoadGameData(GameData d=null){
		// データセット
		cnt      = 0;
		chapter  = 0;
		loadCnt  = 0;
		textLogs = new List<string[]>();
		stop_flg = false;

		if(d!=null){
			loadCnt  = d.scriptCnt;
			chapter  = d.chapter;
			load_flg = true;
		}

		return true;
	}

	// 改行コード処理
    private string SetDefaultText(){
        return "C#あ\n";
    }
}
