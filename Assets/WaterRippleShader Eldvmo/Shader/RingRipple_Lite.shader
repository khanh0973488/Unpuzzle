Shader "Custom/RingRipple_Lite"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _Texture("Texture", 2D) = "white"{}
        _Decay("Decay", Range(0,20)) = 5
        _WaveLiftTime("Wave Life Time", Range(1,10)) = 2
        _WaveFrequency("Wave Frequency", Range(0,100)) = 25
        _WaveSpeed("Wave Speed", Range(0,10)) = 0.1
        _WaveStrength("Wave Strength", Range(0,5)) = 0.5
        _StencilRef("Stencil Ref", Range(0,255)) = 1
        
        // === VORTEX PROPERTIES ===
        [Header(Vortex Settings)]
        _VortexMainColor("Vortex Main Color", Color) = (0.2,0.6,0.8,0.7)
        _VortexRippleColor("Vortex Ripple Color", Color) = (0.1,0.4,0.3,1)
        _VortexTexture("Vortex Texture", 2D) = "white"{} // Texture cho xoáy
        _VortexTextureStrength("Vortex Texture Strength", Range(0, 1)) = 0.5
        _VortexDecay("Vortex Ripple Decay", Range(0,10)) = 5
        _VortexWaveFrequency("Vortex Wave Frequency", Range(0,100)) = 25
        _VortexWaveSpeed("Vortex Wave Speed", Range(0,10)) = 3
        _VortexWaveStrength("Vortex Wave Strength", Range(0,5)) = 0.05
        _VortexDepth("Vortex Depth", Range(0, 5)) = 1.5
        _VortexSize("Vortex Size", Range(0, 1)) = 0.4
        _SpiralStrength("Spiral Strength", Range(0, 10)) = 3.0
        _SpiralTightness("Spiral Tightness", Range(0, 50)) = 15.0
        _RotationSpeed("Rotation Speed", Range(-5, 5)) = 1.0
        _SpiralDetail("Spiral Detail", Range(1, 10)) = 3.0
        _VortexEdgeSharpness("Vortex Edge Sharpness", Range(1, 10)) = 3.0
    }
    SubShader
    {
        pass
        {   
            Stencil{
                ref [_StencilRef]
                comp Equal
                pass replace
            }
            CGPROGRAM
            #include "UnityCG.cginc"
            #pragma vertex vert
            #pragma fragment frag
            
            float4 _Color, _Texture_ST;
            sampler2D _Texture;
            float _Decay, _WaveLiftTime, _WaveFrequency, _WaveSpeed, _WaveStrength;
            
            // Vortex properties
            float4 _VortexMainColor;
            float4 _VortexRippleColor;
            float _VortexDecay, _VortexWaveFrequency, _VortexWaveSpeed, _VortexWaveStrength;
            float _VortexDepth, _VortexSize;
            float _SpiralStrength, _SpiralTightness, _RotationSpeed;
            float _SpiralDetail, _VortexEdgeSharpness;
            
            //InputCentre array : xy = input centre, z = start time
            float4 _InputCentre[10]; // Ring ripples from boats
            float4 _VortexCentre[5];  // Vortex centres
            
            struct VertexInput
            {
                float4 pos: POSITION;
                float2 uv:TEXCOORD0;
                float3 normal: NORMAL;
            };
            
            struct VertexOutput
            {
                float4 pos: SV_POSITION;
                float2 uv:TEXCOORD0;
                float2 rawUV:TEXCOORD1;
                float3 worldPos: TEXCOORD2;
                float3 normal: NORMAL;
            };
            
            // ========== ORIGINAL RING RIPPLE WAVE ==========
            float Wave(float2 uv, float2 centre, float startTime)
            {
                if(startTime <=0) return 0;
                
                //discard old wave
                float age = _Time.y - startTime;
                if(age>_WaveLiftTime) return 0;
                float2 offset = uv-centre;
                float distanceFromCentre = length(offset);
                
                //wave radius grows over time
                float rippleRadius = age * _WaveSpeed;
                float wave = 1.0 - abs(distanceFromCentre - rippleRadius) * _WaveFrequency;
                wave = saturate(wave);
                //distance-based decay
                float spatialDecay = 1.0 - saturate(distanceFromCentre * _Decay);
                //applied time to decay
                float decay = spatialDecay * (1-age/_WaveLiftTime);
                
                return wave * _WaveStrength * decay;
            }
            
            // ========== VORTEX FUNCTIONS ==========
            // Hàm tính toán sóng xoáy (giảm mạnh để chỉ còn gợn sóng nhẹ)
            float CalculateSpiralRipple(float2 uv, float2 centre, float startTime)
            {
                if(startTime <= 0) return 0;
                
                float2 offset = uv - centre;
                float distanceFromCentre = length(offset);
                
                // Tính góc
                float angle = atan2(offset.y, offset.x);
                
                // Pattern xoáy đơn giản hơn
                float spiralPattern = angle * _SpiralTightness + distanceFromCentre * _VortexWaveFrequency;
                float rotation = _Time.y * _RotationSpeed;
                spiralPattern += rotation;
                
                // Sóng rất nhẹ
                float spiralWave = sin(spiralPattern - _Time.y * _VortexWaveSpeed);
                
                // Decay mạnh
                float spatialDecay = 1.0 - saturate(distanceFromCentre * _VortexDecay);
                float spiralIntensity = 1.0 - saturate(distanceFromCentre / _VortexSize);
                spiralIntensity = pow(spiralIntensity, 3);
                
                // Giảm mạnh cường độ sóng, chỉ để gợn nhẹ
                return spiralWave * _VortexWaveStrength * spatialDecay * spiralIntensity * 0.3;
            }

            // Hàm tính toán độ lõm của xoáy nước (cải thiện chi tiết)
            float CalculateVortexDepth(float2 uv, float2 centre)
            {
                float2 offset = uv - centre;
                float distanceFromCentre = length(offset);

                // Tạo độ lõm với edge sắc nét hơn
                float vortexAmount = 1.0 - saturate(distanceFromCentre / _VortexSize);
                
                // Sử dụng hàm mũ cao hơn để tạo đường cong sắc nét
                vortexAmount = pow(vortexAmount, _VortexEdgeSharpness);
                
                return vortexAmount * _VortexDepth; 
            }
            
            // Hàm tạo vân xoắn ốc chi tiết cho visual
            float CalculateSpiralPattern(float2 uv, float2 centre)
            {
                float2 offset = uv - centre;
                float distanceFromCentre = length(offset);
                
                if(distanceFromCentre < 0.001) return 1.0;
                
                // Tính góc
                float angle = atan2(offset.y, offset.x);
                
                // Tạo pattern xoắn ốc chi tiết
                float spiral = angle * _SpiralTightness + distanceFromCentre * 50.0 * _SpiralDetail;
                spiral += _Time.y * _RotationSpeed * 2.0;
                
                // Nhiều layer để tạo chi tiết
                float pattern1 = sin(spiral) * 0.5 + 0.5;
                float pattern2 = sin(spiral * 2.3 + 1.5) * 0.3 + 0.5;
                float pattern3 = sin(spiral * 0.7 - 0.8) * 0.2 + 0.5;
                
                float combinedPattern = pattern1 * 0.6 + pattern2 * 0.25 + pattern3 * 0.15;
                
                // Fade out theo khoảng cách
                float radialFade = 1.0 - saturate(distanceFromCentre / _VortexSize);
                radialFade = pow(radialFade, 2);
                
                return lerp(0.5, combinedPattern, radialFade);
            }
            
            // Hàm tạo biến dạng xoắn (twist) cho mesh
            float2 ApplySpiralTwist(float2 uv, float2 centre, float depth)
            {
                float2 offset = uv - centre;
                float distanceFromCentre = length(offset);
                
                if(distanceFromCentre < 0.001) return uv;
                
                // Góc xoắn phụ thuộc vào độ sâu và khoảng cách
                float twistAmount = depth * _SpiralStrength * 0.1;
                twistAmount *= (1.0 - saturate(distanceFromCentre / _VortexSize));
                
                // Xoay offset
                float cosAngle = cos(twistAmount);
                float sinAngle = sin(twistAmount);
                float2 rotatedOffset;
                rotatedOffset.x = offset.x * cosAngle - offset.y * sinAngle;
                rotatedOffset.y = offset.x * sinAngle + offset.y * cosAngle;
                
                return centre + rotatedOffset;
            }
            
            VertexOutput vert(VertexInput i)
            {
                VertexOutput o;
                float totalHeight = 0;
                
                // === ORIGINAL RING RIPPLES (from boats) ===
                float combinedWave = 0;
                UNITY_LOOP
                for(int n = 0; n < 10; n++)
                {
                    combinedWave += Wave(i.uv, _InputCentre[n].xy, _InputCentre[n].z);
                }
                totalHeight += combinedWave * 0.5;
                
                // === VORTEX EFFECTS ===
                float totalVortexDepth = 0;
                float totalSpiralRipple = 0;
                float2 currentUV = i.uv;
                
                UNITY_LOOP
                for(int v = 0; v < 5; v++)
                {
                    if(_VortexCentre[v].z <= 0) continue;
                    
                    float vortexDepth = CalculateVortexDepth(currentUV, _VortexCentre[v].xy);
                    totalVortexDepth += vortexDepth;
                    
                    float2 twistedUV = ApplySpiralTwist(currentUV, _VortexCentre[v].xy, vortexDepth);
                    totalSpiralRipple += CalculateSpiralRipple(twistedUV, _VortexCentre[v].xy, _VortexCentre[v].z);
                    
                    currentUV = twistedUV;
                }
                
                // Combine: ring waves + spiral waves - vortex depth
                totalHeight += totalSpiralRipple - totalVortexDepth;
                
                // Offset vertex height by combined wave
                i.pos.y = totalHeight;
                
                o.pos = UnityObjectToClipPos(i.pos);
                o.worldPos = mul(unity_ObjectToWorld, i.pos).xyz;
                o.normal = UnityObjectToWorldNormal(i.normal);
                o.rawUV = i.uv;
                o.uv = TRANSFORM_TEX(i.uv, _Texture);
                return o;
            }
            
            float4 frag(VertexOutput o):SV_TARGET
            {
                float4 tex = tex2D(_Texture, o.uv);
                float4 finalColor = _Color;
                
                // === ORIGINAL RING RIPPLE COLOR ===
                float combinedWave = 0;
                UNITY_LOOP
                for(int n = 0; n < 10; n++)
                {
                    if(combinedWave > 1.0) continue;
                    combinedWave += Wave(o.rawUV, _InputCentre[n].xy, _InputCentre[n].z);
                }
                
                // Apply original ring ripple effect
                finalColor = max(0.0, saturate(combinedWave * _Color) + tex);
                
                // === VORTEX COLOR EFFECTS ===
                UNITY_LOOP
                for(int v = 0; v < 5; v++)
                {
                    if(_VortexCentre[v].z <= 0) continue;
                    
                    // Tính toán độ lõm để tạo màu đậm hơn ở tâm xoáy
                    float vortexDepth = CalculateVortexDepth(o.rawUV, _VortexCentre[v].xy);
                    
                    // Tính toán sóng xoáy cho màu sắc
                    float2 twistedUV = ApplySpiralTwist(o.rawUV, _VortexCentre[v].xy, vortexDepth);
                    float spiralValue = CalculateSpiralRipple(twistedUV, _VortexCentre[v].xy, _VortexCentre[v].z);
                    
                    // Vortex mask - chỉ ảnh hưởng trong vùng xoáy
                    float vortexMask = 1.0 - saturate(length(o.rawUV - _VortexCentre[v].xy) / _VortexSize);
                    vortexMask = pow(vortexMask, 2);
                    
                    // Tính pattern xoắn ốc chi tiết
                    float spiralPattern = CalculateSpiralPattern(o.rawUV, _VortexCentre[v].xy);
                    
                    // Blend vortex main color với gradient
                    float depthGradient = vortexDepth / _VortexDepth;
                    float3 vortexColor = lerp(_VortexMainColor.rgb, _VortexMainColor.rgb * 0.4, depthGradient);
                    finalColor.rgb = lerp(finalColor.rgb, vortexColor, vortexMask * 0.6);
                    
                    // Apply spiral pattern như texture overlay
                    finalColor.rgb *= lerp(1.0, spiralPattern * 1.2, vortexMask * 0.8);
                    
                    // Thêm rim highlight ở rìa xoáy
                    float rimEffect = pow(1.0 - vortexMask, 4) * vortexMask * 2.0;
                    finalColor.rgb += _VortexRippleColor.rgb * rimEffect * 0.3;
                    
                    // Làm tối tâm xoáy
                    float centerDarkening = pow(vortexMask, 4);
                    finalColor.rgb *= lerp(1.0, 0.3, centerDarkening);
                }
                
                return finalColor;
            }
            ENDCG
        }
    }
}