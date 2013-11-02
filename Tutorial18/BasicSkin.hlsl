cbuffer data :register(b0)
{
	float4x4 transform;
	float4x4 world;
};

cbuffer PaletteMatrices:register(b1)
{
	float4x4 palette[256];
};

cbuffer Data
{
	float4 lightDirection;
};

struct VS_INPUT
{
	float4 position : POSITION;
	float3 normal : NORMAL;
	float2 tex : TEXCOORD;
	float3 binormal : BINORMAL;
	float3 tangent : TANGENT;
	float4 joint : JOINT;
	float4 weight : WEIGHT;
};

struct VS_OUTPUT
{
	float4 position : SV_POSITION;
	float2 texcoord:TEXCOORD0;
	float3 lightDirection:LIGHT;
};

VS_OUTPUT VSMain( VS_INPUT input )
{
	VS_OUTPUT Output;
	//create palette
	float4x4 mat=palette[input.joint.x] * input.weight.x + 
		palette[input.joint.y] * input.weight.y + 
		palette[input.joint.z] * input.weight.z +
		palette[input.joint.w] * input.weight.w;

	float4 pos = mul(mat , input.position);
		float3 N = mul(mat , input.normal);
		float3 T= mul(mat , input.tangent);
		float3 B = mul(mat , input.binormal);

		B=mul(world,B);
	T=mul(world,T);
	N=mul(world,N);

	float3x3 Tangent={T,B,N};
		Output.lightDirection=mul(lightDirection.xyz,Tangent);


	Output.position = mul(transform,pos);
	Output.texcoord=input.tex;
	return Output;
}


Texture2D textureMap;
SamplerState textureSampler;

Texture2D normalMap;


float4 PSMain( VS_OUTPUT input ) : SV_TARGET
{
	float3 L=normalize(input.lightDirection);

		float4 D=textureMap.Sample( textureSampler, input.texcoord);
		float3 N=normalMap.Sample( textureSampler, input.texcoord).xyz*2.0f-1.0f;
		N=normalize(N);
	L=normalize(L);
	return saturate(dot(N,L))*D+0.2F;
}


