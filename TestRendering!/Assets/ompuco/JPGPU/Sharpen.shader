Shader "Hidden/Sharpen"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Sharpness ("Sharpness", Range(0.0,100.0)) = 1.0
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag





            float3x3 rgb2yuv = float3x3(
                0.299,-0.14713, 0.615,
                0.587,-0.28886,-0.51499,
                0.114,0.436,-0.10001);

            float3x3 yuv2rgb = float3x3(
                1, 1, 1,
                0.0,-0.394,2.03211,
                1.13983,-0.580,0.0);








// YUV, generic conversion
// ranges: Y=0..1, U=-uvmax.x..uvmax.x, V=-uvmax.x..uvmax.x

float3 yuv_rgb (float3 yuv, float2 wbwr, float2 uvmax) {
    float2 br = yuv.x + yuv.yz * (1.0 - wbwr) / uvmax;
	float g = (yuv.x - dot(wbwr, br)) / (1.0 - wbwr.x - wbwr.y);
	return float3(br.y, g, br.x);
}

float3 rgb_yuv (float3 rgb, float2 wbwr, float2 uvmax) {
	float y = wbwr.y*rgb.r + (1.0 - wbwr.x - wbwr.y)*rgb.g + wbwr.x*rgb.b;
    return float3(y, uvmax * (rgb.br - y) / (1.0 - wbwr));
}


//----------------------------------------------------------------------------

// Y*b*r, generic conversion
// ranges: Y=0..1, b=-0.5..0.5, r=-0.5..0.5



float3 ypbpr_rgb (float3 ybr, float2 kbkr) {
    return yuv_rgb(ybr, kbkr, float2(0.5,.5));
}
    
float3 rgb_ypbpr (float3 rgb, float2 kbkr) {
    return rgb_yuv(rgb, kbkr, float2(0.5,.5));
}

//----------------------------------------------------------------------------

// YPbPr, analog, gamma compressed, HDTV
// ranges: Y=0..1, b=-0.5..0.5, r=-0.5..0.5

// YPbPr to RGB, after ITU-R BT.709
float3 ypbpr_rgb (float3 ypbpr) {
    return ypbpr_rgb(ypbpr, float2(0.0722, 0.2126));
}

// RGB to YPbPr, after ITU-R BT.709
float3 rgb_ypbpr (float3 rgb) {
    return rgb_ypbpr(rgb, float2(0.0722, 0.2126));
}

//----------------------------------------------------------------------------






            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _Sharpness;
            float _BufferDownscale;

            fixed4 frag (v2f i) : SV_Target
            {
                //fixed4 col = tex2D(_MainTex, i.uv);


                float2 step = 2.0 / _MainTex_TexelSize.zw;
                //i.uv=round(i.uv*_MainTex_TexelSize.zw+.5)/_MainTex_TexelSize.zw;


                /*
                float3 texA = tex2D( _MainTex, (floor(_MainTex_TexelSize.zw*(i.uv + float2(-step.x, -step.y) ))+.5)/_MainTex_TexelSize.zw).rgb;
                float3 texB = tex2D( _MainTex, (floor(_MainTex_TexelSize.zw*(i.uv + float2( step.x, -step.y) ))+.5)/_MainTex_TexelSize.zw).rgb;
                float3 texC = tex2D( _MainTex, (floor(_MainTex_TexelSize.zw*(i.uv + float2(-step.x,  step.y) ))+.5)/_MainTex_TexelSize.zw).rgb;
                float3 texD = tex2D( _MainTex, (floor(_MainTex_TexelSize.zw*(i.uv + float2( step.x,  step.y) ))+.5)/_MainTex_TexelSize.zw).rgb;
                */
	
                float3 texA = tex2D( _MainTex, i.uv + float2(-step.x, -step.y) ).rgb;
                float3 texB = tex2D( _MainTex, i.uv + float2( step.x, -step.y) ).rgb;
                float3 texC = tex2D( _MainTex, i.uv + float2(-step.x,  step.y) ).rgb;
                float3 texD = tex2D( _MainTex, i.uv + float2( step.x,  step.y) ).rgb;
                texA=rgb_ypbpr(texA);
                texB=rgb_ypbpr(texB);
                texC=rgb_ypbpr(texC);
                texD=rgb_ypbpr(texD);
            
                float3 around = 0.25 * (texA + texB + texC + texD);
                float3 center  = tex2D( _MainTex, i.uv ).rgb;
                center=rgb_ypbpr(center);
                
                float sharpness = pow(_Sharpness,1.15);
                
                
                //float4 col = float4(ypbpr_rgb(center + (center - around) * sharpness),1);
                float y = (center + (center - around) * sharpness).x;

                float4 col = float4(ypbpr_rgb(float3(y,center.gb)),1.0);


                float3 W = float3(0.2125, 0.7154, 0.0721);
                float3 intensity = float3(dot(col.rgb, W).xxx);
                //col.rgb= lerp(intensity, col.rgb, 1);

                
                return col;
            }
            ENDCG
        }
    }
}
