﻿Shader "Unlit/451NoCullShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100
		Cull Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			sampler2D _MainTex;
			
			float MyTexOffset_X;
			float MyTexOffset_Y;
			float MyTexScale_X;
			float MyTexScale_Y;
			//float MyTexRotationZ;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);   // World to NDC

				// o.uv = TRANSFORM_TEX(v.uv, _MainTex);   // WHAT IS THIS DOING?
				// replace with the following
					o.uv.x = v.uv.x * MyTexScale_X + MyTexOffset_X;
					o.uv.y = v.uv.y * MyTexScale_Y + MyTexOffset_Y;
				o.normal = v.normal; // NOTE: this is in the original world space!!
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				col = 0.5 * col + 0.5 * fixed4(i.normal, 1.0);
				return col;
			}
			ENDCG
		}
	}
}