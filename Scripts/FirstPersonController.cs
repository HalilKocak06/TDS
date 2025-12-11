using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Video;

[RequireComponent(typeof(CharacterController))]


public class FirstPersonController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 100f;

    CharacterController controller;
    Transform cam;

    float xRotation = 0f;
    Vector3 velocity;
    bool isGrounded;
    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        cam = Camera.main.transform;

        //Mouse'u ekrana kilitle
        Cursor.lockState = CursorLockMode.Locked; //mouse kilitlenir.
        Cursor.visible = false; //mouse gözükmez.
        
    }

    // Update is called once per frame
    void Update()
    {
        Look();
        Move();
    }

    void Look()
    {
        //Mouse input kısmı
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f); 
        cam.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        //Sağa-Sola bakış
        transform.Rotate(Vector3.up * mouseX);
    }

    void Move()
    {
        //Yerde miyiz ?
        isGrounded = controller.isGrounded;
        if(isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f; //zemine yapıştırmak için küçük negatif değer.
        }

        //WASD input
        float x = Input.GetAxis("Horizontal"); //A-D
        float z = Input.GetAxis("Vertical"); //W-S

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * moveSpeed * Time.deltaTime);

        //ZIPLAMA
        if(Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        //Yerçekimi 
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
