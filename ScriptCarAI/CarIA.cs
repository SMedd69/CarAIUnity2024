using UnityEngine;

public class CarIA : MonoBehaviour
{
    [Header("Paths Params")]
    public float t = 0.0f;
    public int curveResolution = 10;

    [Header("Vehicle Params")]
    public float maxSteer = 15f;
    public float speedLerpSteer = 5f;
    public float distFromPath = 5f;
    public float maxTorque = 20f;
    public float topSpeed = 150f;
    public float decellerationSpeed = 10;
    public float currentSpeed;
    public int currentPathObj;

    [Header("\nBrake params")]
    public Renderer[] breakingMesh;
    public Material idleBreakingLight;
    public Material activeBreakingLight;
    public bool isBreaking;

    [Header("\nSensor Params")]
    public float sensorLength = 5f;
    public float sensorBreakingLength = 2.5f;
    public float decellerationSpeedSensor = 100f;
    public float intensityBrakeSensor = 1.5f;
    public float sidewaySensorLength = 2f;
    public float frontSensorStartPoint = 0f;
    public float frontSensorHeight = 0f;  // Ajout de la hauteur du capteur frontal
    public float frontSensorSideDist = 0f;
    public float frontSensorsAngle = 30f;
    public float sensitivityFrontSensor = 1f;
    public float sensitivityFrontStraightSensor = 2f;
    public float sensitivityFrontAngleSensor = 1.5f;
    public float sensitivitySidewaySensor = 0.5f;
    public float avoidSpeed = 10f;
    [SerializeField] private int flag;

    [Header("\nReversing Params")]
    public float maxTorqueReverse;
    public bool reversing = false;
    public float reverCounter = 0.0f;
    public float waitToReverse = 2.0f;
    public float reverFor = 1.5f;

    [Header("\nRespawn Params")]
    public bool canRespawn = true;
    public float respawnWait = 5.0f;
    public float respawnCounter = 0.0f;


    [Header("\nCENTER OF MASS")]
    public Vector3 CenterOfMass;

    [Header("\nBooleans Params")]
    public bool isMoveToPaths;
    public bool inSelector;

    [Header("\n\n\nReferences")]
    public Transform pathGroup;
    public WheelCollider WFL;
    public WheelCollider WFR;
    public WheelCollider WL;
    public WheelCollider WR;
    Transform[] path;
    Rigidbody rb;

    void Start()
    {
        isMoveToPaths = true;
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = CenterOfMass;
        GetPath();    
    }

    void GetPath()
    {
        // Obtenir tous les enfants du GameObject (ce script est sur le parent)
        Transform[] path_objs = pathGroup.GetComponentsInChildren<Transform>();

        // Filtrer pour exclure le transform du parent lui-même
        path = new Transform[path_objs.Length - 1]; // On ne compte pas le parent lui-même
        int index = 0;

        for (int i = 0; i < path_objs.Length; i++)
        {
            if (path_objs[i] != pathGroup) // Ignorer le parent lui-même
            {
                path[index] = path_objs[i];
                index++;
            }
        }
    }

    void Update()
    {
        if(flag == 0) GetSteer();
        Move();
        BreakingEffect();
        Sensors();
    }

    void GetSteer()
    {
        Vector3 p0 = path[(currentPathObj - 1 + path.Length) % path.Length].position;
        Vector3 p1 = path[currentPathObj].position;
        Vector3 p2 = path[(currentPathObj + 1) % path.Length].position;
        Vector3 p3 = path[(currentPathObj + 2) % path.Length].position;

        Vector3 steerTarget = GetCatmullRomPosition(t, p0, p1, p2, p3);

        // Calculer la direction vers la position interpolée
        Vector3 steerVector = transform.InverseTransformPoint(new Vector3(steerTarget.x, transform.position.y, steerTarget.z));
        float newSteer = maxSteer * (steerVector.x / steerVector.magnitude);

        // Appliquer une interpolation pour éviter les oscillations
        float clampedSteer = Mathf.Clamp(newSteer, -maxSteer, maxSteer);
        WFL.steerAngle = Mathf.Lerp(WFL.steerAngle, clampedSteer, Time.deltaTime * speedLerpSteer);
        WFR.steerAngle = Mathf.Lerp(WFR.steerAngle, clampedSteer, Time.deltaTime * speedLerpSteer);

        // Avancer le long de la courbe
        t += Mathf.Clamp(Time.deltaTime / (curveResolution * 2), 0, 1);

        if (Vector3.Distance(transform.position, path[currentPathObj].position) < distFromPath)
        {
            t = 0f;
            currentPathObj++;
            if (currentPathObj >= path.Length)
            {
                currentPathObj = 0;
            }
        }
    }


    Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        // Catmull-Rom spline (équation paramétrique)
        Vector3 a = 2f * p1;
        Vector3 b = p2 - p0;
        Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
        Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;

