using UnityEngine;
//using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems; 

public class ScrollCircle : ScrollRect  {
	private FixedJoystickHandler m_FixedJoystickHandler;
	public float radius; 
	/*
	protected float mRadius=0f;

	protected override void Start()
	{
		base.Start();
		//计算摇杆块的半径
		mRadius = (transform as RectTransform).sizeDelta.x * 0.5f;
	}

	public override void OnDrag (UnityEngine.EventSystems.PointerEventData eventData)
	{
		base.OnDrag (eventData);
		var contentPostion = this.content.anchoredPosition;
		if (contentPostion.magnitude > mRadius){
			contentPostion = contentPostion.normalized * mRadius ;
			SetContentAnchoredPosition(contentPostion);
		}
	}
	*/

	protected override void Start (){ 
		//计算摇杆块的半径
		this.radius = (((RectTransform)transform).sizeDelta.x * 0.5f) - (((RectTransform)transform.Find("Stick").transform).sizeDelta.x * 0.5f); 
		this.m_FixedJoystickHandler = this.transform.GetComponent<FixedJoystickHandler> ();
	} 

	public override void OnDrag (PointerEventData eventData){ 
		base.OnDrag (eventData); 

		// must do it 
		SetRound ();

		//计算向量
		this.m_FixedJoystickHandler.SetVector2(eventData, base.content.anchoredPosition);
	}

	public void SetStickPosition(Vector2 v2){
		Vector3 v3 = base.content.anchoredPosition;
		v3.x = v2.x;
		v3.y = v2.y;
		base.content.anchoredPosition = v2;

		// must do it 
		SetRound ();
	}
		
	private void SetRound(){
		//让球跟着鼠标走
		Vector3 v3 = base.content.anchoredPosition;
		v3.x = v3.x * 2;
		v3.y = v3.y * 2;
		base.content.anchoredPosition = Vector3.ClampMagnitude(v3 , this.radius); 
	}
}