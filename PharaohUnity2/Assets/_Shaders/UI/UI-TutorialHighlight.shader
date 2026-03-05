Shader "Custom/UI//TutorialHighlight"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Overlay Color", Color) = (0,0,0,0.8)
        
        _Center ("Center (Pixels)", Vector) = (0, 0, 0, 0)
        _Size ("Size (Width/Height or Radius in px)", Vector) = (100, 100, 0, 0)
        _Softness ("Gradient Softness (px)", Float) = 20
        
        [Toggle] _IsRect ("Is Rectangle shape", Float) = 0
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

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            fixed4 _Color;
            float4 _Center;
            float4 _Size;
            float _Softness;
            float _IsRect;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }

            float sdBox(float2 p, float2 b)
            {
                float2 d = abs(p) - b;
                return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 pixelPos = IN.texcoord * _ScreenParams.xy;
                float2 center = _Center.xy;

                float2 pos = pixelPos - center;

                float dist;

                if (_IsRect > 0.5)
                {
                    float2 size = _Size.xy * 0.5; 
                    dist = sdBox(pos, size);
                }
                else
                {
                    dist = length(pos) - (_Size.x * 0.5);
                }

                float alpha = smoothstep(-_Softness, 0.0, dist);

                fixed4 color = IN.color;
                color.a *= alpha;

                return color;
            }
            ENDCG
        }
    }
}