using UnityEngine;

public class HardEdgeModel : MonoBehaviour
{
    private void Awake()
    {
        EmbedSoftEdgeToVertexColor(gameObject);
    }

    /// <summary>
    /// ソフトエッジ情報を頂点カラーに埋め込む
    /// </summary>
    /// <param name="obj"></param>
    private static void EmbedSoftEdgeToVertexColor(GameObject obj)
    {
        var meshFilters = obj.GetComponentsInChildren<MeshFilter>();
        foreach (var meshFilter in meshFilters)
        {
            var mesh = meshFilter.sharedMesh;
            var normals = mesh.normals;
            var vertices = mesh.vertices;
            var vertexCount = mesh.vertexCount;

            // ソフトエッジ法線情報の生成
            var softEdges = new Color[normals.Length];
            for (var i = 0; i < vertexCount; i++)
            {
                // 同じ位置の頂点の法線座標の平均を設定する
                var softEdge = Vector3.zero;
                for (var j = 0; j < vertexCount; j++)
                {
                    var v = vertices[i] - vertices[j];
                    if (v.sqrMagnitude < 1e-8f)
                    {
                        softEdge += normals[j];
                    }
                }
                softEdge.Normalize();
                softEdges[i] = new Color(softEdge.x, softEdge.y, softEdge.z, 0);
            }

            // 頂点カラーに埋め込む
            mesh.colors = softEdges;
        }
    }
}