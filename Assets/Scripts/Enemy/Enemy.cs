using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour
{
	[Header("Movement Settings")]
	public Waypoint waypoint;
	public float patrolSpeed = 3f;
	public float chaseSpeed = 5f;
	public float reachThreshold = 0.1f;

	[Header("Vision Settings")]
	[Range(30, 180)] public float visionAngle = 90f;
	public float visionDistance = 5f;
	public LayerMask visionBlockingLayers;

	[Header("Player Settings")]
	public Transform player;
	public float returnDelay = 2f;

	[Header("Waypoint Pausing")]
	public float pauseAtFirstWaypoint = 1f;
	public float pauseAtLastWaypoint = 1f;

	[Header("Alert Settings")]
	public float alertDuration = 1f;

	[Header("Debug")]
	[SerializeField] private int _currentWaypointIndex = 0;
	[SerializeField] private bool _isChasing = false;
	[SerializeField] private bool _movingForward = true;
	[SerializeField] private bool _isPaused = false;
	[SerializeField] private float _pauseEndTime;
	[SerializeField] private bool _isAlerted = false;
	[SerializeField] private float _alertEndTime;

	private string _currentAnimationState = "Move";
	private Animator _animator;
	private NavMeshAgent _agent;
	private Vector3 _lastWaypointPosition;
	private float _playerLostTime;
	private Vector3 _originalScale;
	private Vector2 _currentFacingDirection = Vector2.right;

	private void Start()
	{
		_agent = GetComponent<NavMeshAgent>();
		_agent.updateRotation = false;
		_agent.updateUpAxis = false;
		_agent.speed = patrolSpeed;
		_agent.stoppingDistance = reachThreshold;
		_originalScale = transform.localScale;
		_animator = GetComponent<Animator>();

		_animator.Play("Move");
	}

	private void Update()
	{
		transform.rotation = Quaternion.identity;

		if (!waypoint || waypoint.Points.Length == 0) return;

		if (_isPaused)
		{
			if (Time.time >= _pauseEndTime)
			{
				_isPaused = false;
				ContinuePatrol();
			}
			return;
		}

		if (_isAlerted)
		{
			if (Time.time >= _alertEndTime)
			{
				_isAlerted = false;
				StartChase(); 
			}
			return;
		}

		UpdateFacingDirection();
		HandlePlayerDetection();
		HandleMovement();
	}

	private void UpdateFacingDirection()
	{
		if (_agent.velocity.magnitude > 0.1f)
		{
			Vector2 movementDir = new Vector2(_agent.velocity.x, _agent.velocity.y).normalized;
			if (movementDir.magnitude > 0.5f)
			{
				_currentFacingDirection = movementDir;
				if (movementDir.x > 0.1f)
				{
					transform.localScale = new Vector3(Mathf.Abs(_originalScale.x), _originalScale.y, _originalScale.z);
				}
				else if (movementDir.x < -0.1f)
				{
					transform.localScale = new Vector3(-Mathf.Abs(_originalScale.x), _originalScale.y, _originalScale.z);
				}
			}
		}
	}

	private void UpdateAnimationState(string newState)
	{
		if (_currentAnimationState == newState) return;

		_currentAnimationState = newState;
		_animator.CrossFade(newState, 0.9f); 
	}

	private void HandlePlayerDetection()
	{
		if (player == null) return;

		bool detected = CanSeePlayer() || CanHearPlayer();

		if (detected)
		{
			if (!_isChasing && !_isAlerted)
			{
				StartAlert();
			}
			_playerLostTime = Time.time;
		}
		else if (_isChasing && Time.time - _playerLostTime > returnDelay)
		{
			ReturnToPath();
		}
	}

	private void StartAlert()
	{
		_isAlerted = true;
		_alertEndTime = Time.time + alertDuration;
		_lastWaypointPosition = waypoint.CurrentPosition + waypoint.Points[_currentWaypointIndex];
		_agent.speed = 0; 
		_agent.isStopped = true;
		UpdateAnimationState("Alert");
	}

	private bool CanSeePlayer()
	{
		if (player == null) return false;

		Vector2 directionToPlayer = (Vector2)(player.position - transform.position);
		float distanceToPlayer = directionToPlayer.magnitude;

		if (player.CompareTag("Player") && distanceToPlayer <= visionDistance)
		{
			float angleToPlayer = Vector2.Angle(_currentFacingDirection, directionToPlayer);
			if (angleToPlayer > visionAngle * 0.5f) return false;

			RaycastHit2D hit = Physics2D.Raycast(
				transform.position,
				directionToPlayer.normalized,
				distanceToPlayer,
				visionBlockingLayers
			);

			return !hit.collider || hit.collider.transform == player;
		}
		return false;
	}

	private void HandleMovement()
	{
		if (_isChasing)
		{
			ChasePlayer();
		}
		else
		{
			PatrolWaypoints();
		}
	}

	private void StartChase()
	{
		_isChasing = true;
		_agent.isStopped = false; 
		_agent.speed = chaseSpeed;
		UpdateAnimationState("Move");
	}

	private void ChasePlayer()
	{
		if (player != null)
		{
			_agent.SetDestination(player.position);
		}
	}

	private void ReturnToPath()
	{
		_isChasing = false;
		_agent.speed = patrolSpeed;
		_agent.SetDestination(_lastWaypointPosition);
		UpdateAnimationState("Move");
	}

	private void PatrolWaypoints()
	{
		Vector3 targetPos = waypoint.CurrentPosition + waypoint.Points[_currentWaypointIndex];

		if (!_agent.pathPending && _agent.remainingDistance <= reachThreshold)
		{
			bool isFirstWaypoint = (_currentWaypointIndex == 0);
			bool isLastWaypoint = (_currentWaypointIndex == waypoint.Points.Length - 1);

			if (isFirstWaypoint && pauseAtFirstWaypoint > 0)
			{
				PauseAtWaypoint(pauseAtFirstWaypoint);
				return;
			}
			else if (isLastWaypoint && pauseAtLastWaypoint > 0)
			{
				PauseAtWaypoint(pauseAtLastWaypoint);
				return;
			}

			UpdateWaypointIndex();
			_agent.SetDestination(waypoint.CurrentPosition + waypoint.Points[_currentWaypointIndex]);
		}
	}

	private void PauseAtWaypoint(float pauseTime)
	{
		_isPaused = true;
		_pauseEndTime = Time.time + pauseTime;
		_agent.isStopped = true;
		UpdateAnimationState("Look");
	}

	private void ContinuePatrol()
	{
		_agent.isStopped = false;
		UpdateWaypointIndex();
		_agent.SetDestination(waypoint.CurrentPosition + waypoint.Points[_currentWaypointIndex]);
		UpdateAnimationState("Move");
	}

	private void UpdateWaypointIndex()
	{
		if (_movingForward)
		{
			if (_currentWaypointIndex < waypoint.Points.Length - 1)
			{
				_currentWaypointIndex++;
			}
			else
			{
				_movingForward = false;
				_currentWaypointIndex--;
			}
		}
		else
		{
			if (_currentWaypointIndex > 0)
			{
				_currentWaypointIndex--;
			}
			else
			{
				_movingForward = true;
				_currentWaypointIndex++;
			}
		}
	}

	private bool CanHearPlayer()
	{
		if (player.GetComponent<Rigidbody2D>().linearVelocity.magnitude < 0.1f)
			return false;

		float distanceToPlayer = Vector2.Distance(transform.position, player.position);
		if (distanceToPlayer > visionDistance) return false;

		float hearingDistance = visionDistance * 0.7f;
		if (distanceToPlayer > hearingDistance) return false;

		RaycastHit2D hit = Physics2D.Raycast(
			transform.position,
			(player.position - transform.position).normalized,
			distanceToPlayer,
			visionBlockingLayers
		);

		return hit.collider == null || hit.collider.transform == player;
	}

	private void OnDrawGizmosSelected()
	{
		Vector2 coneLeft = Quaternion.Euler(0, 0, visionAngle * 0.5f) * _currentFacingDirection * visionDistance;
		Vector2 coneRight = Quaternion.Euler(0, 0, -visionAngle * 0.5f) * _currentFacingDirection * visionDistance;

		Gizmos.color = new Color(1, 1, 0, 0.3f);
		Gizmos.DrawLine(transform.position, (Vector2)transform.position + coneLeft);
		Gizmos.DrawLine(transform.position, (Vector2)transform.position + coneRight);
		Gizmos.DrawLine((Vector2)transform.position + coneLeft, (Vector2)transform.position + coneRight);

		Gizmos.color = Color.red;
		Gizmos.DrawLine(transform.position, (Vector2)transform.position + _currentFacingDirection * 1.5f);

		if (waypoint != null && waypoint.Points.Length > 0)
		{
			Gizmos.color = Color.green;
			for (int i = 0; i < waypoint.Points.Length; i++)
			{
				Vector3 point = waypoint.CurrentPosition + waypoint.Points[i];
				Gizmos.DrawSphere(point, 0.1f);
				if (i < waypoint.Points.Length - 1)
				{
					Gizmos.DrawLine(point, waypoint.CurrentPosition + waypoint.Points[i + 1]);
				}
			}
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Player"))
		{
			player = other.transform;
			if (!_isChasing && !_isAlerted)
			{
				StartAlert();
			}
			_playerLostTime = Time.time;
		}
	}
}