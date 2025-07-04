using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator _animator;
    private bool _isGrabbing => PlayerManager.Instance.SelectItem != null;
    private bool _isMoving => InputManager.Instance.MoveDir != Vector3.zero;

    private void Update()
    {
        PlayAnimation();
    }

    private void PlayAnimation()
    {
        MoveAnim();
        GrabAnim();
        SwingAnim();
        MiningAnim();
        SetAkimbo();
        SetInAir();
    }

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    public void MoveAnim()
    {
        _animator.SetBool("IsWalking", _isMoving);
    }

    public void GrabAnim()
    {
        _animator.SetBool("IsGrabbing", _isGrabbing);
    }

    public void SwingAnim()
    {
        // 나중엔 _isGrabbing도 조건에 추가되도록 변경
        if (Input.GetMouseButtonDown(0) && _isGrabbing && PlayerManager.Instance.SelectItem as MaterialItem)
        {
            _animator.SetTrigger("Swing");
        }
    }

    public void SetAkimbo()
    {
        _animator.SetBool("IsAkimbo", PlayerManager.Instance.IsAkimbo);
    }

    private void SetInAir()
    {
        _animator.SetBool("InAir", !PlayerManager.Instance.Player.Controller.isGrounded);
    }


    public void MiningAnim()
    {
        _animator.SetBool("IsMining", InputManager.Instance.IsUsingTool);
    }
}
