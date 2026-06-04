using UnityEngine;
using UnityEngine.InputSystem;
using SteeringCalcs;
using Globals;

public class Frog : MonoBehaviour
{
    //Bubble prefab for shooting the homing bubbles
    public GameObject bubblePrefab;

    // Frog status.
    public int Health;

    // Steering parameters.
    public float MaxSpeed;
    public float MaxAccel;
    public float AccelTime;

    // The arrival radius is set up to be dynamic, depending on how far away
    // the player right-clicks from the frog. See the logic in Update().
    public float ArrivePct;
    public float MinArriveRadius;
    public float MaxArriveRadius;
    private float _arriveRadius;
    private float _repathTimer = 0f;

    // Turn this off to make it easier to see overshooting when seek is used
    // instead of arrive.
    public bool HideFlagOnceReached;

    // References to various objects in the scene that we want to be able to modify.
    private Transform _flag;
    private SpriteRenderer _flagSr;
    private DrawGUI _drawGUIScript;
    private Animator _animator;
    private Rigidbody2D _rb;
    private InputAction ClickMoveAction;

    //Input action for shooting bubbles
    private InputAction shootBubbleAction;

    // Stores the last position that the player right-clicked. Initially null.
    private Vector2? _lastClickPos;

    //WEEK6
    //Used by DTs to make decision
    private Fly closestFly;
    private Snake closestSnake;
    private float distanceToClosestFly;
    private float distanceToClosestSnake;
    public float anchorWeight;
    public Vector2 AnchorDims;
    public bool AIControlled = false;
    public float scaredRange = 5f;
    public float huntRange = 12f;
    private Vector2? _dtLastTarget = null;  

    private Node[] _currentPath;

    private int _targetWaypointIndex;
    private float _dtTimer = 0f;
    public int FliesEaten;


    




    void Start()
    {
        // Initialise the various object references.
        _flag = GameObject.Find("Flag").transform;
        _flagSr = _flag.GetComponent<SpriteRenderer>();
        _flagSr.enabled = false;

        GameObject uiManager = GameObject.Find("UIManager");
        if (uiManager != null)
        {
            _drawGUIScript = uiManager.GetComponent<DrawGUI>();
        }

        _animator = GetComponent<Animator>();

        _rb = GetComponent<Rigidbody2D>();
        ClickMoveAction = InputSystem.actions.FindAction("Attack");
        shootBubbleAction = InputSystem.actions.FindAction("ShootBubble");

        _lastClickPos = null;
        _arriveRadius = MinArriveRadius;

    }

    void Update()
    {

        // Check if the player right-clicked (mouse button #1).
        if (!AIControlled && ClickMoveAction.WasPressedThisFrame())
        {
            _lastClickPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

            // Set the arrival radius dynamically.
            _arriveRadius = Mathf.Clamp(ArrivePct * ((Vector2)_lastClickPos - (Vector2)transform.position).magnitude, MinArriveRadius, MaxArriveRadius);

            _flag.position = (Vector2)_lastClickPos + new Vector2(0.55f, 0.55f);
            _flagSr.enabled = true;
        }
        else // show the relevant info about fly and snake
        {
            if (closestFly != null)
                Debug.DrawLine(transform.position, closestFly.transform.position, Color.black);
            if (closestSnake != null)
                Debug.DrawLine(transform.position, closestSnake.transform.position, Color.red);
        }

        if (shootBubbleAction.WasPressedThisFrame()) //If the space bar is pressed then instantiate a bubble and fire it in the direction the frog is facing
        {
            GameObject bubble = Instantiate(bubblePrefab, transform.position + transform.up * 1.0f, Quaternion.identity);
            bubble.GetComponent<Bubble>().fire(transform.up, 10f);
        }

    }

    void FixedUpdate()
    {   
        findClosestFly();
        findClosestSnake();

        _dtTimer += Time.fixedDeltaTime;
        if (AIControlled && _dtTimer > 0.25f) {
        RunDecisionTree();
        _dtTimer = 0f;
       }

        Vector2 desiredVel = decideMovement();
        Debug.DrawLine((Vector2)transform.position, (Vector2)transform.position + desiredVel, Color.blue);
        Vector2 steering = Steering.DesiredVelToForce(desiredVel, _rb, AccelTime, MaxAccel);
        _rb.AddForce(steering);

        UpdateAppearance();
    }

