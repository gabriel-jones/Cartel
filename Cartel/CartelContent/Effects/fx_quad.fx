float4x4 Projection;
Texture tex;

sampler texSampler = sampler_state {
	Texture = tex;
};

struct VertexShaderInput {
    float3 Position : POSITION0;
	float2 TextureCoordinate : TEXCOORD0;
};

struct VertexShaderOutput {
    float4 Position : POSITION0;
	float2 TextureCoordinate : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input) {
    VertexShaderOutput output;

	output.Position = float4(input.Position, 1.0f);
	output.TextureCoordinate = input.TextureCoordinate;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0 {
	return tex2D(texSampler, input.TextureCoordinate);
}

technique Quad {
    pass Pass0 {
        // TODO: set renderstates here.

        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
