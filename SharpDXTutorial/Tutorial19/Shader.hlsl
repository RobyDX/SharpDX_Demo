Texture2D<float4> HeightMap;
Texture3D<float4> DiffuseMap;
Texture3D<float4> NormalMap;


SamplerState textureSampler
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
	AddressW = Wrap;
};

cbuffer dataBuffer : register( b0 )
{
    matrix ViewProjection;
    float3 cameraPosition;
    float3 lightDirection;
};

//=============================
//VERTEX SHADER
//=============================

struct VS_CONTROL_POINT_INPUT
{
    float3 Position: POSITION;
	float3 TexCoord: TEXCOORD;
};

struct VS_CONTROL_POINT_OUTPUT
{
    float3 Position: POSITION;
	float3 TexCoord: TEXCOORD;
};


VS_CONTROL_POINT_OUTPUT VSMain( VS_CONTROL_POINT_INPUT Input )
{
    VS_CONTROL_POINT_OUTPUT Output;

    Output.Position = Input.Position;
	Output.TexCoord = Input.TexCoord;
    return Output;
}



//=============================
//HULL SHADER
//=============================


struct HS_CONSTANT_DATA_OUTPUT
{
    float Edges[4]             : SV_TessFactor;
    float Inside[2]            : SV_InsideTessFactor;
};

struct HS_OUTPUT
{
    float3 Position : BEZIERPOS;
	float3 TexCoord: TEXCOORD;
};

HS_CONSTANT_DATA_OUTPUT BezierConstantHS( InputPatch<VS_CONTROL_POINT_OUTPUT, 16> ip, uint PatchID : SV_PrimitiveID )
{    
    HS_CONSTANT_DATA_OUTPUT Output;
    	
	//lati 
	float3 Top = (ip[5].Position + ip[6].Position)/2.0F;
	float3 Bottom = (ip[9].Position + ip[10].Position)/2.0F;
	float3 Left = (ip[5].Position + ip[9].Position)/2.0F;
	float3 Right = (ip[6].Position + ip[10].Position)/2.0F;
	float3 Center = (ip[5].Position + ip[10].Position)/2.0F;

	float4 D = float4(
		length(Top-cameraPosition),
		length(Bottom-cameraPosition),
		length(Left-cameraPosition),
		length(Right-cameraPosition));

	// Distance : ? = MaxDist : 32
	float section=75.0F;
	float maxTile=15;
	D=clamp(ceil(D/section),0,maxTile);
	D=16-D;
	
	Output.Edges[0] = (int)D.z;//L
	Output.Edges[1] = (int)D.x;//T
	Output.Edges[2] = (int)D.w;//R
	Output.Edges[3] = (int)D.y;//B

	float f=length(Center-cameraPosition);
	float DC= 16-clamp(ceil(f/section),0,maxTile);
	
	Output.Inside[0] = Output.Inside[1] = DC;
	
    return Output;
}


[domain("quad")]
[partitioning("integer")]
[outputtopology("triangle_cw")]
[outputcontrolpoints(16)]
[patchconstantfunc("BezierConstantHS")]
HS_OUTPUT HSMain( InputPatch<VS_CONTROL_POINT_OUTPUT, 16> p, 
                    uint i : SV_OutputControlPointID,
                    uint PatchID : SV_PrimitiveID )
{
    HS_OUTPUT Output;
    Output.Position = p[i].Position;
	Output.TexCoord = p[i].TexCoord;
    return Output;
}


//=============================
//DOMAIN SHADER
//=============================


struct DS_OUTPUT
{
    float4 Position : SV_POSITION;
    float3 WorldPos : WORLDPOS;
	float3 LightDirection:LIGHTDIR;
	float3 ViewDirection:VIEWDIR;
	float3 Normal:NORMAL;
	float2 TexCoord: TEXCOORD;
};


float4 BSplineBasis(float t)
{
    float invT = 1.0f - t;

    return float4( invT * invT * invT ,
                   3.0f * t * t * t - 6.0F * t * t + 4,
                   3.0f * (-t * t * t + t * t + t ) + 1,
                   t * t * t ) / 6.0F;
}


float4 DBSplineBasis(float t)
{
    float invT = 1.0f - t;

    return float4( -3 * invT * invT,
                   9 * t * t - 12 * t,
                   - 9 * t * t + 6 * t + 3,
                   3 * t * t );
}


