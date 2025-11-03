Shader "Custom/RippleInSquare"
{
    Properties
    {
        _MainColor("Main Color", Color) = (0.2,0.6,0.8,0.7) // Màu nền nước (có alpha)
        _RippleColor("Ripple Color", Color) = (0.1,0.4,0.3,1) // Màu gợn sóng
        _Texture("Noise Texture (Optional)", 2D) = "white"{} // Dùng cho vân bề mặt nếu muốn
        
        _FlowDirection("Flow Direction (XY)", Vector) = (0, -1, 0, 0) // Hướng chảy (mặc định xuống dưới)
        _FlowSpeed("Flow Speed", Range(0,10)) = 2 // Tốc độ chảy
        _FlowWidth("Flow Width", Range(0, 1)) = 0.15 // Độ rộng của dòng chảy
        
        _WaveFrequency("Wave Frequency", Range(0,100)) = 25 // Tần số vòng sóng
        _WaveSpeed("Wave Speed", Range(0,10)) = 3 // Tốc độ sóng (để tương thích với script)
        _WaveStrength("Wave Strength (Height)", Range(0,5)) = 0.3 // Độ cao/sâu của sóng
        
        // Hiệu ứng uốn lượn
        _WaveDistortion("Wave Distortion", Range(0, 1)) = 0.3 // Độ uốn cong của sóng
        _DistortionFreq("Distortion Frequency", Range(0, 20)) = 5 // Tần số uốn cong
        _DistortionSpeed("Distortion Speed", Range(0, 5)) = 1 // Tốc độ biến dạng
        
        // Kiểm soát phạm vi lan rộng của sóng
        _WaveDecayRate("Wave Decay Rate", Range(0, 5)) = 1.5 // Tốc độ phai của sóng
        
        // Kích thước ô vuông
        _SquareSize("Square Size", Range(0, 1)) = 0.3 // Kích thước ô vuông (bán kính từ tâm)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        
        pass
        {    
            CGPROGRAM
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            #pragma vertex vert
            #pragma fragment frag

            float4 _MainColor;
            float4 _RippleColor;
            sampler2D _Texture;
            float4 _Texture_ST;
            float4 _FlowDirection;
            float _FlowSpeed, _FlowWidth;
            float _WaveFrequency, _WaveSpeed, _WaveStrength;
            float _WaveDistortion, _DistortionFreq, _DistortionSpeed;
            float _WaveDecayRate;
            float _SquareSize;

            //InputCentre array : xy = input centre, z = start time
            float4 _InputCentre; // xy = center, z = start time

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
            
            // Kiểm tra xem điểm có nằm trong ô vuông hay không
            float IsInsideSquare(float2 uv, float2 centre, float size)
            {
                // Tính khoảng cách theo Chebyshev (L∞ norm) - tạo hình vuông thật sự
                float2 offset = abs(uv - centre);
                float maxDist = max(offset.x, offset.y);
                return step(maxDist, size);
            }
            
            // Hàm tính toán sóng chảy theo một hướng với hiệu ứng uốn lượn
            float CalculateDirectionalFlow(float2 uv, float2 centre, float startTime)
            {
                if(startTime <= 0) return 0; // Sóng chưa kích hoạt
                
                // Kiểm tra xem có nằm trong ô vuông không
                float squareMask = IsInsideSquare(uv, centre, _SquareSize);
                if(squareMask == 0) return 0; // Ngoài ô vuông thì không có sóng
                
                float age = _Time.y - startTime;
                
                float2 offset = uv - centre;
                
                // Chuẩn hóa hướng chảy
                float2 flowDir = normalize(_FlowDirection.xy);
                
                // Tính khoảng cách dọc theo hướng chảy (projection)
                float distanceAlongFlow = dot(offset, flowDir);
                
                // Tính khoảng cách vuông góc với hướng chảy
                float2 perpDir = float2(-flowDir.y, flowDir.x);
                float distancePerpendicular = dot(offset, perpDir);
                
                // Thêm hiệu ứng uốn lượn (sine wave) cho sóng
                float wavyDistortion = sin(distanceAlongFlow * _DistortionFreq + _Time.y * _DistortionSpeed) * _WaveDistortion;
                
                // Áp dụng biến dạng vào khoảng cách vuông góc
                float distortedPerpendicular = abs(distancePerpendicular + wavyDistortion);
                
                // Chỉ tính sóng nếu trong phạm vi độ rộng của dòng chảy
                float widthMask = 1.0 - saturate(distortedPerpendicular / _FlowWidth);
                
                // Thêm nhiễu bổ sung cho sóng chính
                float noiseDistortion = sin(distancePerpendicular * _DistortionFreq * 2 - _Time.y * _DistortionSpeed * 0.7) * _WaveDistortion * 0.5;
                
                // Sóng di chuyển theo hướng chảy với biến dạng
                float wavePosition = distanceAlongFlow + noiseDistortion - _Time.y * _FlowSpeed;
                float wave = cos(wavePosition * _WaveFrequency) * 0.5 + 0.5;
                
                // Thêm sóng phụ với tần số khác để tạo độ phức tạp
                float secondaryWave = cos(wavePosition * _WaveFrequency * 1.7 + wavyDistortion * 3) * 0.3 + 0.5;
                wave = wave * 0.7 + secondaryWave * 0.3;
                
                // Sóng chỉ xuất hiện phía sau điểm khởi đầu (theo hướng chảy)
                float flowMask = smoothstep(0, 0.1, distanceAlongFlow);
                
                // Phai dần theo khoảng cách
                float decay = exp(-distanceAlongFlow * _WaveDecayRate);
                
                // Thêm giới hạn khoảng cách tối đa
                float maxDistance = _SquareSize * 1.5;
                float distanceMask = 1.0 - saturate(distanceAlongFlow / maxDistance);
                
                return wave * _WaveStrength * widthMask * flowMask * decay * distanceMask * squareMask;
            }

            VertexOutput vert(VertexInput i)
            {
                VertexOutput o;

                // Tính toán hiệu ứng dòng chảy theo hướng
                float flow = CalculateDirectionalFlow(i.uv, _InputCentre.xy, _InputCentre.z);
                
                // Áp dụng vào vị trí Y của đỉnh
                i.pos.y += flow;

                o.pos = UnityObjectToClipPos(i.pos);
                o.worldPos = mul(unity_ObjectToWorld, i.pos).xyz;
                o.normal = UnityObjectToWorldNormal(i.normal);
                o.rawUV = i.uv;
                o.uv = TRANSFORM_TEX(i.uv, _Texture);
                return o;
            }

            float4 frag(VertexOutput o):SV_TARGET
            {
                // Lấy màu nền nước và áp dụng độ trong suốt
                float4 finalColor = _MainColor;
                
                // Lấy màu texture nếu có (có thể là noise)
                float4 texColor = tex2D(_Texture, o.uv);

                // Tính toán lại dòng chảy cho màu sắc
                float flowValue = CalculateDirectionalFlow(o.rawUV, _InputCentre.xy, _InputCentre.z);

                // Cộng màu của dòng chảy lên màu nền
                finalColor.rgb = lerp(finalColor.rgb, _RippleColor.rgb, flowValue * 2); 

                return finalColor;
            }

            ENDCG
        }
    }
}