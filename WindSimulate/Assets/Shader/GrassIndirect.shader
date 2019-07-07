Shader "Unlit/GrassIndirect"
{
	Properties {
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_StaticWind("Static Wind", Vector) = (0,0,0,0)
		_Radius("Static Wind", float) = 1
		fBendScale("Static Wind", float) = 1
	}
	SubShader {

		Pass {

			Tags {"LightMode" = "ForwardBase" "Queue" = "AlphaTest"}
			//AlphaTest Greater 0.5
			
			//ZTest LEqual
			//ZWrite On
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
			float4 _StaticWind;
			float _Radius;
			float fBendScale;

		#if SHADER_TARGET >= 45
			StructuredBuffer<float4> _positionBuffer;
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

			//void rotate2D(inout float2 v, float r)
			//{
			//	float s, c;
			//	sincos(r, s, c);
			//	v = float2(v.x * c - v.y * s, v.x * s + v.y * c);
			//}

			//void WindSimulate()

			v2f vert(appdata_full v, uint instanceID : SV_InstanceID)
			{
			#if SHADER_TARGET >= 45
				float4 data = _positionBuffer[instanceID];
			#else
				float4 data = float4(0, 0, 0, 1);
			#endif

				//float rotation = data.w * data.w * _Time.x * 0.5f;
				//rotate2D(data.xz, rotation);

				float3 localPosition = v.vertex.xyz * data.w;
				float3 worldPosition = data.xyz + localPosition;

				//windsimulate
				float3 windTiers = normalize(_StaticWind.xyz);
				//float velocity = 
				half dis = clamp(v.vertex.y / 2, 0, 1);
				dis += pow(dis, 1.5);
				float3 change = dis * windTiers * _Radius;
				worldPosition.x += change.x / 2;
				worldPosition.z += change.z / 2;
				worldPosition.y -= abs(change.x / 2);


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
				clip(output.a - 0.5);
				return output;
			}

			ENDCG
		}
	}
}
