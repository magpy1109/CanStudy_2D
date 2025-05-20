using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	#region Singleton
	static public GameManager Instance { get; private set; }

	private void Awake() => Instance = this;
	#endregion

	/// <summary>
	/// Text는 Legacy 컴포넌트. 이제는 TMPro의 TextMeshPro를 사용한다.
	/// TextMeshPro는 Unity의 Package
	/// </summary>
	[SerializeField] private Text _scoreText;

	private void Start() => Score = 0; // 프로퍼티를 사용. set 함수 호출

	private int _score; // 이 변수를 사용하지 않도록 한다
	public int Score
	{
		get => _score;
		set
		{
			_score = value;
			_scoreText.text = $"Score : {Score}";
		}
	}
}