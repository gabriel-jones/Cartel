sampler s0;
texture lightMask;
float lightIntensity;

sampler lightSampler = sampler_state { 
	Texture = lightMask;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;

	AddressU = Clamp;
	AddressV = Clamp;
};

float4 PixelShaderFunction(float2 coords: TEXCOORD0) : COLOR0 {
	float4 AmbientColor = float4(lightIntensity, lightIntensity, lightIntensity, 1);

	float4 color = tex2D(s0, coords);
	float4 lightColor = tex2D(lightSampler, coords);
	return color * (lightColor + AmbientColor);
}

technique SpriteBatch {
	pass {
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}