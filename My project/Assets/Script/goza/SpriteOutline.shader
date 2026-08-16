Shader "Custom/SpriteOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (0,1,0,1)
        _OutlineSize ("Outline Size", Range(0, 5)) = 1
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _OutlineColor;
            float _OutlineSize;

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 color = tex2D(_MainTex, i.uv);
                
                // 테두리 두께만큼 샘플링
                float2 size = _OutlineSize * _MainTex_TexelSize.xy;
                
                float maxAlpha = 0;
                // 8방향 샘플링으로 충분히 깔끔한 모서리 생성
                maxAlpha = max(maxAlpha, tex2D(_MainTex, i.uv + float2(size.x, 0)).a);
                maxAlpha = max(maxAlpha, tex2D(_MainTex, i.uv + float2(-size.x, 0)).a);
                maxAlpha = max(maxAlpha, tex2D(_MainTex, i.uv + float2(0, size.y)).a);
                maxAlpha = max(maxAlpha, tex2D(_MainTex, i.uv + float2(0, -size.y)).a);
                maxAlpha = max(maxAlpha, tex2D(_MainTex, i.uv + float2(size.x, size.y)).a);
                maxAlpha = max(maxAlpha, tex2D(_MainTex, i.uv + float2(-size.x, -size.y)).a);
                maxAlpha = max(maxAlpha, tex2D(_MainTex, i.uv + float2(size.x, -size.y)).a);
                maxAlpha = max(maxAlpha, tex2D(_MainTex, i.uv + float2(-size.x, size.y)).a);

                // 원본 알파가 높으면 원본 유지, 아니면 테두리 계산
                return lerp(fixed4(_OutlineColor.rgb, maxAlpha * _OutlineColor.a), color, color.a);
            }
            ENDCG
        }
    }
}