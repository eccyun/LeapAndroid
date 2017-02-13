﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class DialogPanelComponent : MonoBehaviour {
	public delegate void Delegate();
	private Delegate yesClicked_;
	private Delegate noClicked_;

	void Start () {

	}

	void Update () {
	}

	public void show(string captionText, Delegate yesCallBack = null, Delegate noCallBack = null){
		this.gameObject.SetActive(true);

		GameObject caption = this.gameObject.transform.FindChild("Caption").gameObject;
		GameObject yesBtn  = this.gameObject.transform.FindChild("Yes").gameObject;
		GameObject noBtn   = this.gameObject.transform.FindChild("No").gameObject;

		// イベントをセットする
		noClicked_  = noCallBack;
		yesClicked_ = yesCallBack;

		// テキスト生成
		caption.GetComponent<Text>().text = captionText;
		setClickEvent_(yesBtn, yesClicked);
		setClickEvent_(noBtn, noClicked);
	}

	public void hide(){
		this.gameObject.SetActive(false);
	}

	public void setClickEvent_ (GameObject object_, UnityAction<BaseEventData> event_){
		// クリックイベントのセット
		EventTrigger trigger     = object_.GetComponent<EventTrigger> ();
		EventTrigger.Entry entry = new EventTrigger.Entry ();

		// クリックイベントのコールバック登録
		entry.eventID = EventTriggerType.PointerClick;
		entry.callback.AddListener(event_);
		trigger.triggers.Add(entry);
	}

	void yesClicked(BaseEventData eventData) {
        yesClicked_();
    }

	void noClicked(BaseEventData eventData) {
		noClicked_();
    }
}