#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

float4 SmoothCurve( float4 x )
{
	return x * x *( 3.0 - 2.0 * x );
}

float4 TriangleWave( float4 x )
{
	return abs( frac( x + 0.5 ) * 2.0 - 1.0 );
}

float4 SmoothTriangleWave( float4 x )
{
	return SmoothCurve( TriangleWave( x ) );
}

float3 AnimateVertex2(float3 pos, float3 normal, float4 animParams,float4 wind, float2 time)
{	
    // animParams stored in color
    // animParams.x = branch phase
    // animParams.y = edge flutter factor
    // animParams.z = primary factor
    // animParams.w = secondary factor

    float fDetailAmp = 0.1f;
    float fBranchAmp = 0.3f;
	
    // Phases (object, vertex, branch)
    float fObjPhase = dot(UNITY_MATRIX_M[3].xyz, 1);
    float fBranchPhase = fObjPhase + animParams.x;
	
    float fVtxPhase = dot(pos.xyz, animParams.y + fBranchPhase);
	
    // x is used for edges; y is used for branches
    float2 vWavesIn = time  + float2(fVtxPhase, fBranchPhase );
	
    // 1.975, 0.793, 0.375, 0.193 are good frequencies
    float4 vWaves = (frac( vWavesIn.xxyy * float4(1.975, 0.793, 0.375, 0.193) ) * 2.0 - 1.0);
	
    vWaves = SmoothTriangleWave(vWaves);
    float2 vWavesSum = vWaves.xz + vWaves.yw;

    // Edge (xz) and branch bending (y)
    float3 bend = animParams.y * fDetailAmp;
    bend.y = animParams.w * fBranchAmp;
    pos += ((vWavesSum.xyx * bend) + (wind.xyz * vWavesSum.y * animParams.w)) * wind.w; 

    // Primary bending
    // Displace position
    pos += animParams.z * wind.xyz;
			
			
    return pos;
}

float3 GetPositionModifiedByWind(
	float3 positionOS,
	float3 normalOS,
	half vertexAlpha,
	half4 inputWind,
	half windEdgeFlutter,
	half windEdgeFlutterFreqScale
	)
{
	float4 wind;		
	float bendingFact = vertexAlpha;
		
	wind.xyz = mul((float3x3)UNITY_MATRIX_I_M, inputWind.xyz);
	wind.w = inputWind.w * bendingFact;
				
	float4 windParams = float4(0, windEdgeFlutter, bendingFact.xx);
	float windTime = _Time.y * windEdgeFlutterFreqScale;
	float3 mdlPos = AnimateVertex2(positionOS, normalOS, windParams, wind, windTime);
	return mdlPos;
}