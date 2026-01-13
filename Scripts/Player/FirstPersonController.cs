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
    [Header("Crouch")]
    [Tooltip("Stand height otomatik alınır . Bu oran crouch yüksekliğini belirler.")]
    [Range(0.3f, 095f)] public float crouchHeightRatio = 0.55f;

    [Tooltip("Stand cameraY otomatik alınır . Bu oran crouch kamera yüksekliğini belirler.")]
    [Range(0.3f, 098f)] public float crouchCameraRatio = 0.58f;

    public float crouchHeight = 1.5f; // Eğilince height
    public float standHeight = 1.89f; //Normal height //Buna dokunma !!!!!
    public float crouchSpeed = 2.5f; // Eğilince hız
    public float standSpeed = 5f; // Normal hız
    public float crouchCameraY = 0.8f; // Eğilince kamera local Y
    public float standCameraY = 0.9f; // normal karema Local Y
    bool isCrouching; // Eğiliyormu ? = 

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

        standSpeed = moveSpeed;

          // ✅ Stand değerlerini sahneden oku (senin kodunla aynı mantık)
        standHeight = controller.height;
        standCameraY = cam.localPosition.y;

        // ✅ Crouch değerlerini stand değerlerden oranla üret
        crouchHeight = standHeight * crouchHeightRatio;
        crouchCameraY = standCameraY * crouchCameraRatio;
        controller.center = new Vector3(0, standHeight / 2f, 0);
        
    }

    // Update is called once per frame
    void Update()
    {
        Look();
        Move();

        HandleCrouch();
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
        if(Input.GetKeyDown(KeyCode.Space) && isGrounded && !isCrouching)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        //Yerçekimi 
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleCrouch()
    {
        //Basılı tutarak eğilme 
        bool wantsCrouch = Input.GetKey(KeyCode.LeftControl);

        if(wantsCrouch && !isCrouching)
        {
            //Eğil
            isCrouching =true;

            controller.height = crouchHeight;
            controller.center = new Vector3(0, crouchHeight / 2f, 0);

            cam.localPosition = new Vector3(
                cam.localPosition.x,
                crouchCameraY,
                cam.localPosition.z
            );

            moveSpeed = crouchSpeed;
        }

        else if (!wantsCrouch && isCrouching)
        {
            // AYAĞA KALK
            isCrouching = false;

            controller.height = standHeight;
            controller.center = new Vector3(0, standHeight / 2f, 0);

            cam.localPosition = new Vector3(
                cam.localPosition.x,
                standCameraY,
                cam.localPosition.z
            );

            moveSpeed = standSpeed;
        }

    }
}
