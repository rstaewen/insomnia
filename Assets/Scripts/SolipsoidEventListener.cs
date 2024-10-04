using UnityEngine;
using System.Collections;

public class SolipsoidEventListener : SolipsoBehavior {

	public string eventName;
	public SolipsoMessage eventType;
	public enum Response{ActivateObject, DeactivateObject}
	public Response response;
	// Use this for initialization
	void Awake () {
		switch(eventType)
		{
		case SolipsoMessage.Engine:
			SubscribeEngineMessage();
			break;
		case SolipsoMessage.Log:
			SubscribeLogMessage();
			break;
		case SolipsoMessage.Meta:
			SubscribeMetaMessage();
			break;
		case SolipsoMessage.UI:
			SubscribeUIMessage();
			break;
		}
	}

	bool matchEvent (object[] data) {return (string)data[0] == eventName;}

	protected override void OnEngineMessage (object[] data)
	{
		if(matchEvent(data))
			Respond();
	}

	protected override void OnLogMessage (object[] data)
	{
		if(matchEvent(data))
			Respond();
	}

	protected override void OnMetaMessage (object[] data)
	{
		if(matchEvent(data))
			Respond();
	}

	protected override void OnUIMessage (object[] data)
	{
		if(matchEvent(data))
			Respond();
	}

	void Respond()
	{
		switch(response)
		{
		case Response.ActivateObject:
			gameObject.SetActive(true);
			break;
		case Response.DeactivateObject:
			gameObject.SetActive(false);
			break;
		}
	}
}
