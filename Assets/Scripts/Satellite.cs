using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Satellite : MonoBehaviour {
    [SerializeField] public GameObject shieldObjectToCreate;
    [SerializeField] public GameObject missileObjectToCreate;
    [SerializeField] public uint nrOfMissiles;
    [SerializeField] public float missileSpeed;
    [SerializeField] public float missileTurningSpeed;
    [SerializeField] public float missileTimeout;
    [SerializeField] public float explosionForce;
    [SerializeField] public Environment environment;
    [SerializeField] public Position position;
    [SerializeField] public AntiPosition antiPosition;
    [SerializeField] public Objective objective;

    // Selectors
    [SerializeField] public AbilitySelector cameraSelector;
    [SerializeField] public AbilitySelector shieldSelector;
    [SerializeField] public AbilitySelector missileSelector;
    private AbilitySelector currentSelector;

    // Other UI elements
    [SerializeField] public ObjectiveArrow objectiveArrow;
    [SerializeField] public Image objectiveCircle;
    [SerializeField] public CameraFlash cameraFlash;
    [SerializeField] public Text levelText;

    bool rotating = false;

    interface Ability {
        void Activate();

        void Deactivate();

        void Use();

        void Sync();

        void Passive();

        AbilitySelector GetSelector();
    }

    class Shield : Ability {
        private GameObject shield;
        private AbilitySelector selector;

        public Shield(GameObject shieldObject, AbilitySelector selector) {
            this.shield = Instantiate(shieldObject, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
            this.shield.SetActive(false);
            this.selector = selector;
        }

        public void Activate() {
            this.shield.SetActive(true);
        }

        public void Deactivate() {
            this.shield.SetActive(false);
        }

        public void Use() {
            return;
        }

        public void Sync() {
            return;
        }

        public void Passive() {
            return;
        }

        public AbilitySelector GetSelector() {
            return this.selector;
        }
    }

    class Rocket : Ability {
        private Queue<GameObject> missilePool;
        private HashSet<GameObject> activePool;
        private Camera camera;
        private AbilitySelector selector;

        private void Destroy(GameObject missile) {
            // Disable
            missile.SetActive(false);

            // Remove from active pool and add to item queue
            activePool.Remove(missile);
            missilePool.Enqueue(missile);
        }

        private Vector3 GetTarget() {
            Vector3 mpos = Input.mousePosition;
            return this.camera.ScreenToWorldPoint(mpos);
        }

        public Rocket(GameObject missileObject, uint nrOfMissiles, float speed, float turningSpeed,
                      float maxDist, Position position, AntiPosition antiPosition, Camera camera,
                      AbilitySelector selector, float timeout, float explosionForce) {
            // Create containers
            this.missilePool = new Queue<GameObject>();
            this.activePool = new HashSet<GameObject>();

            // Internal state
            this.camera = camera;
            this.selector = selector;

            // Fill the item pool
            for (uint i = 0; i < nrOfMissiles; ++i) {
                GameObject rocket = Instantiate(missileObject, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
                rocket.SetActive(false);

                Missile missile = rocket.GetComponent<Missile>();
                missile.speed = speed;
                missile.turningSpeed = turningSpeed;
                missile.onDestroy = this.Destroy;
                missile.maxDistance = maxDist;
                missile.position = position;
                missile.timeout = timeout;
                missile.explosionForce = explosionForce;

                TrailShifter ts = rocket.GetComponentInChildren<TrailShifter>();
                ts.SetSimulationSpace(antiPosition);

                missilePool.Enqueue(rocket);
            }
        }

        public void Activate() {
            return;
        }

        public void Deactivate() {
            return;
        }

        public void Use() {
            if (this.missilePool.Count > 0) {
                GameObject top = this.missilePool.Dequeue();

                Missile missile = top.GetComponent<Missile>();

                Vector3 target = this.GetTarget();
                missile.Seek(target);
                missile.transform.position = new Vector3(target.x, target.y, 0f).normalized * 2f;
                top.SetActive(true);
                missile.Activate();

                // Add to the list of active missiles
                this.activePool.Add(top);
            }
        }

        public void Sync() {
            foreach (GameObject gm in this.activePool) {
                Missile missile = gm.GetComponent<Missile>();

                // TODO: seek the nearest object
                missile.Seek(this.GetTarget());
            }
        }

        public void Passive() {
            return;
        }

        public AbilitySelector GetSelector() {
            return this.selector;
        }
    }

    class SpaceCamera : Ability {
        private Objective objective;
        private AbilitySelector selector;
        private ObjectiveArrow objectiveArrow;
        private Image objectiveCircle;
        private Camera camera;
        private CameraFlash cameraFlash;
        private SceneChanger sceneChanger;
        private Text level;

        private bool isDone = false;

        public SpaceCamera(Objective objective, AbilitySelector selector, ObjectiveArrow objectiveArrow, Image objectiveCircle,
                           Camera camera, CameraFlash cameraFlash, SceneChanger sceneChanger, Text level) {
            this.objective = objective;
            this.selector = selector;
            this.objectiveArrow = objectiveArrow;
            this.objectiveCircle = objectiveCircle;
            this.camera = camera;
            this.cameraFlash = cameraFlash;
            this.sceneChanger = sceneChanger;
            this.level = level;

            // By default, off
            this.objectiveCircle.gameObject.SetActive(false);
            this.objectiveArrow.gameObject.SetActive(false);
        }

        public void Activate() {

        }

        public void Deactivate() {

        }

        public void Use() {
            if (this.objective.IsAchievable()) {
                this.cameraFlash.Flash();

                // Change UI
                this.objectiveCircle.gameObject.SetActive(false);
                this.objectiveArrow.gameObject.SetActive(false);

                // Next level
                this.objective.Start();

                // Increment level
                int currentLevel = Int32.Parse(this.level.text);
                currentLevel += 1;
                this.level.text = currentLevel.ToString();
            }
        }

        public void Sync() {
            return;
        }

        public void Passive() {
            if (this.isDone)
                return;

            if (!this.objective.IsDiscovered())
                return;

            this.objectiveArrow.gameObject.SetActive(true);
            this.objectiveCircle.gameObject.SetActive(true);

            // Get the position to the objective
            Vector3 target = this.objective.GetPosition();
            Vector3 screenPoint = this.camera.WorldToScreenPoint(target);

            // Set the position of the circle
            this.objectiveCircle.transform.position = screenPoint;

            // Set the arrow position
            this.objectiveArrow.Distance(screenPoint);
            this.objectiveArrow.PointTo(target);
        }

        public AbilitySelector GetSelector() {
            return this.selector;
        }
    }

    // Using enum during development
    enum Side : uint {
        SIDE1,
        SIDE2,
        SHIELD,
        MISSILE,
        CAMERA,
        SIDE6,
        NONE
    };

    Dictionary<Side, Ability> sideAbility = new Dictionary<Side, Ability>();

    private Side side = 0;
    private Side activeSide = Side.NONE;

    // Start is called before the first frame update
    void Start() {
        sideAbility.Add(Side.SIDE1, null);
        sideAbility.Add(Side.SIDE2, null);
        sideAbility.Add(Side.SHIELD, new Shield(shieldObjectToCreate, shieldSelector));
        sideAbility.Add(Side.MISSILE, new Rocket(missileObjectToCreate, nrOfMissiles, missileSpeed,
                                                 missileTurningSpeed, environment.maxDistance, position,
                                                 antiPosition, Camera.main, missileSelector, missileTimeout, this.explosionForce));
        sideAbility.Add(Side.CAMERA, new SpaceCamera(this.objective, this.cameraSelector, this.objectiveArrow, this.objectiveCircle,
                                                     Camera.main, this.cameraFlash, gameObject.GetComponent<SceneChanger>(), this.levelText));
        sideAbility.Add(Side.SIDE6, null);
        sideAbility.Add(Side.NONE, null);
    }

    void DeactivateAbility() {
        Ability a = sideAbility[this.activeSide];

        if (a != null) {
            a.Deactivate();
        }

        this.activeSide = Side.NONE;

        // UI
        if (this.currentSelector != null)
            this.currentSelector.Deselect();
    }

    void ActivateAbility(Side side) {
        Ability ability = sideAbility[side];

        if (ability != null) {
            ability.Activate();

            // UI
            this.currentSelector = ability.GetSelector();
            this.currentSelector.Select();
        }

        this.activeSide = side;
    }

    // Update is called once per frame
    void Update() {
        // Do stuff when an ability is active
        if (this.sideAbility[this.activeSide] != null)
            this.sideAbility[this.activeSide].Sync();


        // Passive abilities
        foreach (Ability a in this.sideAbility.Values) {
            if (a == null)
                continue;

            a.Passive();
        }

        if (!this.rotating) {
            if (Input.GetKeyDown("q")) {
                this.side = Side.SIDE1;
                StartCoroutine("RotateTo");
            }

            if (Input.GetKeyDown("e")) {
                this.side = Side.SIDE2;
                StartCoroutine("RotateTo");
            }

            if (Input.GetKeyDown("1")) {
                if (this.activeSide == Side.CAMERA) {
                    this.sideAbility[Side.CAMERA].Use();
                }

                else {
                    this.side = Side.CAMERA;
                    StartCoroutine("RotateTo");
                }
            }

            if (Input.GetKeyDown("2")) {
                if (this.activeSide == Side.SHIELD) {
                    this.sideAbility[Side.SHIELD].Use();
                }

                else {
                    this.side = Side.SHIELD;
                    StartCoroutine("RotateTo");
                }
            }

            if (Input.GetKeyDown("3")) {
                if (this.activeSide == Side.MISSILE) {
                    this.sideAbility[Side.MISSILE].Use();
                }

                else {
                    this.side = Side.MISSILE;
                    StartCoroutine("RotateTo");
                }
            }

            if (Input.GetKeyDown("r")) {
                this.side += 1;

                // Clamp
                if (this.side == Side.SIDE6)
                    this.side = Side.SIDE1;

                StartCoroutine("RotateTo");
            }
        }
    }

    readonly Vector3[] rotations = {
        new Vector3(0, 0, 0),
        new Vector3(90, 0, 0),
        new Vector3(180, 0, 0),
        new Vector3(270, 0, 0),
        new Vector3(0, 90, 0),
        new Vector3(0, -90, 0)
    };

    IEnumerator RotateTo() {
        // We are rotating now!
        this.rotating = true;

        // Get the current rotation
        Vector3 currentRotation = transform.eulerAngles;

        // Get the desired rotation
        Vector3 desiredRotation = rotations[(uint) this.side];

        // Deactivate current ability
        DeactivateAbility();

        // Stepper
        float step = 0.0f;

        // Rotate
        while (step < 1.0f) {
            step += 0.04f;
            transform.eulerAngles = Vector3.Lerp(currentRotation, desiredRotation, step);

            yield return new WaitForSeconds(0.01f);
        }

        // Make sure that the rotation is exact
        transform.eulerAngles = desiredRotation;

        // Update the active ability
        ActivateAbility(this.side);

        // The rotation has ended
        this.rotating = false;

        yield return null;
    }

    public void OnCollisionEnter(Collision collision) {
        if (collision.collider.name == "MissileCollider")
            Physics.IgnoreCollision(collision.collider, gameObject.GetComponent<BoxCollider>());

        else
            gameObject.GetComponent<SceneChanger>().ChangeScene();
    }
}
