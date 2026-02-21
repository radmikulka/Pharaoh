Shader "Custom/Clouds"
{
    Properties
    {
        _TexA ("Noise A", 2D) = "white" {}
        _TexB ("Noise B", 2D) = "white" {}
        _ScrollA ("Scroll A", Vector) = (0.01, 0.02, 0, 0)
        _ScrollB ("Scroll B", Vector) = (-0.02, 0.01, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Overlay" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 uv : TEXCOORD0;
            };

            sampler2D _TexA;
            float4 _TexA_ST;
            sampler2D _TexB;
            float4 _TexB_ST;
            float4 _ScrollA;
            float4 _ScrollB;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.rgb);
                OUT.uv.xy = IN.uv + _TexA_ST.xy + _TexA_ST.zw + _ScrollA.xy * _Time.y;
                OUT.uv.zw = IN.uv + _TexB_ST.xy + _TexB_ST.zw + _ScrollB.xy * _Time.y;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                half a = tex2D(_TexA, IN.uv.xy).r;
                half b = tex2D(_TexB, IN.uv.zw).r;

                //half combined = (a + b) * 0.5;
                half combined = a * b;
                return half4(combined, combined, combined, 1);
            }
            ENDHLSL
        }
    }
}
