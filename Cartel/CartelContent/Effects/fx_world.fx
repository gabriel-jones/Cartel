sampler s0;
texture lightMask;
float dayProgress;

static const float PI = 3.14159265f;

sampler lightSampler = sampler_state { 
	Texture = lightMask;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;

	AddressU = Clamp;
	AddressV = Clamp;
};

float4 PixelShaderFunction(float2 coords: TEXCOORD0) : COLOR0 {
	// 0 = midnight, 0.5 = noon, 1.0 = midnight
	float day = dayProgress;
	if (day > 0.5) {
		day = 1 - day;
	}
	day *= 2;
	float dayValue = sin((dayProgress / 1.4) * PI * 2 + 5); // I don't fuckin know lol
	float AmbientIntensity = clamp(dayValue, 0.1, 1.0);
	float4 AmbientColor = float4(AmbientIntensity, AmbientIntensity, AmbientIntensity, 1);

	float4 color = tex2D(s0, coords);
	float4 lightColor = tex2D(lightSampler, coords);
	return color * (lightColor + AmbientColor);
}

technique SpriteBatch {
	pass {
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}