Shader "Custom/AlphaBlendZWrite"
{
	Properties
	{
		_Color("Main Tint",Color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
		_AlphaScale("Alpha Scale",Range(0,1)) = 1
	}
	SubShader
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="true" "RenderType"="Transparent"}
		
		Pass{
			ZWrite On
			ColorMask 0
		}
		Pass
		{
			Tags{"LightMode"="ForwardBase"}
 
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
 
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "Lighting.cginc"
 
			struct a2f{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 texcoord : TEXCOORD0;
			};
 
			struct v2f{
				float4 pos : POSITION;
				float3 worldNormal : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
				float2 uv : TEXCOORD2;
			};
 
			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Color;
			fixed _AlphaScale;
 
			v2f vert(a2f v){
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				o.worldPos = mul(unity_ObjectToWorld,v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord,_MainTex);
				return o;
			} 			
 
			fixed4 frag(v2f i):SV_TARGET0{
				fixed3 worldNormal = normalize(i.worldNormal);
				fixed3 worldLightDir = UnityWorldSpaceLightDir(i.worldPos);
 
				float4 texColor = tex2D(_MainTex,i.uv);
				fixed3 albedo = texColor.rgb * _Color.rgb;
 
				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * albedo;
 
				fixed3 diffuse = _LightColor0.rgb * albedo * max(0,dot(worldNormal,worldLightDir));
 
				return fixed4(diffuse + ambient,_AlphaScale*texColor.a);
			}
			
			ENDCG
		}
	}
	Fallback "Diffuse"
}