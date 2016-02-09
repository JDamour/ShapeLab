Shader "Custom/ToolRadius"
{
	Properties
	{
		_Color("Color", Color) = (1.0,1.0,1.0,1.0)
		_Radius("Radius", Float) = 5.0
		_Transparency("Transparency", Float) = 1.0
	}
	SubShader
	{
		Tags {"Queue" = "Transparent"  "RenderType" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag alpha
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float2 uv : TEXCOORD0;
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			float4 _Color;
			float _Radius;
			float _Transparency;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.normal = v.normal;
				o.uv = v.uv;
				float radius = _Radius / 5.0;
				o.vertex = mul(UNITY_MATRIX_MVP, float4(v.vertex.rgb*radius,1.0));
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				if (_Transparency == 0) discard;
				// sample the texture
				fixed4 col = _Color * (dot(i.normal, normalize(float3(0.08,-0.14,0.4)))*0.5 +0.5);
				col.a = _Transparency;
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
