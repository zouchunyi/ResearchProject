
#ifndef WINDSIMULATE_CG
#define WINDSIMULATE_CG

sampler2D _DetailWindGradient;
float4 _DetailWindGradient_ST;

uniform float4 _TimeEditor;

uniform sampler3D _DynamicWindTexture;
uniform float4 _PlayerPositon;

#include "UnityCG.cginc"

void RotateMesh(in out float3 worldVertex, in float maxAngle, in float3 rootVertex, in float3 fromDir, in float3 toDir)
{
	float cosAngle = dot(fromDir, toDir);
	float angle = acos(cosAngle);
	float angleDegrees = degrees(angle);
	angleDegrees = min(abs(angleDegrees), maxAngle) * (abs(angleDegrees) / angleDegrees);
	angle = radians(angleDegrees);
	cosAngle = cos(angle);
	float sinAngle = sin(angle);
	float3 V = worldVertex - rootVertex;
	float3 K = cross(fromDir, toDir);
	float3 Vrot = V * cosAngle + K * dot(K, V) * (1 - cosAngle) + cross(K, V) * sinAngle;
	worldVertex = Vrot + rootVertex;
}

void WindSimulate(in out float4 finalVertex, in float stressLevel, in float3 windDir, in float3 rootWorldVertex, in float3 vertex, in float3 worldVertex, in float3 normal, in float2 uv)
{
	float3 changePosition = stressLevel * windDir;
	float3 newPosition = worldVertex + changePosition;
	float3 fromDir = normalize(worldVertex - rootWorldVertex);
	float3 toDir = normalize(newPosition - rootWorldVertex);

	//旋转
	RotateMesh(worldVertex, 50, rootWorldVertex, fromDir, toDir);

	//计算局部扰动效果
	float4 time = _Time + _TimeEditor;
	float2 detailUV = (uv + time.g * float2(0.5, 0));
	float4 detailWind = tex2Dlod(_DetailWindGradient, float4(TRANSFORM_TEX(detailUV, _DetailWindGradient), 0.0, 0));
	float3 finalDetailWind = (detailWind.rgb * 0.4) * normal * stressLevel;
	worldVertex.xyz += mul(unity_ObjectToWorld, finalDetailWind);

	finalVertex = mul(UNITY_MATRIX_VP, float4(worldVertex, 1));
}

#endif // WINDSIMULATE_CG
