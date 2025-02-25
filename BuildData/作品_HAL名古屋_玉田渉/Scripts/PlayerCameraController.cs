using Unity.Cinemachine;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] Transform player;                  // �v���C���[��Transform
    [SerializeField] float distance;                    // �J�����ƃv���C���[�̋���
    [SerializeField] float rotationSpeed;               // �}�E�X���͂ɉ�������]���x
    [SerializeField] float lookAtHeightOffset = 2.0f;   // �v���C���[�̓��̍����ɍ��킹�邽�߂̃I�t�Z�b�g         
    [SerializeField] float minVerticalAngle = -20.0f;   // ������]�̉���
    [SerializeField] float maxVerticalAngle = 60.0f;    // ������]�̏��

    float currentPitch = 0.0f;  // ���������̉�]�p�x
    float currentYaw = 0.0f;    // ���������̉�]�p�x

    void Start()
    {
        // �J�����̏����ʒu��ݒ�
        Vector3 initialPosition = player.position - transform.forward * distance;
        transform.position = initialPosition;

        // �v���C���[�𒍎�
        UpdateLookAt();
    }

    void LateUpdate()
    {
        // �}�E�X���͂��擾
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // ���������̉�]�iYaw�j
        currentYaw += mouseX * rotationSpeed;

        // ���������̉�]�iPitch�j�ŁA�p�x������K�p
        currentPitch -= mouseY * rotationSpeed;
        currentPitch = Mathf.Clamp(currentPitch, minVerticalAngle, maxVerticalAngle);

        // �J�����̈ʒu���v�Z���Đݒ�
        UpdateCameraPosition();

    }

    void UpdateCameraPosition()
    {
        // ��]���v�Z���ăJ�����̈ʒu���v���C���[���S�Őݒ�
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0);
        Vector3 offset = rotation * Vector3.back * distance;
        transform.position = player.position + offset;

        // �v���C���[�̓��̂�����𒍎�
        UpdateLookAt();
    }

    void UpdateLookAt()
    {
        // �����_���v���C���[�̈ʒu�ɍ����I�t�Z�b�g�������Đݒ�
        Vector3 lookAtPosition = player.position + Vector3.up * lookAtHeightOffset;
        transform.LookAt(lookAtPosition);
    }
}
