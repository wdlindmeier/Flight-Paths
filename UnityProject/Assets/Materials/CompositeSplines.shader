Shader "Custom/CompositeSplines"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_RenderTexture ("Render Texture", 2D) = "black" {}
		//_keyingColor ("KeyColour", Color) = (0,1,1,1)
		//_thresh ("Threshold", Range (0, 16)) = 0.65
		//_slope ("Slope", Range (0, 1)) = 0.63
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
			sampler2D _RenderTexture;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 input_color = tex2D(_RenderTexture, i.uv);
				fixed4 colFX = tex2D(_MainTex, i.uv);			
				fixed4 additive = min(input_color + (colFX * colFX.a), float4(1,1,1,1));
				fixed4 multiply = lerp(input_color, colFX, colFX.a * 0.5);
				return additive;//lerp(multiply, additive, 0.25);
			}
			ENDCG
		}
	}
}
