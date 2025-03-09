using UnityEngine;

public class CharacterOutline : MonoBehaviour
{
    [SerializeField] // プレイヤーのカメラ
    Camera playerCamera; 
    [SerializeField] // アウトラインを適用するオブジェクト
    Renderer characterRenderer; 

    Material material;

    void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main; // カメラが指定されていない場合は `MainCamera` を使用
        }
        material = characterRenderer.material;
    }

    void Update()
    {
        if (playerCamera != null)
        {
            material.SetVector("_CameraPosition", playerCamera.transform.position);
        }
    }

    /// <summary>
    /// シーン遷移時にカメラを切り替えるメソッド
    /// </summary>
    /// <param name="newCamera"></param>
    public void SetCamera(Camera newCamera)
    {
        playerCamera = newCamera;
    }
}