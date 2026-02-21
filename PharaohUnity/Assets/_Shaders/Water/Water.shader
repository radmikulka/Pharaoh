Shader "Custom/Water"
{
    Properties
    {
        [Header(Surface Textures)]
        _Normals ("Normals", 2D) = "bump" {}
        _Normals2 ("Normal 2", 2D) = "bump" {}
        _EnvTex ("Reflection Cubemap", CUBE) = "_Default" {}

        [Header(Color and Transparency)]
        _ShallowColor ("Shallow Color", Color) = (0.2, 0.8, 1.0, 1.0)
        _DeepColor ("Deep Color", Color) = (0.0, 0.2, 0.4, 1.0)
        _DistortionStrength("Distortion Strength", Range(0, 0.1)) = 0.02
        
        [Header(Fresnel)]
        _FresnelBias ("Fresnel Bias", Range(0, 1)) = 0.05
        _FresnelPower ("Fresnel Power", Range(1, 20)) = 7.0

        [Header(Wave Animation)]
        _ScrollSpeed ("Scroll Speed", Float) = 0.1
        _ScrollDir1 ("Normal1 Scroll Angle [X,Y]", Vector) = (1, 0, 0, 0)
        _ScrollDir2 ("Normal2 Scroll Angle [X,Y]", Vector) = (2, 0, 0, 0)
        _Normal1Strength ("Normal 1 Strength", Float) = 1.0
        _Normal2Strength ("Normal 2 Strength", Float) = 1.0
        
        [Header(Depth)]
        _DepthScale ("Depth Scale", Range(0.0, 10.0)) = 3
        
        [Header(Foam)]
        _FoamDepth ("Foam Depth", Range(0.0, 10.0)) = 0.5
        _FoamScrollSpeed ("Scroll Speed", Range(0.0, 10.0)) = 0.05
        _FoamMod ("Foam Power Mod", Range(0.0, 10.0)) = 1
        _FoamNoise1 ("FoamNoise 1", 2D) = "white" {}
        _FoamNoise2 ("FoamNoise 2", 2D) = "white" {}
        
        [Header(Waves)]
        [Toggle(_WAVES_ON)] _EnableWaves("Enable Waves", Float) = 0
        _WaveAmplitude ("Wave Amplitude", Range(0, 2)) = 0.1
        _WaveFrequency ("Wave Frequency", Range(0, 20)) = 1.0
        _WaveSpeed ("Wave Speed", Range(0, 10)) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" "IgnoreProjector"="True" }
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            //Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _LOW_QUALITY _MEDIUM_QUALITY
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #pragma multi_compile_fog
            #pragma shader_feature _ _WAVES_ON
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _ShallowColor;
                half4 _DeepColor;
                half _FresnelBias;
                half _FresnelPower;
                half _BumpsTiling;
                half _BumpsTiling2;
                half _ScrollSpeed;
                half _DepthScale;
                half4 _ScrollDir1;
                half4 _ScrollDir2;
                half _Normal1Strength;
                half _Normal2Strength;
                half _FoamDepth;
                half _FoamScrollSpeed;
                half _FoamMod;
                half _DistortionStrength;
                float4 _Normals_ST;
                float4 _Normals2_ST;
                float4 _FoamNoise1_ST;
                float4 _FoamNoise2_ST;
                half _WaveAmplitude;
                half _WaveFrequency;
                half _WaveSpeed;
            CBUFFER_END

            TEXTURE2D(_FoamNoise1);                     SAMPLER(sampler_FoamNoise1);
            TEXTURE2D(_FoamNoise2);                     SAMPLER(sampler_FoamNoise2);
            TEXTURE2D(_Normals);                        SAMPLER(sampler_Normals);
            TEXTURE2D(_Normals2);                       SAMPLER(sampler_Normals2);
            TEXTURECUBE(_EnvTex);                       SAMPLER(sampler_EnvTex);
            TEXTURE2D_X_FLOAT(_CameraDepthTexture);     SAMPLER(sampler_CameraDepthTexture);
            TEXTURE2D_X(_CameraOpaqueTexture);          SAMPLER(sampler_CameraOpaqueTexture);

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS       : SV_POSITION;
                float3 positionWS       : TEXCOORD0;
                float4 normalWSAndFog   : TEXCOORD1;
                float4 uv1Uv2           : TEXCOORD2;
                float4 foamUv           : TEXCOORD3;
                half4  vertexColor      : TEXCOORD4;
                float4 screenPos        : TEXCOORD5;
                half3 vertexSH          : TEXCOORD6;
                #if defined(_MAIN_LIGHT_SHADOWS) && !defined(_RECEIVE_SHADOWS_OFF)
                float4 shadowCoord      : TEXCOORD7;
                #endif
            };

            half3 MixNormals(half3 n1, half3 n2)
            {
               half3 r = half3(n1.xy + n2.xy, n1.z * n2.z);
               return normalize(r);
            }

            Varyings vert(Attributes v)
            {
                Varyings o;

                #if defined(_WAVES_ON)
                float3 worldPos = TransformObjectToWorld(v.positionOS.xyz);
                float wave = 
                    sin(worldPos.x * _WaveFrequency + _Time.y * _WaveSpeed) +
                    cos(worldPos.z * _WaveFrequency * 0.8 + _Time.y * _WaveSpeed * 1.4);
                worldPos.y += wave * _WaveAmplitude;

                v.positionOS = float4(TransformWorldToObject(worldPos), 1);
                #endif
                
                VertexPositionInputs posInputs = GetVertexPositionInputs(v.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(v.normalOS.xyz, 0);
                o.positionCS = posInputs.positionCS; 
                o.positionWS = posInputs.positionWS;

                o.normalWSAndFog.rgb = normalInput.normalWS;
                o.normalWSAndFog.a = ComputeFogFactor(posInputs.positionCS.z);
                o.vertexColor = v.color;

                float2 scrollVec1 = _ScrollDir1.xy * _ScrollSpeed * _Time.y;
                float2 scrollVec2 = _ScrollDir2.xy * _ScrollSpeed * _Time.y * -0.1;

                o.uv1Uv2.xy = o.positionWS.xz * _Normals_ST.xy + _Normals_ST.zw + scrollVec1;
                o.uv1Uv2.zw = o.positionWS.xz * _Normals2_ST.xy + _Normals2_ST.zw + scrollVec2;
                
                o.foamUv.xy = o.positionWS.xz * _FoamNoise1_ST.xy + _FoamNoise1_ST.zw + _Time.y * _FoamScrollSpeed;
                o.foamUv.zw = o.positionWS.xz * _FoamNoise2_ST.xy + _FoamNoise2_ST.zw + _Time.y * 0.5 * _FoamScrollSpeed;

                o.screenPos = ComputeScreenPos(o.positionCS);

                o.vertexSH = SampleSH(normalInput.normalWS.xyz);

                #if defined(_MAIN_LIGHT_SHADOWS) && !defined(_RECEIVE_SHADOWS_OFF)
                    o.shadowCoord = GetShadowCoord(posInputs);
                #endif

                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                half3 normalMap1 = UnpackNormal(SAMPLE_TEXTURE2D(_Normals, sampler_Normals, i.uv1Uv2.xy));
                normalMap1.xy *= _Normal1Strength;
                half3 normalMap2 = UnpackNormal(SAMPLE_TEXTURE2D(_Normals2, sampler_Normals2, i.uv1Uv2.zw));
                normalMap2.xy *= _Normal2Strength;
                half3 combinedNormal = MixNormals(normalMap1, normalMap2);
                half3 finalNormal = MixNormals(combinedNormal, i.normalWSAndFog.rgb);
                
                //half NdotL = saturate(dot(finalNormal, normalize(_MainLightPosition.xyz)));
                //NdotL = pow(NdotL, 5);
                //return half4(NdotL.rrr, 1.0);

                half3 viewDirWS = normalize(_WorldSpaceCameraPos.xyz - i.positionWS);
                
                half3 reflectionVector = reflect(-viewDirWS, finalNormal);
                half3 reflectionColor = SAMPLE_TEXTURECUBE(_EnvTex, sampler_EnvTex, reflectionVector).rgb;
                //return half4(reflectionColor, 1);

                float2 uv = i.screenPos.xy / i.screenPos.w;
                float rawDepth = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, uv).r;
                float linearDepthMeters = LinearEyeDepth(rawDepth, _ZBufferParams);

                float fragDepthMeters = i.screenPos.w;
                half depthDiffMeters = linearDepthMeters - fragDepthMeters;

                half depthNormalized = saturate(depthDiffMeters / _DepthScale); 
                //return half4(depthNormalized.xxx, 1);

                half foamFactor = 0;
                #if !defined(_LOW_QUALITY)
                half foamNoise1 = SAMPLE_TEXTURE2D(_FoamNoise1, sampler_FoamNoise1, i.foamUv.xy).r;
                half foamNoise2 = SAMPLE_TEXTURE2D(_FoamNoise2, sampler_FoamNoise2, i.foamUv.zw).r;
                
                half normalMod = dot(combinedNormal, float3(0,1,0));
                normalMod = saturate(normalMod * 5);

                half foamNoise = foamNoise1 * foamNoise2 * _FoamMod;
                //return half4(foamNoise.xxx, 1);
                foamFactor = saturate(1.0 - depthDiffMeters / _FoamDepth) * foamNoise;
                foamFactor *= i.vertexColor.a * normalMod * (1 - depthNormalized);
                #endif

                float4 shadowCoord = float4(0, 0, 0, 0);
                #if defined(_MAIN_LIGHT_SHADOWS) && !defined(_RECEIVE_SHADOWS_OFF)
                    shadowCoord = i.shadowCoord;
                #endif

                float2 distortion = finalNormal.xy * _DistortionStrength;
                float2 distortedUV = uv + distortion;
                
                half3 shallowColor = _ShallowColor.rgb;
                #if !defined(_MEDIUM_QUALITY) && !defined(_LOW_QUALITY)
                half3 sceneColor = SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, distortedUV).rgb;
                shallowColor = sceneColor;
                #endif
                
                //return half4(sceneColor, 1);
                half NdotV = 1.0 - saturate(dot(viewDirWS, finalNormal));
                half fresnel = saturate(_FresnelBias + (1.0 - _FresnelBias) * pow(NdotV, _FresnelPower));
                //return half4(fresnel.rrr, 1.0);

                half3 waterColor = lerp(shallowColor, _DeepColor.rgb, depthNormalized);
                //return half4(waterColor, 1.0);

                half3 finalColor = lerp(waterColor, reflectionColor, fresnel) + foamFactor;
                //return half4(finalColor, 1.0);

                //finalColor += i.vertexColor.rgb;
                //return half4(i.vertexColor.rgb, 1);

                //return half4(shadowCoord.rgb, 1);
                Light mainLight = GetMainLight(shadowCoord, i.positionWS, 0);
                finalColor *= mainLight.color * mainLight.shadowAttenuation + i.vertexSH;
                //return remappedAttenuation;

                //half NdotL = saturate(dot(finalNormal, _MainLightPosition));
                //half3 radiance = _MainLightColor * (mainLight.shadowAttenuation * NdotL * 0.5);
                //return half4(shadowAttenuation.rrr, 1.0);

                finalColor = MixFog(finalColor, i.normalWSAndFog.a);

                return half4(finalColor, 0.95);
            }
            ENDHLSL
        }
    }
}