
Shader "Custom/DrawBuffer"
{
	Properties{
		_color ("Object Color", Color) = (.34, .85, .92, 1)
		_lightDir("Light Direction", Vector) = (0.5,0.2,0.2)
		_shininess("Shininess", Float) = 1.0

	}
	SubShader
	{
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		//Blend SrcAlpha OneMinusSrcAlpha
		Pass
		{
			Cull front

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma target 5.0
			#pragma vertex vert
			#pragma fragment frag

			struct Vert
			{
				float3 position;
				float3 normal;
				float3 color;
			};

			uniform StructuredBuffer<Vert> vertexBuffer;

			struct v2f
			{
				float4  pos : SV_POSITION;
				float3 normal : Color;
				float4 color : Color1;
			};

			v2f vert(uint id : SV_VertexID)
			{
				Vert vert = vertexBuffer[id];

				v2f OUT;
				OUT.pos = mul(UNITY_MATRIX_MVP, float4(vert.position.xyz, 1));
				OUT.normal = vert.normal;
				OUT.color = float4(vert.color.r, vert.color.g, vert.color.b,1.0);

				return OUT;
			}

			float3 _lightDir;
			float _shininess;
			float4 _color;

			float4 frag(v2f IN) : COLOR
			{
				// TODO: specular calculation
				//float specular = pow(dot(reflect(normalize(ObjSpaceViewDir(IN.pos)), IN.normal),_lightDir),_shininess);
				float diffuse = dot(IN.normal, normalize(_lightDir));
				return IN.color * diffuse;
			}

			ENDCG
		}
	}
}