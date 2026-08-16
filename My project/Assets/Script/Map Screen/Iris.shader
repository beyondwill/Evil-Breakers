Shader "Custom/IrisTransition"
{
    Properties
    {
        _Radius ("Radius", Range(0,2)) = 1
        _Center ("Center", Vector) = (0.5,0.5,0,0)
        _Color ("Color", Color) = (0,0,0,1)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float _Radius;
            float4 _Center;
            float4 _Color;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float aspect = _ScreenParams.x / _ScreenParams.y;

                float2 diff;
                diff.x = (i.uv.x - _Center.x) * aspect;
                diff.y = i.uv.y - _Center.y;

                float dist = length(diff);

                if (dist < _Radius)
                {
                    return float4(0,0,0,0);
                }

                return _Color;
            }
            ENDCG
        }
    }
}