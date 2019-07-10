// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/GrassIndirect"
{
	Properties {
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_GlobalWindDirection("Global Wind Direction", Vector) = (0,0,0,0)
		_GlobalWindStrength("Global Wind Strength", float) = 1

		_TopPositionY("Top Position", float) = 1
		_BottomPositionY("Bottom Position", float) = 1
	}
	SubShader {

		Pass {

			Tags {"LightMode" = "ForwardBase" "Queue" = "AlphaTest"}
			//AlphaTest Greater 0.5
			
			ZTest LEqual
			ZWrite On
			//Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
			#pragma target 4.5

			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc"
			#include "AutoLight.cginc"

			sampler2D _MainTex;
			float4 _GlobalWindDirection;
			float _GlobalWindStrength;

			uniform float4 _GlobalWind;
			uniform sampler3D _DynamicWindTexture;

			float _TopPositionY;

		#if SHADER_TARGET >= 45
			StructuredBuffer<float4> _positionBuffer;
			StructuredBuffer<float4> _vegetationArgsBuffer;
		#endif

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv_MainTex : TEXCOORD0;
				float3 ambient : TEXCOORD1;
				float3 diffuse : TEXCOORD2;
				float3 color : TEXCOORD3;
				SHADOW_COORDS(4)
			};

			v2f vert(appdata_full v, uint instanceID : SV_InstanceID)
			{
			#if SHADER_TARGET >= 45
				float4 data = _positionBuffer[instanceID];
				float4 args = _vegetationArgsBuffer[instanceID];
			#else
				float4 data = float4(0, 0, 0, 1);
				float4 args = float4(60, 0, 0, 0);
			#endif

				float3 localPosition = v.vertex.xyz;
				float3 worldPosition = data.xyz + localPosition;

				//Wind Simulate
				float3 windTiers = _GlobalWind.xyz * _GlobalWind.w;
				
				//Main Wind

				float maxAngle = args.x;
				float stressLevel = clamp((v.vertex.y - 0.2) / _TopPositionY, 0, 1);
				stressLevel += pow(stressLevel, 2);
				float3 change = stressLevel * windTiers;
				float3 newPosition = worldPosition + change;
				float3 root = data.xyz + float3(v.vertex.x, 0, v.vertex.z);
				float3 way1 = normalize(worldPosition - root);
				float3 way2 = normalize(newPosition - root);
				float cosAngle = dot(way1, way2);
				float angle = acos(cosAngle);
				float angleDegrees = degrees(angle);
				angleDegrees = min(abs(angleDegrees), maxAngle) * (abs(angleDegrees) / angleDegrees);
				angle = radians(angleDegrees);
				cosAngle = cos(angle);
				float sinAngle = sin(angle);
				float3 V = worldPosition - root;
				float3 K = cross(way1, way2);
				float3 Vrot = V * cosAngle + K * dot(K, V) * (1 - cosAngle) + cross(K, V) * sinAngle;
				worldPosition.xyz = Vrot + root;

				//Detail Wind
				//float3 normal = mul(unity_ObjectToWorld, v.normal);
				//worldPosition.xyz += windTiers * 1 * normal;


				float3 worldNormal = v.normal;

				float3 ndotl = saturate(dot(worldNormal, _WorldSpaceLightPos0.xyz));
				float3 ambient = ShadeSH9(float4(worldNormal, 1.0f));
				float3 diffuse = (ndotl * _LightColor0.rgb);
				float3 color = v.color;

				v2f o;
				o.pos = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0f));
				o.uv_MainTex = v.texcoord;
				o.ambient = ambient;
				o.diffuse = diffuse;
				o.color = color;
				TRANSFER_SHADOW(o)
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed shadow = SHADOW_ATTENUATION(i);
				fixed4 albedo = tex2D(_MainTex, i.uv_MainTex);
				float3 lighting = i.diffuse * shadow + i.ambient;
				fixed4 output = fixed4(albedo.rgb * i.color * lighting, albedo.w);
				UNITY_APPLY_FOG(i.fogCoord, output);
				clip(output.a - 0.1);
				return output;
			}

			ENDCG
		}
	}
}
