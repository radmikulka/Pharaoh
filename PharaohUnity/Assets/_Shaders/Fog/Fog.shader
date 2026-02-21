Shader "Custom/FogDissolveWorld"
{
    Properties
    {
        _Color("Fog Color", Color) = (0.7, 0.7, 0.7, 1)
        _Intensity("Intensity", Range(0, 0.1)) = 0.01
        _NoiseTex0("Clouds Base Texture", 2D) = "white" {}

        [Toggle(_FADE)] _EnableFade("Enable Fade", Float) = 0
        _NoiseTex1("Noise Texture 1", 2D) = "white" {}
        _NoiseTex2("Noise Texture 2", 2D) = "white" {}
        _ScrollSpeed1("Scroll Speed 1 (X,Y)", Vector) = (0.05, 0.03, 0, 0)
        _ScrollSpeed2("Scroll Speed 2 (X,Y)", Vector) = (-0.03, 0.02, 0, 0)

        _Dissolve("Dissolve Amount", Range(0,1)) = 0
        _EdgeSoftness("Edge Softness", Range(0.001, 0.3)) = 0.05
    }

    SubShader
    {
        Tags { "Queue" = "Transparent+1000" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            //Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _FADE
            

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color      : COLOR;
                float2 Uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 screenPos  : TEXCOORD0;
                half color        : TEXCOORD1;
                float2 CloudUv    : TEXCOORD2;
                #if _FADE
                float4 fadeUv       : TEXCOORD3;
                #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D_X_FLOAT(_CameraDepthTexture);             SAMPLER(sampler_CameraDepthTexture);
            TEXTURE2D(_NoiseTex0);                              SAMPLER(sampler_NoiseTex0);
            TEXTURE2D(_NoiseTex1);                              SAMPLER(sampler_NoiseTex1);
            TEXTURE2D(_NoiseTex2);                              SAMPLER(sampler_NoiseTex2);

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Intensity;
                half4 _NoiseTex0_ST;
                half4 _NoiseTex1_ST;
                half4 _NoiseTex2_ST;
                half4 _ScrollSpeed1;
                half4 _ScrollSpeed2;
                half _Dissolve;
                half _EdgeSoftness;
            CBUFFER_END            

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.screenPos = ComputeScreenPos(OUT.positionCS);
                OUT.color = IN.color.a;

                OUT.CloudUv.xy = IN.Uv * _NoiseTex0_ST.xy + _NoiseTex0_ST.zw;

                #if _FADE
                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.fadeUv.xy = worldPos.xz * _NoiseTex1_ST.xy + _NoiseTex1_ST.zw + _Time.y * _ScrollSpeed1.xy;
                OUT.fadeUv.zw = worldPos.xz * _NoiseTex2_ST.xy + _NoiseTex2_ST.zw + _Time.y * _ScrollSpeed2.xy;
                #endif

                return OUT;
            }

            half4 frag(Varyings IN) : SV_TARGET
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

                float sceneRawDepth = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, IN.screenPos.xy / IN.screenPos.w).r;
                float sceneLinearEyeDepth = LinearEyeDepth(sceneRawDepth, _ZBufferParams);
                float fogLinearEyeDepth = IN.screenPos.w;
                float diff = saturate(_Intensity * (sceneLinearEyeDepth - fogLinearEyeDepth));
                float smoothDiff = diff * diff;

                float3 clouds = SAMPLE_TEXTURE2D(_NoiseTex0, sampler_NoiseTex0, IN.CloudUv.xy).rgb;
                //return (1,1,1, clouds);

                float mask = 1;
                #if _FADE
                float n1 = SAMPLE_TEXTURE2D(_NoiseTex1, sampler_NoiseTex1, IN.fadeUv.xy).r;
                float n2 = SAMPLE_TEXTURE2D(_NoiseTex2, sampler_NoiseTex2, IN.fadeUv.zw).r;
                float noise = saturate((n1 + n2) * 0.5);

                noise = lerp(_EdgeSoftness, 1 - _EdgeSoftness, noise); 
                mask = 1 - smoothstep(_Dissolve - _EdgeSoftness, _Dissolve + _EdgeSoftness, noise);
                #endif

                half4 baseColor = half4(_Color.rgb * clouds, smoothDiff * mask * IN.color);
                return baseColor;
            }

            ENDHLSL
        }
    }
}
