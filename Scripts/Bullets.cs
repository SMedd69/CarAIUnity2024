using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullets : MonoBehaviour
{
    public enum BulletType { Simple, Explosive }
    public BulletType bulletType = BulletType.Explosive;

    public float force = 10f; // Force pour une balle simple
    public float explosionRadius = 5f; // Rayon d'explosion pour une balle explosive
    public float explosionForce = 500f; // Force de l'explosion pour une balle explosive

    private void OnCollisionEnter(Collision collision)
    {
        if (bulletType == BulletType.Simple)
        {
            ApplySimpleImpact(collision);
        }
        else if (bulletType == BulletType.Explosive)
        {
            Explode();
        }

        // Détruire la balle après impact
        Destroy(gameObject);
    }

    // Appliquer une force simple à l'objet touché
    void ApplySimpleImpact(Collision collision)
    {
        Rigidbody rb = collision.collider.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Appliquer une force dans la direction de la balle
            Vector3 impactDirection = collision.contacts[0].normal;
            rb.AddForce(-impactDirection * force, ForceMode.Impulse);
        }
        if(collision.gameObject.tag == "Building")
        {
            Rigidbody rbAdd = collision.gameObject.AddComponent<Rigidbody>();

            // Appliquer une force dans la direction de la balle
            Vector3 impactDirection = collision.contacts[0].normal;
            rbAdd.AddForce(-impactDirection * force, ForceMode.Impulse);
        }
    }

    // Explosion et impact sur les objets alentour
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

    // Optionnel : Visualiser le rayon d'explosion dans l'éditeur Unity
    private void OnDrawGizmosSelected()
    {
        if (bulletType == BulletType.Explosive)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}
