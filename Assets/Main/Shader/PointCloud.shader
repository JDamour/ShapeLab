Shader "Custom/PointCloud" {
	Properties{
		_PointSize("PointSize", Float) = 10
	}
	SubShader{
		Pass{
		LOD 200

		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag

		struct VertexInput {
			float4 vertex : POSITION;
			float4 color: COLOR;
		};

		struct VertexOutput {
			float4 pos : SV_POSITION;
			float4 color : COLOR;
			float size : PSIZE;
		};

		float _PointSize;

		VertexOutput vert(VertexInput v) {

			VertexOutput o;
			o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			o.size = _PointSize;
			o.color = v.color;

			return o;
		}

		float4 frag(VertexOutput o) : COLOR{
			return o.color;
		}

		ENDCG
		}
	}
}
