Shader "Custom/UI/HorizontalFade"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Power ("Fade Power", Range(0.1, 5)) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            fixed4 _Color;
            float _Power;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 중앙 0.5 기준으로 거리 계산
                float dist = abs(i.uv.x - 0.5) * 2;

                // 중앙은 1, 끝은 0
                float alpha = pow(1 - dist, _Power);

                fixed4 col = _Color * i.color;
                col.a *= alpha;

                return col;
            }
            ENDCG
        }
    }
}