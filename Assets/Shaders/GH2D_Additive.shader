Shader "GH2D_Additive"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		LOD 100
		cull off
		ztest Always
		zwrite off
		blend ONE ONE
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
				float3 worldPos : TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld,v.vertex ).xyz;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			uniform half _GH_Distance, _GH_Fade;
			fixed4 frag (v2f i) : SV_Target
			{
				
				fixed4 col = tex2D(_MainTex, i.uv);
				//col.rgb = i.worldPos;
				col.rgb *= (1 - saturate((i.worldPos.z - _GH_Distance) / _GH_Fade));
				col.a = max(max(col.r,col.g),col.b);
				return col;
			}
			ENDCG
		}
	}
}
