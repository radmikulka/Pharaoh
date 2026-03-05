Shader "Custom/UI/Default WaveScroll"
{
    Properties
    {
		[Toggle] _USETIMESCALE("UseTimescale", Float) = 1
		_Note ("Sprite must have clamped swap mode and canvas have to have enabled Texcoord1 shader channel", float) = 0
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
		_ScrollTex ("Scroll Texture", 2D) = "white" {}
		_ScrollMaskTex ("Scroll Texture - R - channel", 2D) = "white" {}
		_WaveColor ("Wave Color", Color) = (1,1,1,1)
		_DelayBetweenScrolls ("Delay Between Scrolls", Range (0, 10)) = 1
		_ScrollingDuration("Scrolling Duration", Range (0.01, 10)) = 1

        [Toggle] _ANIM_MANUAL("Manual Control (Ignore Time)", Float) = 0
        _AnimPhase("Animation Phase (0..1)", Range(0,1)) = 0

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        [Enum(None,0,Alpha,1,Red,8,Green,4,Blue,2,RGB,14,RGBA,15)] _ColorMask("Color Mask", Int) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma shader_feature _ _USETIMESCALE_ON
            #pragma shader_feature _ _ANIM_MANUAL_ON
            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile __ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float2 texcoord2 : TEXCOORD1;
                float3 tangent : TANGENT;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                half4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 texcoordScroll  : TEXCOORD1;
                float4 worldPosition : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            half4 _Color;
            half4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _ScrollTex_ST;
			half4 _WaveColor;
			float _DelayBetweenScrolls;
			float _ScrollingDuration;
			float _ScrollingSpeed;
			float _UnscaledTime;
			float _UseTimescale;
            float _AnimPhase;
            float _ANIM_MANUAL; 

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord.xy = v.texcoord;

                OUT.color = v.color * _Color;

            	float scrollingOffset = v.tangent.x;

				float t = (0);
				#if defined (_USETIMESCALE_ON)
				{
					t = _Time.y + scrollingOffset;
				}
				#else
				{
					t = _UnscaledTime + scrollingOffset;
				}
                #endif

                #if defined(_ANIM_MANUAL_ON)
                    t = _AnimPhase;
                #endif

				float delay = fmod(t, _DelayBetweenScrolls + _ScrollingDuration);
				if(delay < _ScrollingDuration)
				{
					float duration = delay / _ScrollingDuration;
					OUT.texcoordScroll.xy = v.texcoord2;
					OUT.texcoordScroll.zw = v.texcoord2 + lerp(half2(1, 0), half2(-1, 0), duration);
					//OUT.texcoord.zw *= _ScrollTex_ST.xy + _ScrollTex_ST.zw + frac(_ScrollingSpeed * _Time.y);
				}
				else
				{
					OUT.texcoordScroll = half4(-1, 0, -1, 0);
				}

                return OUT;
            }

            sampler2D _MainTex;
            sampler2D _ScrollTex;
            sampler2D _ScrollMaskTex;

            half4 frag(v2f IN) : SV_Target
            {
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd);
                half4 maskColor = (tex2D(_ScrollMaskTex, IN.texcoordScroll.xy) + _TextureSampleAdd);
                half4 scrollColor = (tex2D(_ScrollTex, IN.texcoordScroll.zw) + _TextureSampleAdd) * maskColor.r * color.a * _WaveColor;
				//return scrollColor;
				//return half4(IN.texcoordScroll.xy, 0,1);
				//return half4(maskColor.a, 0,0,1);
                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                return color * IN.color + scrollColor * scrollColor.a;
            }
        ENDCG
        }
    }
}
