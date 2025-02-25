using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class PrefabExporter : MonoBehaviour
{
    public GameObject prefab; // Inspectorでプレハブをセット

    [System.Serializable]
    class MaterialData
    {
        public string name;
        public Color color;
        public string texturePath;
    }

    [System.Serializable]
    class PrefabData
    {
        public string name;
        public List<string> components = new List<string>();
        public List<MaterialData> materials = new List<MaterialData>();
    }

    void Start()
    {
        if (prefab == null)
        {
            Debug.LogError("プレハブがセットされていません！");
            return;
        }

        PrefabData data = new PrefabData { name = prefab.name };

        // すべてのコンポーネント名を取得
        Component[] components = prefab.GetComponents<Component>();
        foreach (Component comp in components)
        {
            data.components.Add(comp.GetType().Name);
        }

        // マテリアル情報を取得（MeshRendererがある場合）
        MeshRenderer renderer = prefab.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            foreach (Material mat in renderer.sharedMaterials)
            {
                MaterialData matData = new MaterialData
                {
                    name = mat.name,
                    color = mat.HasProperty("_Color") ? mat.color : Color.white,
                    texturePath = mat.HasProperty("_MainTex") && mat.mainTexture != null ? mat.mainTexture.name : "None"
                };
                data.materials.Add(matData);
            }
        }

        // JSONに変換＆書き出し
        string json = JsonUtility.ToJson(data, true);
        string path = Application.dataPath + "/Data/PrefabData.json";
        File.WriteAllText(path, json);

        Debug.Log("Prefabデータをエクスポートしました！\n" + path);
    }
}
