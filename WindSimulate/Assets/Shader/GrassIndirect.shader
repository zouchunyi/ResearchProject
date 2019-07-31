// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/GrassIndirect"
{
	Properties {
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_GlobalWindDirection("Global Wind Direction", Vector) = (0,0,0,0)
		_GlobalWindStrength("Global Wind Strength", float) = 1
		
		_DetailWindGradient("Detail Wind Gradient", 2D) = "white" {}
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
			//#pragma target 4.5

			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc"
			#include "AutoLight.cginc"
			#include "WindSimulateCG.cginc"

			sampler2D _MainTex;
			float4 _GlobalWindDirection;
			float _GlobalWindStrength;
			
			uniform float4 _GlobalWind;


			float _TopPositionY;

			StructuredBuffer<float4> _positionBuffer;
			StructuredBuffer<float4> _vegetationArgsBuffer;

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
				float4 data = _positionBuffer[instanceID];
				float4 args = _vegetationArgsBuffer[instanceID];

				float3 localPosition = v.vertex.xyz;
				float3 worldPosition = data.xyz + localPosition;

				//Wind Simulate
				
				//Global Wind
				float3 windTiers = _GlobalWind.xyz * _GlobalWind.w;

				//Dynamic Wind
				float3 dynamicUV;
				float3 origin = _PlayerPositon.xyz - float3(16, 8, 16);
				float3 centerWorld = data.xyz;
				dynamicUV.x = (centerWorld.x - origin.x) / 32.0;
				dynamicUV.y = (centerWorld.y - origin.y) / 16.0;
				dynamicUV.z = (centerWorld.z - origin.z) / 32.0;
				if (dynamicUV.x >= 0 && dynamicUV.x <= 1 && dynamicUV.y >= 0 && dynamicUV.y <= 1 && dynamicUV.z >= 0 && dynamicUV.z <= 1)
				{
					float4 tex = tex3Dlod(_DynamicWindTexture, float4(dynamicUV,0));
					float3 dynamicWind = (tex.xyz * 2 - 1) * tex.w * 10;
					windTiers += dynamicWind;
				}

				//Main Wind

				float maxAngle = args.x;
				float stressLevel = clamp((v.vertex.y - 0.2) / _TopPositionY, 0, 1);
				stressLevel += pow(stressLevel, 2);
				
				v2f o;
				float3 root = data.xyz + float3(v.vertex.x, 0, v.vertex.z);
				WindSimulate(o.pos, stressLevel, windTiers, root, v.vertex.xyz, worldPosition, v.normal, v.texcoord);


				float3 worldNormal = v.normal;

				float3 ndotl = saturate(dot(worldNormal, _WorldSpaceLightPos0.xyz));
				float3 ambient = ShadeSH9(float4(worldNormal, 1.0f));
				float3 diffuse = (ndotl * _LightColor0.rgb);
				float3 color = v.color;

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
				albedo.xyz *= 3;
				return albedo;
			}

			ENDCG
		}
	}
}
