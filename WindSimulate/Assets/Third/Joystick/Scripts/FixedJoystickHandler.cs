using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class FixedJoystickHandler : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler 
{
	[Serializable] 
	public class VirtualJoystickEvent : UnityEvent<Vector3>{}

	public Transform content; 
	public UnityEvent beginControl; 
	public VirtualJoystickEvent controlling; 
	public UnityEvent endControl;
	public Canvas canvas;

	private Vector2 res;
	private ScrollCircle scrollCircle;

	//  第二种表现： 鼠标点哪，球在哪 : 1
	public int clickEffect = 0; // 0 or 1;
	private float[] initPosition = {0f,0f};
	private bool isInitPosition = false;
	private float touchRadius = 150;
	private float resX = 0;
	private float resY = 0;


	void Start ()
    {
		ScrollCircle other = this.GetComponent<ScrollCircle>();

		touchRadius = other.radius;
		//*2
		scrollCircle = transform.GetComponent<ScrollCircle> ();

		Vector2 posstickGo = Vector2.zero;//TransformToCanvasLocalPosition (stickGo.transform, canvas);
		RectTransform canvasRectTransform = canvas.GetComponent<RectTransform> ();

		float scale = canvas.pixelRect.width / canvasRectTransform.rect.width;
		initPosition [0] = (canvasRectTransform.rect.width * 0.5f + posstickGo.x) * scale;
		initPosition [1] = (canvasRectTransform.rect.width * 0.5f + posstickGo.x) * scale;
	}

	//摇杆开始拖拽
	public void OnBeginDrag(PointerEventData eventData)
    {
        resX = 0f;
		resY = 0f;
		//*2
		if (clickEffect == 1 && !isInitPosition) 
        {
			isInitPosition = true;

			Vector2 v2 = eventData.pressPosition;
			v2.x = initPosition [0];
			v2.y = initPosition [1];
			eventData.pressPosition = v2;

			Vector2 v2Position = eventData.position;
			v2Position.x = v2Position.x - initPosition [0];
			v2Position.y = v2Position.y - initPosition [1];
							
			scrollCircle.SetStickPosition (v2Position);
        }

        this.beginControl.Invoke(); 
	}

	//拖拽摇杆
	public void OnDrag(PointerEventData eventData)
    { 
		if(this.content)
        { 
			this.controlling.Invoke(this.content.localPosition.normalized); 
		} 
	}

	//拖拽结束
	public void OnEndDrag(PointerEventData eventData)
    { 
		resX = 0f;
		resY = 0f;
		//*2
		if (clickEffect == 1)
        {
            isInitPosition = false;
        }
			
        this.endControl.Invoke(); 
	}

	public Vector2 GetVector2()
    {
		res = new Vector2(resX, resY);
		return res;
	}

	public void SetVector2(PointerEventData eventData, Vector3 v3)
    {
		float pressX = eventData.pressPosition.x;
		float pressY = eventData.pressPosition.y;

		float currX = eventData.position.x;
		float currY = eventData.position.y;

		float absoluteX = v3.x;
		float absoluteY = v3.y;

		float _x = (currX - pressX);
		float _y = (currY - pressY);

		if (_x > 0 && _x > absoluteX)
			_x = absoluteX;

		if (_x < 0 && _x < absoluteX)
			_x = absoluteX;

		if (_y > 0 && _y > absoluteY)
			_y = absoluteY;

		if (_y < 0 && _y < absoluteY)
			_y = absoluteY;

		//水平方向为x，右向为正；垂直方向为y，上向为正
		//_x = -_x;

		resX = (float)System.Math.Round ((double)(System.Math.Abs (_x) / touchRadius) >= 0x31 ? (_x > 0 ? 0x31 : 0x2d31) : (_x / touchRadius), 2);
		resY = (float)System.Math.Round ((double)(System.Math.Abs (_y) / touchRadius) >= 0x31 ? (_y > 0 ? 0x31 : 0x2d31) : (_y / touchRadius), 2);
	}


	public Vector2 TransformToCanvasLocalPosition(Transform current, Canvas canvas)
	{
		Vector2 screenPos = canvas.worldCamera.WorldToScreenPoint(current.transform.position);
		Vector2 localPos;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.GetComponent<RectTransform>(), screenPos,
			canvas.worldCamera, out localPos);
		return localPos;
	}

}