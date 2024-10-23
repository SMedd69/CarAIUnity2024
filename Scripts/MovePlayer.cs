using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlayer : MonoBehaviour
{
    public float speed = 5;
    public float rotSpeed = 3;
    public float gravity = 9.1f;

    public Transform cameraPlayer;
    CharacterController characterController;
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float moveZ = Input.GetAxis("Vertical") * speed;
        float moveX = Input.GetAxis("Horizontal") * speed;

        Vector3 direction = (transform.forward * moveZ + transform.right * moveX) * Time.deltaTime;
        characterController.Move(direction);

        float rotH = Input.GetAxis("Mouse X") * rotSpeed;
        float rotV = Input.GetAxis("Mouse Y") * rotSpeed;

        Vector3 rot = new Vector3(0.0f, rotH);
        transform.Rotate(rot);
        
        Vector3 rotCam = new Vector3(-rotV, 0.0f);
        cameraPlayer.transform.Rotate(rotCam);

        if(!characterController.isGrounded)
        {
            Vector3 down = new Vector3(0.0f, -gravity * Time.deltaTime);
            characterController.Move(down);
        }
    }
}
