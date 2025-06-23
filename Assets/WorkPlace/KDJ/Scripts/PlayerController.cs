using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private CharacterController _controller;
    [SerializeField] private float _speed;
    [SerializeField] private CinemachineVirtualCamera _virCam;

    [Header("Mouse Config")]
    [SerializeField][Range(-90, 0)] private float _minPitch;
    [SerializeField][Range(0, 90)] private float _maxPitch;
    [SerializeField][Range(0, 5)] private float _mouseSensitivity = 1;

    private Vector3 _verVelocity;


    private void Update()
    {
        POVControl();
        Move(PlayerMoveInput());
        Jump();
    }

    private void Move(Vector3 moveDir)
    {
        // 카메라를 기준으로 정면을 잡고 움직이도록 수정해야함

    }

    private Vector3 PlayerMoveInput()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        return new Vector3(x, 0, y).normalized;
    }

    private Vector2 PlayerMouseInput()
    {
        float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity;
        return new Vector2(mouseX, mouseY);
    }

    private void Jump()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            _verVelocity.y = 5f; // Jump force
        }
    }

    private void POVControl()
    {
        Vector2 mouseInput = PlayerMouseInput();
        Vector3 eulerAngles = _virCam.transform.eulerAngles;
        eulerAngles.x -= mouseInput.y;
        eulerAngles.y += mouseInput.x;
        // Pitch 제한
        eulerAngles.x = Mathf.Clamp(eulerAngles.x, _minPitch, _maxPitch);
        _virCam.transform.eulerAngles = eulerAngles;
    }
}
