using Unity.Cinemachine;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] Transform player;                  // プレイヤーのTransform
    [SerializeField] float distance;                    // カメラとプレイヤーの距離
    [SerializeField] float rotationSpeed;               // マウス入力に応じた回転速度
    [SerializeField] float lookAtHeightOffset = 2.0f;   // プレイヤーの頭の高さに合わせるためのオフセット         
    [SerializeField] float minVerticalAngle = -20.0f;   // 垂直回転の下限
    [SerializeField] float maxVerticalAngle = 60.0f;    // 垂直回転の上限

    float currentPitch = 0.0f;  // 垂直方向の回転角度
    float currentYaw = 0.0f;    // 水平方向の回転角度

    void Start()
    {
        // カメラの初期位置を設定
        Vector3 initialPosition = player.position - transform.forward * distance;
        transform.position = initialPosition;

        // プレイヤーを注視
        UpdateLookAt();
    }

    void LateUpdate()
    {
        // マウス入力を取得
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // 水平方向の回転（Yaw）
        currentYaw += mouseX * rotationSpeed;

        // 垂直方向の回転（Pitch）で、角度制限を適用
        currentPitch -= mouseY * rotationSpeed;
        currentPitch = Mathf.Clamp(currentPitch, minVerticalAngle, maxVerticalAngle);

        // カメラの位置を計算して設定
        UpdateCameraPosition();

    }

    void UpdateCameraPosition()
    {
        // 回転を計算してカメラの位置をプレイヤー中心で設定
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0);
        Vector3 offset = rotation * Vector3.back * distance;
        transform.position = player.position + offset;

        // プレイヤーの頭のあたりを注視
        UpdateLookAt();
    }

    void UpdateLookAt()
    {
        // 注視点をプレイヤーの位置に高さオフセットを加えて設定
        Vector3 lookAtPosition = player.position + Vector3.up * lookAtHeightOffset;
        transform.LookAt(lookAtPosition);
    }
}