    private void UpdateAppearance()
    {
        if (_rb.linearVelocity.magnitude > Constants.MIN_SPEED_TO_ANIMATE)
        {
            _animator.SetBool("Walking", true);
            transform.up = _rb.linearVelocity;
        }
        else
        {
            _animator.SetBool("Walking", false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.tag.Equals("Spike"))
        {
            TakeDamage();
        }
    }

    public void CatchFly()
    {
    FliesEaten++;
    print("Frog caught a fly! Total: " + FliesEaten);
    }

    public void TakeDamage()
    {
        if (Health > 0)
        {
            Health--;
            print("Frog took damage, health is now " + Health);
        }
    }

    //TODO Implement the following Decision Tree
    // no health <= 0 --> set speed to 0 and color to red (1, 0.2, 0.2)
    // user clicked --> go to that click
    // nearby/outside of screen --> go towards screen (similar to flies)
    // closest snake nearby --> flee from snake within the screen
    // closest fly within screen --> go towards that fly
    // otherwise --> go to center of the screen

    //TODO SUGGESTED IMPROVEMENTS:
    //go to the center of mass of flies within screen
    //if 2 snake nearby -> freeze
    //Handle shooting bubbles
    //Come up with a better DT, for example: find flies that are within a circle around the frog that doesnt include any snake
    //Extra0 shoot bubble?
    //Extra1 update your code so that: 
    //Extra2 update your code with a better DT (find flies that are within a circle around the frog that doesnt include any snake)
    //Gameplay: tweak speed, range, acceleration and anchoring
    // private Vector2 decideMovement()
    // {
    //     if (_lastClickPos != null)
    //     {
    //         _repathTimer += Time.fixedDeltaTime;
    //         // Add the grid regeneration and timer reset
    //         if (_currentPath == null || _repathTimer > 0.5f) { 
    //             Pathfinding.grid.CreateGrid(); // Refreshes moving snake positions
    //             _currentPath = Pathfinding.RequestPath(transform.position, (Vector2)_lastClickPos);
    //             _targetWaypointIndex = 0;
    //             _repathTimer = 0f;
    //         }
    //         return getVelocityTowardsPath();
    //     }
    //     else
    //     {
    //         _currentPath = null;
    //         return Vector2.zero;
    //     }
    // }

    private Vector2 decideMovement()
    {
        if (_lastClickPos != null)
        {
            _repathTimer += Time.fixedDeltaTime;
            if (_currentPath == null || _repathTimer > 0.5f) { 
                
                if (closestSnake != null) 
                {
                    Collider2D snakeCollider = closestSnake.GetComponent<Collider2D>();
                    if (snakeCollider != null && Pathfinding.grid != null) 
                    {
                        Pathfinding.grid.UpdateGridRegion(snakeCollider.bounds);
                    }
                }

                _currentPath = Pathfinding.RequestPath(transform.position, (Vector2)_lastClickPos);
                _targetWaypointIndex = 0;
                _repathTimer = 0f;
            }
            return getVelocityTowardsPath();
        }
        else
        {
            _currentPath = null;
            return Vector2.zero;
        }
    }

    private Vector2 getVelocityTowardsPath()
    {
        if (_currentPath == null || _targetWaypointIndex >= _currentPath.Length) 
        {
            _lastClickPos = null;
            _dtLastTarget = null;
            if (HideFlagOnceReached) _flagSr.enabled = false;
            return Vector2.zero;
        }

        Vector2 targetPos = _currentPath[_targetWaypointIndex].worldPosition;
        float distToWaypoint = Vector2.Distance(transform.position, targetPos);

        if (distToWaypoint < Constants.TARGET_REACHED_TOLERANCE)
        {
            _targetWaypointIndex++;
            if (_targetWaypointIndex >= _currentPath.Length)
            {
                _currentPath = null;
                return Vector2.zero;
            }
            targetPos = _currentPath[_targetWaypointIndex].worldPosition;
        }

        if (_targetWaypointIndex == _currentPath.Length - 1)
            return Steering.ArriveDirect(transform.position, targetPos, _arriveRadius, MaxSpeed);
        else
            return Steering.SeekDirect(transform.position, targetPos, MaxSpeed);
    }

    private Vector2 getVelocityTowardsFlag()
    {
        Vector2 desiredVel = Vector2.zero;
        if (_lastClickPos != null)
        {
            if (((Vector2)_lastClickPos - (Vector2)gameObject.transform.position).magnitude > Constants.TARGET_REACHED_TOLERANCE)
            {
                desiredVel = Steering.ArriveDirect(gameObject.transform.position, (Vector2)_lastClickPos, _arriveRadius, MaxSpeed);
            }
            else
            {
                _lastClickPos = null;

                if (HideFlagOnceReached)
                {
                    _flagSr.enabled = false;
                }
            }

        }
        return desiredVel;
    }

    private void findClosestFly()
    {
        distanceToClosestFly = Mathf.Infinity;
        closestFly = null;

        foreach (Fly fly in (Fly[])GameObject.FindObjectsByType(typeof(Fly), FindObjectsSortMode.None))
        {
            float distanceToFly = (fly.transform.position - transform.position).magnitude;
            if (fly.GetComponent<Fly>().State != Fly.FlyState.Dead)
            {
                if (distanceToFly < distanceToClosestFly)
                {
                    closestFly = fly;
                    distanceToClosestFly = distanceToFly;

                }
            }

        }
    }

    //TODO See findClosestFly for inspiration
    private void findClosestSnake()
    {
        distanceToClosestSnake = Mathf.Infinity;
        closestSnake = null;

        foreach (Snake snake in (Snake[])GameObject.FindObjectsByType(typeof(Snake), FindObjectsSortMode.None))
        {
            float d = (snake.transform.position - transform.position).magnitude;
            if (d < distanceToClosestSnake)
            {
                distanceToClosestSnake = d;
                closestSnake = snake;
            }
        }
    }

    //TODO Check wether the current transform is out of screen (true) or not (false)
    private bool isOutOfScreen(Transform t)
    {
        if (Camera.main == null) return false;
        Vector3 vp = Camera.main.WorldToViewportPoint(t.position);
        return vp.x < 0.05f || vp.x > 0.95f || vp.y < 0.05f || vp.y > 0.95f;
    }


    private void RunDecisionTree()
    {
    // Node 1: No health
        if (Health <= 0)
        {
            _lastClickPos = null;
            _currentPath = null;
            GetComponent<SpriteRenderer>().color = new Color(1f, 0.2f, 0.2f);
            return;
        }

    // Node 2: Snake nearby (ROOT - chosen by Information Gain)
        bool snakeNearby = closestSnake != null && distanceToClosestSnake < scaredRange;

        if (snakeNearby)
        {
            // Node 3: Is snake attacking?
            bool snakeAttacking = closestSnake.State == Snake.SnakeState.Attack;

            if (snakeAttacking)
            {
                // LEAF: flee far away
                Vector2 away = ((Vector2)transform.position - (Vector2)closestSnake.transform.position).normalized;
                Vector2 fleeTarget = (Vector2)transform.position + away * scaredRange * 2f;
                fleeTarget.x = Mathf.Clamp(fleeTarget.x, -AnchorDims.x, AnchorDims.x);
                fleeTarget.y = Mathf.Clamp(fleeTarget.y, -AnchorDims.y, AnchorDims.y);
                SetDTTarget(fleeTarget);
            }
            else
            {
                // LEAF: move away a little
                Vector2 away = ((Vector2)transform.position - (Vector2)closestSnake.transform.position).normalized;
                Vector2 safeTarget = (Vector2)transform.position + away * scaredRange;
                safeTarget.x = Mathf.Clamp(safeTarget.x, -AnchorDims.x, AnchorDims.x);
                safeTarget.y = Mathf.Clamp(safeTarget.y, -AnchorDims.y, AnchorDims.y);
                SetDTTarget(safeTarget);
            }
        }
        else
        {
            // Node 3: Is fly nearby?
            bool flyNearby = closestFly != null && distanceToClosestFly < huntRange;

            if (flyNearby)
            {
                // LEAF: chase the fly
                SetDTTarget(closestFly.transform.position);
            }
            else
            {
                // Node 4: Out of screen?
                if (isOutOfScreen(transform))
                {
                    // LEAF: go to screen centre
                    SetDTTarget(GetScreenCenter());
                }
                else
                {
                // LEAF: go to centre
                    SetDTTarget(GetScreenCenter());
                }
            }
        }
    }

    private void SetDTTarget(Vector2 target, float threshold = 0.5f)
    {
        if (_dtLastTarget == null ||
            Vector2.Distance(_dtLastTarget.Value, target) > threshold)
        {
            _dtLastTarget = target;
            _lastClickPos = target;

            float dist = Vector2.Distance(transform.position, target);
            _arriveRadius = Mathf.Clamp(
                ArrivePct * dist, MinArriveRadius, MaxArriveRadius);

            _flag.position = (Vector3)target + new Vector3(0.55f, 0.55f, 0f);
            _flagSr.enabled = true;

            _currentPath = null;
            _repathTimer = 0f;
        }
    }

    private Vector2 GetScreenCenter()
    {
        if (Camera.main != null)
        {
            return new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.y);
        }
        return Vector2.zero; // Fallback
    }
    
    private void OnDrawGizmos()
    {
        // Only draw if there's actually a path
        if (_currentPath != null && _currentPath.Length > 0)
        {
            Gizmos.color = Color.black; 

            for (int i = _targetWaypointIndex; i < _currentPath.Length; i++)
            {
                Gizmos.DrawCube(_currentPath[i].worldPosition, Vector3.one * 0.3f);

                //line connecting the waypoints
                if (i == _targetWaypointIndex)
                {
                    Gizmos.DrawLine(transform.position, _currentPath[i].worldPosition);
                }
                else
                {
                    Gizmos.DrawLine(_currentPath[i - 1].worldPosition, _currentPath[i].worldPosition);
                }
            }
        }
    }



}
