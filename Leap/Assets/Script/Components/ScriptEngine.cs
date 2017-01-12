﻿using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Text;
using System.Collections.Generic;

public class ScriptEngine : SingletonMonoBehaviour<ScriptEngine> {
	private List<string>   listScript;  // スクリプトファイルから読み込んだ命令群を格納
	private int            cnt;         // スクリプトのカウンタ
	public  List<string[]> textLogs;
	public  int            chapter;     // 章情報
	public  GameObject     stillPrefab; // スチルのプレハブ
	public  bool           stop_flg;    // テキストの読みこみを止めるかを判定 trueなら止める

	public void Awake(){
        if(this != Instance){
            Destroy(this);
            return;
        }
        DontDestroyOnLoad(this.gameObject);
    }

	public void Start () {
		textLogs = new List<string[]>();
	}

	public void readScenarioFile(){
		// 変数の初期化
		listScript = new List<string>();
		cnt        = 0;
		chapter++;

		string fileName  = "script"+chapter.ToString("0,0")+".txt";
		string stillName = "Prefab/Still-"+chapter.ToString("0,0");

		// スチルデータのセット
		stillPrefab = (GameObject)Resources.Load (stillName);
		if(stillPrefab != null){
			Instantiate(stillPrefab, Vector3.zero, Quaternion.identity);
		}

		FileInfo f = new FileInfo(Application.streamingAssetsPath+"/Scenario/"+fileName);
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
	}

	public string[] readScript(){
		if(stop_flg) return null;

		string ret = listScript[cnt];
		cnt++;

		if(ret == "STOP;"){
			stop_flg = true;
			return null;
		}

		return ret.Split(':');
	}

	public void fade(string mode, float range, String color="black"){
		SceneComponent sceneComponent = GameObject.Find("MainGameManager").GetComponent<SceneComponent> ();
		sceneComponent.fade(mode, range, color);
	}

	// 改行コード処理
    private string SetDefaultText(){
        return "C#あ\n";
    }
}
