#ifndef __DATA_TYPE__
#define __DATA_TYPE__


struct InstanceData {
	float4x4 ToWorld;
	float4x4 ToObject;
	float diameter;
};



#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
uniform StructuredBuffer<uint> cullResult;
uniform StructuredBuffer<InstanceData> instances;
#endif // UNITY_PROCEDURAL_INSTANCING_ENABLED



void setupProcedual()
{
#ifdef unity_ObjectToWorld
    #undef unity_ObjectToWorld
    #define unity_ObjectToWorld unity_ObjectToWorld
#endif

#ifdef unity_WorldToObject
    #undef unity_WorldToObject
    #define unity_WorldToObject unity_WorldToObject
#endif
    
#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
    uint index = cullResult[unity_InstanceID];
    unity_ObjectToWorld = instances[index].ToWorld;
	unity_WorldToObject = instances[index].ToObject;
#endif // UNITY_PROCEDURAL_INSTANCING_ENABLED
}


#endif