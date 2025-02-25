Shader "Custom/TreeLeaves_ShellFin"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _AlphaTex ("Alpha Mask", 2D) = "white" {} 
        _LeafColor ("Leaf Color", Color) = (0.3, 0.8, 0.3, 1)
        _Alpha ("Leaf Transparency", Range(0,1)) = 0.8
        _WindStrength ("Wind Strength", Range(0,1)) = 0.2
        _WindSpeed ("Wind Speed", Range(0,5)) = 1.0
        _ShellCount ("Shell Layers", Range(1,10)) = 5
        _ShellDistance ("Shell Spacing", Range(0.01, 0.1)) = 0.05

        // Tree Editor �p�̃v���p�e�B
        _TranslucencyColor ("Translucency Color", Color) = (0.5, 0.5, 0.5, 1)
        _TranslucencyViewDependency ("Translucency View Dependency", Range(0,1)) = 0.5
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.3
        _ShadowStrength ("Shadow Strength", Range(0,1)) = 0.8
    }

    SubShader
    {
        Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" }
        LOD 200
        Cull Off
        ZWrite On
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata_t
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float shellFactor : TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _AlphaTex;
            fixed4 _LeafColor;
            half _Alpha;
            half _WindStrength;
            half _WindSpeed;
            half _ShellCount;
            half _ShellDistance;
            fixed4 _TranslucencyColor;
            half _TranslucencyViewDependency;
            half _Cutoff;
            half _ShadowStrength;

            v2f vert (appdata_t v)
            {
                v2f o;

                // **�C��: shellIndex �����̐����l�Ƃ��ČŒ�**
                float shellIndex = floor(_Time.y * _WindSpeed) % _ShellCount;
                float shellOffset = shellIndex * _ShellDistance;

                // **�C��: ���̌v�Z�����X���[�Y��**
                float wind = sin(_Time.y * _WindSpeed + v.vertex.x * 0.1) * _WindStrength;
                v.vertex.x += wind;
                v.vertex.y += wind * 0.3;

                // **�V�F���@: �@�������ɃI�t�Z�b�g**
                v.vertex.xyz += v.normal * shellOffset;

                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.shellFactor = shellIndex / _ShellCount;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);
                fixed4 alphaMask = tex2D(_AlphaTex, i.uv);

                // **�C��: �t�̓����x�����肳����**
                float alpha = _Alpha * alphaMask.a * (1.0 - i.shellFactor);
                return fixed4(tex.rgb * _LeafColor.rgb * _TranslucencyColor.rgb, alpha);
            }
            ENDCG
        }
    }
}
