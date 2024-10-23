using System;
using UnityEngine;

public class Missile : MonoBehaviour {
    [Header("REFERENCES")] 
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private GameObject _target;
    [SerializeField] private GameObject _explosionPrefab;

    [Header("MOVEMENT")] 
    [SerializeField] private float _speed = 15;
    [SerializeField] private float _rotateSpeed = 95;

    [Header("PARAMS EXPLOSE")] 
    [SerializeField] private float explosionRadius;
    [SerializeField] private float explosionForce;
    [Header("PREDICTION")] 
    [SerializeField] private float _maxDistancePredict = 100;
    [SerializeField] private float _minDistancePredict = 5;
    [SerializeField] private float _maxTimePrediction = 5;
    private Vector3 _standardPrediction, _deviatedPrediction;

    [Header("DEVIATION")] 
    [SerializeField] private float _deviationAmount = 50;
    [SerializeField] private float _deviationSpeed = 2;

    public void SetTarget(GameObject target)
    {
        _target = target;
    }

    private void FixedUpdate() {
        _rb.velocity = transform.forward * _speed;

        var leadTimePercentage = Mathf.InverseLerp(_minDistancePredict, _maxDistancePredict, Vector3.Distance(transform.position, _target.transform.position));

        PredictMovement(leadTimePercentage);

        AddDeviation(leadTimePercentage);

        RotateRocket();
    }

    private void PredictMovement(float leadTimePercentage) {
        var predictionTime = Mathf.Lerp(0, _maxTimePrediction, leadTimePercentage);
        Rigidbody rbTarget = _target.GetComponent<Rigidbody>();
        Vector3 vel = (rbTarget != null) ? rbTarget.velocity : Vector3.zero;
        _standardPrediction = _target.transform.position + vel * predictionTime;
    }

    private void AddDeviation(float leadTimePercentage) {
        var deviation = new Vector3(Mathf.Cos(Time.time * _deviationSpeed), 0, 0);
        
        var predictionOffset = transform.TransformDirection(deviation) * _deviationAmount * leadTimePercentage;

        _deviatedPrediction = _standardPrediction + predictionOffset;
    }

    private void RotateRocket() {
        var heading = _deviatedPrediction - transform.position;

        var rotation = Quaternion.LookRotation(heading);
        _rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, rotation, _rotateSpeed * Time.deltaTime));
    }

    private void OnCollisionEnter(Collision collision) {
        if(_explosionPrefab) Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
        //if (collision.transform.TryGetComponent<IExplode>(out var ex)) ex.Explode();
        Explode();
        Destroy(gameObject);
    }

    void Explode()
    {
        // Trouver tous les objets dans le rayon d'explosion
        Collider[] objectsInRange = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider nearbyObject in objectsInRange)
        {
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Appliquer une force d'explosion aux objets avec un Rigidbody
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }
            else if(nearbyObject.tag == "Building")
            {
                Rigidbody rbAdd = nearbyObject.gameObject.AddComponent<Rigidbody>();
                
                // Appliquer une force d'explosion aux objets avec un Rigidbody
                rbAdd.AddExplosionForce(explosionForce, transform.position, explosionRadius);

                BrickRemoved brickRemoved = rbAdd.GetComponent<BrickRemoved>();

                if(brickRemoved != null)
                {
                    brickRemoved.StartReset();
                }
            }
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, _standardPrediction);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(_standardPrediction, _deviatedPrediction);
    }
}