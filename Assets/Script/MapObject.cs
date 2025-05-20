using UnityEngine;

public class MapObject : MonoBehaviour
{
	public enum ObjectType { Coin, Obstacle }
	[SerializeField] private ObjectType _type;
	[SerializeField] private SpriteRenderer _renderer;
	[SerializeField] private Collider2D _collider;
	[SerializeField] private int _amount = 10;

	private void Start() => _renderer.enabled = _collider.enabled = true;

	#region Collision
	/* MonoBehaviour의 내장 Collision 함수들

	OnCollision + [Enter/Stay/Exit] + [/2D] : Collider와 충돌할 때 받는 이벤트 콜백 함수
	OnTrigger + [Enter/Stay/Exit] + [/2D] : Collider가 IsTrigger일 때 충돌 이벤트 콜백 함수

	이외에도 많은 이벤트 함수가 있다. (On...으로 시작
	*/
	private void OnTriggerEnter2D(Collider2D collider)
	{
		/// [중요] collider2D에서는 gameObject에 접근할 수 있다
		/// collider.gameObject, collider.GetComponent<T>()
		if (collider.tag == "Player")
		{
			switch (_type)
			{
				case ObjectType.Obstacle:
					var player = collider.GetComponent<PlayerController>();
					player.HP -= _amount;
					break;
				case ObjectType.Coin:
					GameManager.Instance.Score += _amount;
					break;
			}
			
			/// 컴포넌트의 OnDisable을 호출
			/// renderer는 드로우를 중지, collider는 충돌 감지를 중지
			_renderer.enabled = _collider.enabled = false;
		}
	}
	#endregion
}