Shader "Custom/UnlitAircraftLights"
{
    Properties
    {
        [HDR]_FlashColor ("Flash HDR Color", Color) = (1,0,0,1)

        [HDR]_IdleColor ("Idle HDR Color", Color) = (0,0,0,1)

        _FlashOn1 ("Flash On 1 (s)", Range(0, 0.3)) = 0.06
        _FlashGap ("Gap (s)", Range(0, 0.5)) = 0.10
        _FlashOn2 ("Flash On 2 (s)", Range(0, 0.3)) = 0.06
        _Pause ("Pause (s)", Range(0, 3.0)) = 0.78
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 50

        Pass
        {
            ZWrite On
            Cull Back
            Blend Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _FlashColor;
            fixed4 _IdleColor;

            half _FlashOn1, _FlashGap, _FlashOn2, _Pause;

            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 pos : SV_POSITION; };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            half Blink01(float t)
            {
                float cycle = _FlashOn1 + _FlashGap + _FlashOn2 + _Pause;

                float x = frac(t / cycle) * cycle;

                float a1 = _FlashOn1;
                float b0 = _FlashOn1 + _FlashGap;
                float b1 = b0 + _FlashOn2;

                float inA = (1.0h - step(a1, x));
                float inB = step(b0, x) * (1.0h - step(b1, x));

                return (half)saturate(inA + inB);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float t = _Time.y;
                half on = Blink01(t);

                fixed3 idle = _IdleColor.rgb;
                fixed3 flash = _FlashColor.rgb;

                fixed3 outCol = lerp(idle, flash, (fixed)on);
                return fixed4(outCol, 1.0);
            }
            ENDCG
        }
    }
}
