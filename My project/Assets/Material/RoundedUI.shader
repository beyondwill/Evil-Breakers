Shader "UI/RoundedUI"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Radius ("Radius", Range(0, 0.5)) = 0.1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };

            float _Radius;
            sampler2D _MainTex;

            v2f vert (appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color; // UI의 기본 색상 정보를 가져옵니다.
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                // 둥근 모서리 계산
                float2 uv = abs(i.uv - 0.5) * 2.0;
                float dist = length(max(uv - (1.0 - _Radius), 0.0)) - _Radius;
                
                // 안티앨리어싱 적용된 알파값
                float alpha = 1.0 - smoothstep(0.0, 0.01, dist);
                
                // 텍스처와 UI 컴포넌트의 색상을 그대로 사용
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                
                col.a *= alpha;
                return col;
            }
            ENDCG
        }
    }
}