float3 EvaluateBezier( float3 P[16], float4 BasisU, float4 BasisV )
{
    float3 Value = float3(0,0,0);

    Value  = BasisV.x * ( P[0] * BasisU.x + P[1] * BasisU.y + P[2] * BasisU.z + P[3] * BasisU.w );
    Value += BasisV.y * ( P[4] * BasisU.x + P[5] * BasisU.y + P[6] * BasisU.z + P[7] * BasisU.w );
    Value += BasisV.z * ( P[8] * BasisU.x + P[9] * BasisU.y + P[10] * BasisU.z + P[11] * BasisU.w );
    Value += BasisV.w * ( P[12] * BasisU.x + P[13] * BasisU.y + P[14] * BasisU.z + P[15] * BasisU.w );
    return Value;
}


float2 EvaluateTex(float2 tex[16], float2 uv)
{
    return lerp(tex[5],tex[10],uv.xy) ;
}

[domain("quad")]
DS_OUTPUT DSMain( HS_CONSTANT_DATA_OUTPUT input, float2 UV : SV_DomainLocation, const OutputPatch<HS_OUTPUT, 16> bezpatch )
{
	
	float3 P[16];
	float2 T[16];
	for(int i=0;i<16;i++)
	{
		float3 tp=bezpatch[i].Position;
		
		T[i] = bezpatch[i].TexCoord.xy;
		float I=HeightMap.SampleLevel(textureSampler, bezpatch[i].TexCoord.xy/16,0,0).x;

		P[i] = tp + float3(0, 0, I*800.0F);
	}

	
    float4 BasisU = BSplineBasis( UV.x );
    float4 BasisV = BSplineBasis( UV.y );
    float4 dBasisU = DBSplineBasis( UV.x );
    float4 dBasisV = DBSplineBasis( UV.y );

    float3 WorldPos = EvaluateBezier( P, BasisU, BasisV );
	
	float2 TexPos = EvaluateTex( T, UV);
    float3 Tangent = EvaluateBezier( P, dBasisU, BasisV );
    float3 Binormal = EvaluateBezier( P, BasisU, dBasisV );
	
    float3 Normal = normalize( cross( Tangent, Binormal ) );

    DS_OUTPUT Output;
	float3x3 tangentMat=float3x3(normalize(Tangent),normalize(Binormal),normalize(Normal));
	
    Output.Position = mul( float4(WorldPos,1), ViewProjection );
    Output.WorldPos = WorldPos;
	Output.TexCoord= TexPos;
	
	Output.LightDirection=mul(tangentMat,lightDirection);
	Output.ViewDirection=mul(tangentMat,normalize(cameraPosition));
	Output.Normal=Normal;
    return Output;    
}



//=============================
//PIXEL SHADER
//=============================


struct OutputPS
{
	float4 color0: SV_TARGET0;
};

//Oren Nayar
float4 OrenNayar (float3 n,float3 l,float3 v) 
{
	float fRoughness=0.003F;
    // Compute the other aliases
    float gamma=dot(v-n*dot(v,n),l-n*dot(l,n));
    float rough_sq = fRoughness * fRoughness;
 
    float A = 1.0f - 0.5f * (rough_sq / (rough_sq + 0.57f));
 
    float B = 0.45f * (rough_sq / (rough_sq + 0.09));
 
    float alpha = max( acos( dot( v, n ) ), acos( dot( l, n ) ) );
    float beta  = min( acos( dot( v, n ) ), acos( dot( l, n ) ) );
    float C = sin(alpha) * tan(beta);
    float final = (A + B * max( 0.0f, gamma ) * C);
    return max( 0.0f, dot( n, l ) ) * final;
}


OutputPS PSMain( DS_OUTPUT Input ) 
{
	OutputPS outP;
	
	float3 No=normalize(Input.Normal);
	float4 diffuse=  DiffuseMap.Sample( textureSampler, float3(Input.TexCoord.xy,No.z));
	float3 N = normalize(NormalMap.Sample(textureSampler, float3(Input.TexCoord.xy,No.z)).xyz*2.0F-1);

	float3 L = normalize(Input.LightDirection.xyz);
	float3 V = normalize(Input.ViewDirection.xyz);
    
	outP.color0 = OrenNayar(N,-L,V) * diffuse ;
	
	return outP;

}



