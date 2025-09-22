Texture2D InputTexture : register(t0);
SamplerState InputSampler : register(s0);

cbuffer constants : register(b0)
{
    int mode      : packoffset(c0.x); // 0=Hue,1=S,2=L,3=R,4=G,5=B,6=A
    float offset      : packoffset(c0.y); // 始点
    int isInvert      : packoffset(c0.z); // 反転
};

// RGB→HSL変換 (Hueは0〜1で返す)
float3 RGBtoHSL(float3 color)
{
    float r = color.r;
    float g = color.g;
    float b = color.b;

    float maxc = max(r, max(g, b));
    float minc = min(r, min(g, b));

    float h = 0.0;
        if (minc == b)
            h = 60 * (g - r) / maxc - minc + 60;
        else if (minc == r)
            h = 60 * (b - g) / maxc - minc + 180;
        else
            h = 60 * (r - b) / maxc - minc + 300;

    float l = maxc;

    float s = maxc - minc;

    return float3(h, s, l);
}

float4 main(float4 pos : SV_POSITION, float4 posScene : SCENE_POSITION, float4 uv : TEXCOORD0) : SV_Target
{
    float4 color = InputTexture.Sample(InputSampler, uv.xy);

    float value = 0.0;

    if (mode == 0) // Hue (0〜360° → 0〜1 正規化)
    {
        float3 hsl = RGBtoHSL(color.rgb);
        value = hsl.x / 360.0; // = hsl.x
    }
    else if (mode == 1) // Saturation
    {
        float3 hsl = RGBtoHSL(color.rgb);
        value = hsl.y;
    }
    else if (mode == 2) // Lightness
    {
        float3 hsl = RGBtoHSL(color.rgb);
        value = hsl.z;
    }
    else if (mode == 3) // Red
    {
        value = color.r;
    }
    else if (mode == 4) // Green
    {
        value = color.g;
    }
    else if (mode == 5) // Blue
    {
        value = color.b;
    }
    else if (mode == 6) // Alpha
    {
        value = color.a;
    }

    value = value + offset;
    if(value > 1) value = value -1;
    if(isInvert == 1) value = 1- value;

    return float4(value, value, value, 1.0);
}
