Shader "Custom/Outline"
{
    Properties
    {
        _MainColor ("Main Color", Color) = (1, 1, 1, 1)
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineThickness ("Outline Thickness", Float) = 0.02
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Name "Outline"
            Cull Front   // ���ʂ�`��
            ZWrite On
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // �v���p�e�B
            float _OutlineThickness;
            float4 _OutlineColor;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            // ���_�V�F�[�_�[
            v2f vert(appdata_t v)
            {
                v2f o;
                // ���_�ʒu���O���ɉ����o��
                float3 worldNormal = normalize(UnityObjectToWorldNormal(v.normal));
                v.vertex.xyz += worldNormal * _OutlineThickness;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            // �t���O�����g�V�F�[�_�[
            fixed4 frag(v2f i) : SV_Target
            {
                return _OutlineColor; // �֊s���̐F
            }
            ENDCG
        }

        Pass
        {
            Name "Main"
            Cull Back // �ʏ�̕\�ʕ`��
            ZWrite On
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragMain
            #include "UnityCG.cginc"

            float4 _MainColor;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 fragMain(v2f i) : SV_Target
            {
                return _MainColor; // ���C���J���[
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
