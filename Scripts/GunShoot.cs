using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GunShoot : MonoBehaviour
{
    public enum Mode { Rifle, LauncherMissile}
    public Mode mode = Mode.Rifle;

    [Header("MODE RIFLE PARAMS")]
    public float forceShoot = 10f;
    public float timeBetweenShot = 1f;

    [Header("MODE LAUNCHER PARAMS")]
    public float forceShootMissile = 3f;
    public float timeToLaunchMissile = .6f;
    public float timeBetweenShotMissile = 2f;
    public float maxDistanceRay = 200f;
    public float currentTime = 0f;

    [Header("UI PARAMS")]
    public RawImage aim;
    public Color colorInit;
    public Color colorLocked;

    [Header("REFERENCES")]
    public Transform bulletSpawn;
    public GameObject bulletPrefabs;
    public GameObject missilePrefabs;

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            mode = Mode.Rifle;
        }
        else if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            mode = Mode.LauncherMissile;

        }
        currentTime += Time.deltaTime;
        if(Input.GetButton("Fire1") && currentTime >= timeBetweenShot && mode == Mode.Rifle)
        {
            Shoot();
            currentTime = 0;
        }
        
        if(mode == Mode.LauncherMissile)
        {
            RaycastHit hit;
            if(Physics.Raycast(transform.position, transform.forward, out hit, maxDistanceRay))
            {
                aim.color = colorLocked;
                if(Input.GetButton("Fire1") && currentTime >= timeBetweenShotMissile)
                {
                    missilePrefabs.GetComponent<Missile>().SetTarget(hit.collider.gameObject);
                    Shoot();
                    currentTime = 0;
                }
            }
            else
            {
                aim.color = colorInit;
            }
        }
    }

    void Shoot()
    {
        if(mode == Mode.Rifle)
        {
            GameObject bullet = Instantiate(bulletPrefabs, bulletSpawn.position, bulletSpawn.rotation);
            if(bullet != null)
                bullet.GetComponent<Rigidbody>().AddForce(bulletSpawn.forward * forceShoot, ForceMode.Impulse);
        }
        
        else if(mode == Mode.LauncherMissile)
        {
            GameObject missile = Instantiate(missilePrefabs, bulletSpawn.position, bulletSpawn.rotation);
            missile.GetComponent<Missile>().enabled = false;
            if(missile != null)
            {
                missile.GetComponent<Rigidbody>().AddForce(bulletSpawn.forward * forceShootMissile, ForceMode.Impulse);
                StartCoroutine(StartMissile(missile.GetComponent<Missile>()));
            }

        }
        
    }

    IEnumerator StartMissile(Missile missile)
    {
        yield return new WaitForSeconds(timeToLaunchMissile);
        missile.enabled = true;
    }
}
