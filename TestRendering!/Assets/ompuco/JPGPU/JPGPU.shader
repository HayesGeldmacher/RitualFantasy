//JPGPU:
//a JPEG compression post post processing effect
//by ompu co | Sam Blye (c) 2021, all rights reserved


Shader "Hidden/JPGPU"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" { }

		_BS ("Block Size", Range(1, 128)) = 8
		_Quality ("Quality", Range(0, 16)) = 1
		_H ("H", float) = .5
		_G ("Hg", vector) = (2, 1, 2, 2)
		_Truncation ("Truncation", Range(.01, 10)) = .01
		_Contrast ("Contrast", float) = 0
	}
	SubShader
	{


		CGINCLUDE

		float nrand(float x, float y)
		{
			return frac(sin(dot(float2(x, y), float2(12.9898, 78.233))) * 43758.5453);
		}

		float _H;
		int _BS;
		float _Quality;
		float4 _G;
		float _Contrast;

		#define BS _BS

		static const float PI = radians(180.0);
		static const float BSf = float(BS);

		float basis1D(float k, float i)
		{
			return k == 0 ? sqrt(1. / BSf) :
			sqrt((_G.w + _Contrast) / BSf) * cos(float((_G.x * i + _G.y) * k) * PI / (_G.z * BSf));
		}

		float basis2D(float2 jk, float2 xy)
		{
			return basis1D(jk.x, xy.x) * basis1D(jk.y, xy.y);
		}


		float4 rgbToYUV(float4 rgba)
		{
			return mul(
				float4x4(
					0.299, 0.587, 0.114, 0.0,
					- 0.169, -0.331, 0.5, 0.5,
					0.5, -0.419, -0.091, 0.5,
					0.0, 0.0, 0.0, 1.0),
					rgba);
			}
			float4 yuvToRGB(float4 yuva)
			{
				return mul(
					float4x4(
						1.014, 0.0239, 1.4017, -0.7128,
						0.9929, -0.3564, -0.7142, 0.5353,
						1.0, 1.7722, 0.001, -0.8866,
						0.0, 1.7722, 0.0, 1.0),
						yuva);
				}

				float4 YUVRGB(float4 col, int m)
				{
					if (m)
						return yuvToRGB(col);
					else
						return rgbToYUV(col);
				}




				static const float quality = length(float2(_Quality, _Quality));



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

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = v.uv;
					return o;
				}

				sampler2D _MainTex;
				float4 _MainTex_TexelSize;
				int _JPGDS;
				int _DS;
				int _Truncate;
				float _Truncation;

				float2 jM(float2 iB, float2 xy, int m)
				{
					return lerp(iB, xy, m);
				}

				float4 jpg(v2f i, int m)
				{


					float4 fragColor = float4(0, 0, 0, 1);

					i.uv.xy -= .5 * _DS / _ScreenParams.xy;

					

					_ScreenParams.xy = lerp(floor(_ScreenParams.xy / 2) * 2, floor(_ScreenParams.xy / (4)) * 2, _DS);
					
					float2 coords = int2(i.uv * (_ScreenParams.xy));

					float2 inBlock = coords % BS - m * _H,
					block = coords - inBlock;

					
					for (int2 xy = 0; xy.x < BS; xy.x++)
					{
						for (xy.y = 0; xy.y < BS; xy.y++)
						{
							fragColor += tex2D(_MainTex, (block + xy) / _ScreenParams.xy) * basis2D(jM(inBlock, xy, m), jM(inBlock, xy, 1 - m));

						}
					}
					fragColor *= lerp(step(length(float2(inBlock)), quality), 1.0, m);


					return fragColor;
				}




				//Experimental SDR version for hardware that can't use HDR buffers
				float4 jpgSDR(v2f i, int m)
				{

					float4 fragColor = float4(0, 0, 0, 1);

					i.uv.xy -= .5 * _DS / _MainTex_TexelSize.zw;

					_MainTex_TexelSize.zw = lerp(floor(_MainTex_TexelSize.zw / 2) * 2, floor(_MainTex_TexelSize.zw / (4)) * 2, _DS);
					
					float2 coords = int2(i.uv * (_MainTex_TexelSize.zw));

					float2 inBlock = coords % BS - m * _H,
					block = coords - inBlock;

						
					float scale = 32.0;


					for (int2 xy = 0; xy.x < BS; xy.x++)
					{
						for (xy.y = 0; xy.y < BS; xy.y++)
						{
							float4 cl = tex2D(_MainTex, (block + xy) / _MainTex_TexelSize.zw);///pow(BS*_H,2);
							fragColor += lerp(cl, cl * scale - (.25 * scale), m) * basis2D(jM(inBlock, xy, m), jM(inBlock, xy, 1 - m));
						}
					}
					fragColor *= lerp(step(length(float2(inBlock)), quality), 1.0, m);
					fragColor *= lerp(clamp(step(coords.x, _MainTex_TexelSize.z) + step(coords.y, _MainTex_TexelSize.w), 0.0, 1.0), 1.0, 0);




					return clamp(lerp(fragColor / scale + .25, fragColor, m), 0, 1);
				}






				ENDCG

				// No culling or depth
				Cull Off ZWrite Off ZTest Always

				Pass
				{
					CGPROGRAM

					#pragma vertex vert
					#pragma fragment frag
					

					

					fixed4 frag(v2f i) : SV_Target
					{


						int c = step(i.uv.x, 1);
						float4 f = jpg(i, 0);
						_Truncation = log10(1 + _Truncation);
						_Truncate = step(.1, _Truncation);
						return lerp(f, round(f / _Truncation) * _Truncation, _Truncate);
					}
					ENDCG

				}


				Pass
				{
					CGPROGRAM

					#pragma vertex vert
					#pragma fragment frag
					

					

					fixed4 frag(v2f i) : SV_Target
					{
						
						float2 uv = i.uv.xy;
						i.uv.xy += .5001 / _ScreenParams.xy;//.zw;
						i.uv.xy -= .5001 / _ScreenParams.xy;//.zw;
						
						float4 f = jpg(i, 1);
						return f;
					}
					ENDCG

				}

				Pass
				{

					
					CGPROGRAM

					#pragma vertex vert
					#pragma fragment frag
					

					

					fixed4 frag(v2f i) : SV_Target
					{

						//next two lines fix the position of pixels,
						//as it's offset during compression process
						i.uv.xy += _DS * 1 / _ScreenParams.xy;
						i.uv.xy += 1 / _ScreenParams.xy;
						return tex2D(_MainTex, i.uv);
					}
					ENDCG

				}
			}
		}
