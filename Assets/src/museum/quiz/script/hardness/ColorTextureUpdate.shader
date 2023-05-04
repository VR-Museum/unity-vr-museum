Shader "Custom/ColorTextureUpdate"
{
    Properties
    {
        _DrawingColor ("Drawing Color", Color) = (1, 0, 0, 1)
        _innerRadius ("Inner Radius", float) = 0.005
        _outerRadius ("Outer Radius", float) = 0.01
        _smoothMultiplier ("Smooth Multiplier", float) = 2
        _Point1 ("Point1", Vector) = (0.5, 0.5, 0, 0)
        _Point2 ("Point2", Vector) = (0.6, 0.5, 0, 0)
    }
    SubShader
    {
        Lighting Off
        Blend One Zero
        
        Pass
        {
            CGPROGRAM
            #include "UnityCustomRenderTexture.cginc"
            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag
            #pragma target 3.0

            float4 _DrawingColor;
            float _innerRadius;
            float _outerRadius;
            float _smoothMultiplier;
            float4 _Point1;
            float4 _Point2;

            float4 frag(v2f_customrendertexture IN) : COLOR
            {
                float A = 1 / (_Point2.x - _Point1.x);
                float B = -1 / (_Point2.y - _Point1.y);
                float C = _Point1.y / (_Point2.y - _Point1.y) - _Point1.x / (_Point2.x - _Point1.x);
                float Z = C + A * IN.localTexcoord.y - B * IN.localTexcoord.x;
                float y = (-B * C + A * (Z - C)) / (A * A + B * B);
                float x = (A * y - Z + C) / B;
                float proectionX = ((x >= _Point1.x && x <= _Point2.x) || (x <= _Point1.x && x >= _Point2.x) || x == _Point1.x || x == _Point2.x) ? x : ((distance(x, _Point1.x) < distance(x, _Point2.x)) ? _Point1.x : _Point2.x);
                float proectionY = ((y >= _Point1.y && y <= _Point2.y) || (y <= _Point1.y && y >= _Point2.y) || y == _Point1.y || y == _Point2.y) ? y : ((distance(y, _Point1.y) < distance(y, _Point2.y)) ? _Point1.y : _Point2.y);
                float4 proection = float4(proectionX, proectionY, 0, 0);
                float4 color = tex2D(_SelfTexture2D, IN.localTexcoord.xy);
                float4 newColor = 
                 (1 - smoothstep(_innerRadius, _outerRadius, distance(IN.localTexcoord.xy, proection) / _smoothMultiplier))
                 * _DrawingColor / _smoothMultiplier;

                return max(color, newColor);
            }
            ENDCG
        }
    }
}
