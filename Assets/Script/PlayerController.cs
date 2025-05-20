using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 간단한 State Machine으로 동작하는 스크립트. (FSM : 유한 상태 기계, 디자인 패턴 중 하나)
/// enum PlayerState에 따라 Rigidbody, Animation을 조작한다.
/// 입력은 Input System 패키지를 사용했다.
/// </summary>
/// [RequireComponent( type )] :
/// 	이 컴포넌트(MonoBehaviour)가 부착된 GameObject에
/// 	다음과 같은 type의 Component가 있는지 검사하고 없으면 붙여라.
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
	// #region : 코드 구역 나누기. 가독성을 위해 쓰며 접고 펼치기 가능

	#region Inspector / Property
	/// Inspector
	/// 	Editor에서 GameObject를 클릭했을 때 Inspector창에서 설정 가능한 변수들
	/// 	private, protected 변수는 띄우지 않는 것이 원칙

	/* [SerializeField] : 직렬화 필드
		private 변수지만 Inspector에서 설정하고 싶을 때나 JSON 데이터에 포함하고 싶을 때 사용

		public 변수는 추천하지 않는다.
			다른 컴포넌트가 이 클래스를 사용할 때 불필요한 변수가 많을 수록
			클래스 이해가 어려워지고 가독성이 떨어진다
	*/
	[SerializeField] private float _moveSpeed = 5f;
	[SerializeField] private float _jumpForce = 10f;
	/* LayerMask

		GameObject가 Collision용으로 사용하는 Layer의 타입.
		Inspector에서 우측 상단의 Layer를 바꿀 수 있다.
	*/
	[SerializeField] private LayerMask _groundLayer;
	[SerializeField] private BoxCollider2D _outOfMapCollider;

	/// Property : 통상의 get, set 메서드와 같음
	/// 	변수에 대입할 때 값에 따라 어떤 행동을 해야하는 경우 사용
	/// 	summary 주석을 사용한다면 Property에 hover할 때 설명을 띄워준다
	private enum PlayerState
	{
		Dead = 0,
		Run = 1,
		Slide = 2,
		Jump = 4,
		Double_Jump = 5
	}
	private const int JUMP_MAX = 2;

	[SerializeField] private Slider _hpSlider;
	[SerializeField] private int _hp = 20; // 이 변수에 대입하는 일이 없도록 하자
	/// <summary>
	/// HP가 0 이하일 경우 PlayerState는 Dead가 된다
	/// </summary>
	public int HP
	{
		get => _hp;
		set
		{
			if ((_hp = value) <= 0)
				State = PlayerState.Dead;
			_hpSlider.value = value;
		}
	}

	private PlayerState _state; // 이 변수에 대입하는 일이 없도록 하자
	/// <summary>
	/// State Machine의 행동을 정의하는 프로퍼티.
	/// 모든 State에 관한 행동은 여기에서만 정의된다.
	/// </summary>
	private PlayerState State
	{
		get => _state;
		set
		{
			if (_state == value) return;

			switch (_state)
			{
				case PlayerState.Slide:
					_col.offset += new Vector2(0, _col.offset.y);
					_col.size += new Vector2(0, _col.size.y);
					break;
				case PlayerState.Jump:
				case PlayerState.Double_Jump:
					_jumpCount = 0;
					break;
			}

			switch (_state = value)
			{
				case PlayerState.Slide:
					_col.offset += new Vector2(0, -_col.offset.y / 2f);
					_col.size += new Vector2(0, -_col.size.y / 2f);
					break;
				case PlayerState.Jump:
				case PlayerState.Double_Jump:
					_rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
					_jumpCount++;
					break;
				case PlayerState.Dead:
					_rb.gravityScale = 0f;
					_rb.linearVelocity = Vector2.zero;
					break;
			}
			_anim.SetInteger("State", (int)State);
		}
	}
	#endregion

	#region Members / Initialization
	private PlayerInputActions _input; // Input System 패키지 방식 중 하나
	private Rigidbody2D _rb;
	private BoxCollider2D _col;
	private Animator _anim;
	private bool _isGrounded;
	private int _jumpCount;

	private void Awake()
	{
		_input = new PlayerInputActions();

		_rb = GetComponent<Rigidbody2D>();
		_col = GetComponent<BoxCollider2D>();
		_anim = GetComponent<Animator>();

		_hpSlider.maxValue = _hp;
		_hpSlider.value = _hp;

		State = PlayerState.Run;
	}
	#endregion

	#region Input Callbacks
	private void OnEnable()
	{
		_input.Enable();
		_input.Player.Jump.started += OnJump; // started : 버튼을 눌렀을 때
		_input.Player.Crouch.started += OnSlide;
		_input.Player.Crouch.canceled += OnSlide; // canceled : 버튼을 떼었을 때
	}
	private void OnDisable()
	{
		_input.Disable();
		_input.Player.Jump.started -= OnJump; // started : 버튼을 눌렀을 때
		_input.Player.Crouch.started -= OnSlide;
		_input.Player.Crouch.canceled -= OnSlide; // canceled : 버튼을 떼었을 때
	}

	private void OnJump(InputAction.CallbackContext context)
	{
		if (State != PlayerState.Dead && _jumpCount < JUMP_MAX)
			State = _jumpCount == 0 ? PlayerState.Jump : PlayerState.Double_Jump;
	}
	private void OnSlide(InputAction.CallbackContext context)
	{
		if (!_isGrounded || State == PlayerState.Dead) return;

		if (context.phase == InputActionPhase.Started)
		{
			State = PlayerState.Slide;
		}
		/// State를 체크하는 이유는 버튼을 눌렀을 때 조건이 맞지 않아 Slide하지 않았지만
		/// 버튼을 뗀 입력은 들어오기 때문에
		else if (State == PlayerState.Slide) // phase는 Canceled임
		{
			State = PlayerState.Run;
		}
	}
	#endregion

	#region Update
	/// Update의 종류
	/// Update - FixedUpdate - LateUpdate순으로 작동한다
	/// FixedUpdate는 주로 Physics(물리) 관련 행동을
	/// LateUpdate는 주로 Render(드로우) 관련 행동을 수행한다

	/// <summary>
	/// Physics(물리) 관련의 행동을 할 때 자주 쓰이는 Update
	/// </summary>
	private void FixedUpdate()
	{
		if (State == PlayerState.Dead) return;

		_rb.linearVelocityX = _moveSpeed;
		_isGrounded = Physics2D.IsTouchingLayers(_col, _groundLayer);

		/// Jump = 4 (0100), Double_Jump = 5 (0101)
		/// 비트 연산자 &는 AND이므로 4의 자리 비트가 1인지를 검사한다
		if ((State & PlayerState.Jump) > 0)
		{
			/// 점프한 바로 뒤의 프레임에선 isGrounded일 수 있으므로
			/// rigidbody의 속도가 아래로 향하는 때가 점프가 끝나는 타이밍
			if (_isGrounded && !(_rb.linearVelocityY > 0f))
				State = PlayerState.Run;
		}

		// Map Place Collider에서 벗어났을 때
		if (!_outOfMapCollider.OverlapPoint(_rb.position))
			HP = 0;
	}

	/// <summary>
	/// transform은 Physics(물리)와는 다른 일반 컴포넌트로 작용하는 듯.
	/// 속도가 느리니까 transform보다는 물리 컴포넌트를 이용하자.
	/// </summary>
	private void Update()
	{
		// Camera가 position의 x를 따라오도록 (y=0, z=-10 고정)
		var camPos = Camera.main.transform.position;
		Camera.main.transform.position = new Vector3(transform.position.x, camPos.y, camPos.z);
	}
	#endregion
}