        Vector3 pos = 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));
        return pos;
    }

    void Move()
    {
        if(!isMoveToPaths) return;
        currentSpeed = 2 * Mathf.PI * WL.radius * WL.rpm * 60 / 1000;
        currentSpeed = Mathf.Round(currentSpeed);

        if(currentSpeed <= topSpeed && !inSelector)
        {
            if(!reversing)
            {
                WL.motorTorque = maxTorque;
                WR.motorTorque = maxTorque;
            }
            else
            {
                WL.motorTorque = -maxTorqueReverse;
                WR.motorTorque = -maxTorqueReverse;
            }

            WL.brakeTorque = 0;
            WR.brakeTorque = 0;
        }
        else if(!inSelector)
        {
            WL.motorTorque = 0.0f;
            WR.motorTorque = 0.0f;
            WL.brakeTorque = decellerationSpeed;
            WR.brakeTorque = decellerationSpeed;
        }
    }

    void BreakingEffect()
    {
        foreach (Renderer breakingRenderer in breakingMesh)
        {
            if (isBreaking)
            {
                breakingRenderer.material = activeBreakingLight;
            }
            else
            {
                breakingRenderer.material = idleBreakingLight;
            }
            
        }
    }

    void Sensors()
    {
        flag = 0;
        float avoidSensitivity = 0.0f;

        Vector3 pos = transform.position + transform.forward * frontSensorStartPoint + transform.up * frontSensorHeight;

        RaycastHit hit;
        Vector3 angleRightSensor = Quaternion.AngleAxis(frontSensorsAngle, transform.up) * transform.forward;
        Vector3 angleLeftSensor = Quaternion.AngleAxis(-frontSensorsAngle, transform.up) * transform.forward;

        // Front Sensor - Central
        if (Physics.Raycast(pos, transform.forward, out hit, sensorBreakingLength))
        {
            if (hit.transform.tag != "Terrain")
            {
                flag++;
                ApplyBrakes();
                Debug.DrawLine(pos, hit.point, Color.red);

                // Ajouter ici le calcul de la largeur
                float obstacleWidth = MeasureObstacleWidth(hit);
                
                // Tourner vers le côté le plus proche pour éviter
                if (obstacleWidth > 0)
                {
                    if (hit.point.x > transform.position.x)
                    {
                        avoidSensitivity -= sensitivityFrontStraightSensor; // Tourner à gauche
                    }
                    else
                    {
                        avoidSensitivity += sensitivityFrontStraightSensor; // Tourner à droite
                    }
                }
            }
        }

        // Front Sensors (Right & Angled Right)
        pos += transform.right * frontSensorSideDist;
        if (Physics.Raycast(pos, transform.forward, out hit, sensorLength) || Physics.Raycast(pos, angleRightSensor, out hit, sensorLength))
        {
            if (hit.transform.tag != "Terrain")
            {
                flag++;
                avoidSensitivity -= sensitivityFrontStraightSensor;  // Évite vers la gauche
                Debug.DrawLine(pos, hit.point, Color.magenta);
            }
        }

        // Front Sensors (Left & Angled Left)
        pos = transform.position + transform.forward * frontSensorStartPoint;
        pos += transform.right * -frontSensorSideDist;
        if (Physics.Raycast(pos, transform.forward, out hit, sensorLength) || Physics.Raycast(pos, angleLeftSensor, out hit, sensorLength))
        {
            if (hit.transform.tag != "Terrain")
            {
                flag++;
                avoidSensitivity += sensitivityFrontStraightSensor;  // Évite vers la droite
                Debug.DrawLine(pos, hit.point, Color.magenta);
            }
        }

        // Side Sensors
        if (Physics.Raycast(transform.position, transform.right, out hit, sidewaySensorLength))
        {
            if (hit.transform.tag != "Terrain")
            {
                flag++;
                avoidSensitivity -= sensitivitySidewaySensor;  // Évite vers la gauche
                Debug.DrawLine(transform.position, hit.point, Color.magenta);
            }
        }
        if (Physics.Raycast(transform.position, -transform.right, out hit, sidewaySensorLength))
        {
            if (hit.transform.tag != "Terrain")
            {
                flag++;
                avoidSensitivity += sensitivitySidewaySensor;  // Évite vers la droite
                Debug.DrawLine(transform.position, hit.point, Color.magenta);
            }
        }

        // Si aucun obstacle n'est détecté, revenir au chemin
        if (flag == 0)
        {
            ResumePath();
        }
        else
        {
            AvoidSteer(avoidSensitivity);
        }

        // Gestion du recul si bloqué
        if (rb.velocity.magnitude < 2 && flag > 0)
        {
            reverCounter += Time.deltaTime;
            if (reverCounter >= waitToReverse)
            {
                reverCounter = 0.0f;
                reversing = true;
            }
        }
        else if (!reversing) 
        {
            reverCounter = 0.0f;
        }

        if (reversing)
        {
            ReverseMovement();
        }
    }

    void AvoidSteer(float sensitivity)
    {
        float targetAvoidAngle = Mathf.Clamp(avoidSpeed * sensitivity, -maxSteer, maxSteer);
        WFR.steerAngle = Mathf.Lerp(WFR.steerAngle, targetAvoidAngle, Time.deltaTime * speedLerpSteer);
        WFL.steerAngle = Mathf.Lerp(WFL.steerAngle, targetAvoidAngle, Time.deltaTime * speedLerpSteer);
    }

    void ResumePath()
    {
        Vector3 pathTarget = path[currentPathObj].position;
        Vector3 steerVector = transform.InverseTransformPoint(new Vector3(pathTarget.x, transform.position.y, pathTarget.z));
        float pathSteer = maxSteer * (steerVector.x / steerVector.magnitude);

        // Lerp la direction pour revenir progressivement sur le chemin
        WFL.steerAngle = Mathf.Lerp(WFL.steerAngle, pathSteer, Time.deltaTime * speedLerpSteer);
        WFR.steerAngle = Mathf.Lerp(WFR.steerAngle, pathSteer, Time.deltaTime * speedLerpSteer);
    }

    void ApplyBrakes()
    {
        WL.brakeTorque = Mathf.Lerp(WL.brakeTorque, decellerationSpeedSensor, Time.deltaTime * intensityBrakeSensor);
        WR.brakeTorque = Mathf.Lerp(WR.brakeTorque, decellerationSpeedSensor, Time.deltaTime * intensityBrakeSensor);
        WFL.brakeTorque = Mathf.Lerp(WFL.brakeTorque, decellerationSpeedSensor, Time.deltaTime * intensityBrakeSensor);
        WFR.brakeTorque = Mathf.Lerp(WFR.brakeTorque, decellerationSpeedSensor, Time.deltaTime * intensityBrakeSensor);
    }

    void ReverseMovement()
    {
        WL.brakeTorque = 0.0f;
        WR.brakeTorque = 0.0f;
        WFL.brakeTorque = 0.0f;
        WFR.brakeTorque = 0.0f;

        // Reculer
        WL.motorTorque = -maxTorqueReverse;
        WR.motorTorque = -maxTorqueReverse;

        reverCounter += Time.deltaTime;
        if (reverCounter >= reverFor)
        {
            reverCounter = 0.0f;
            reversing = false;
        }
    }

    void Respawn()
    {
        if(!canRespawn) return;

        if(rb.velocity.magnitude < 2)
        {
            respawnCounter += Time.deltaTime;
            if(respawnCounter >= respawnWait)
            {
                if(currentPathObj == 0) transform.position = path[path.Length].position;
                else transform.position = path[currentPathObj - 1].position;
            }
        }
    }

    // Fonction pour mesurer la largeur de l'obstacle
    float MeasureObstacleWidth(RaycastHit hit)
    {
        // Lancer deux rayons latéraux pour mesurer la largeur
        RaycastHit hitLeft, hitRight;
        Vector3 leftPos = hit.point - transform.right * frontSensorSideDist;
        Vector3 rightPos = hit.point + transform.right * frontSensorSideDist;

        bool leftHit = Physics.Raycast(leftPos, transform.forward, out hitLeft, sensorBreakingLength);
        bool rightHit = Physics.Raycast(rightPos, transform.forward, out hitRight, sensorBreakingLength);

        // Si les deux côtés détectent l'objet, calculer la largeur
        if (leftHit && rightHit)
        {
            float width = Vector3.Distance(hitLeft.point, hitRight.point);
            Debug.DrawLine(hitLeft.point, hitRight.point, Color.green); // Visualiser la largeur
            return width;
        }

        // Si un seul côté est détecté, c'est que l'objet est plus petit que la distance du capteur latéral
        return 0f;
    }

    // DRAW GIZMOS
    void OnDrawGizmos()
    {
        // Gizmos pour la distance au chemin
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distFromPath);

        // Gizmos pour le centre de masse
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + CenterOfMass, .3f);
    }
}
