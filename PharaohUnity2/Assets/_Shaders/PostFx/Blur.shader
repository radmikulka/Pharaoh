Shader "Hidden/Custom/Blur"
{
    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

    float _PixelOffset;

    float4 Frag(Varyings input) : SV_Target
    {
        float2 offset = _BlitTexture_TexelSize.xy * _PixelOffset;

        half4 o = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord);
        o += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord + float2(offset.x, offset.y));
        o += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord + float2(offset.x, -offset.y));
        o += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord + float2(-offset.x, offset.y));
        o += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord + float2(-offset.x, -offset.y));
        return o / 5;
    }
    ENDHLSL

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"
        }
        LOD 100
        ZWrite Off Cull Off
        Pass
        {
            Name "KawaseBlur"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            ENDHLSL
        }
    }
}