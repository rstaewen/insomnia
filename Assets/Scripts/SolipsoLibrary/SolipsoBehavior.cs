using UnityEngine;
using System.Collections;

public enum SolipsoMessage {Engine, UI, Log, Meta}
public abstract class SolipsoBehavior : MonoBehaviour
{
	protected virtual void OnDestroy()
	{
		EventRouter.Unsubscribe(SolipsoMessage.Engine, OnEngineSender);
		EventRouter.Unsubscribe(SolipsoMessage.Log, OnLogSender);
		EventRouter.Unsubscribe(SolipsoMessage.Meta, OnMetaSender);
		EventRouter.Unsubscribe(SolipsoMessage.UI, OnUISender);
	}

	protected void SubscribeEngineMessage()
	{
		EventRouter.Subscribe(SolipsoMessage.Engine, OnEngineSender);
	}

	protected void SubscribeUIMessage()
	{
		EventRouter.Subscribe(SolipsoMessage.UI, OnUISender);
	}

	protected void SubscribeLogMessage()
	{
		EventRouter.Subscribe(SolipsoMessage.Log, OnLogSender);
	}

	protected void SubscribeMetaMessage()
	{
		EventRouter.Subscribe(SolipsoMessage.Meta, OnMetaSender);
	}
	
	protected void OnEngineSender (EventRouter.Event evt) {if(evt.HasData) OnEngineMessage(evt.Data);}
	protected virtual void OnEngineMessage (object[] data) {}

	protected void OnUISender (EventRouter.Event evt) {if(evt.HasData) OnUIMessage(evt.Data);}
	protected virtual void OnUIMessage (object[] data) {}

	protected void OnLogSender (EventRouter.Event evt) {if(evt.HasData) OnLogMessage(evt.Data);}
	protected virtual void OnLogMessage (object[] data) {}

	protected void OnMetaSender (EventRouter.Event evt) {if(evt.HasData) OnMetaMessage (evt.Data);}
	protected virtual void OnMetaMessage (object[] data) {}

	public void EngineMessage(params object[] data){EventRouter.Publish(SolipsoMessage.Engine, this, data);}
	public void UIMessage(params object[] data){EventRouter.Publish(SolipsoMessage.UI, this, data);}
	public void LogMessage(params object[] data){EventRouter.Publish(SolipsoMessage.Log, this, data);}
	public void MetaMessage(params object[] data){EventRouter.Publish(SolipsoMessage.Meta, this, data);}
}
