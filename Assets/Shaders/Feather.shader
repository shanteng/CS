// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//ps的羽化shader
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'
Shader "ShaderDemo/Feather"
{
    Properties
    {
        _Color("Main Tint",Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
        _size("size",Vector) = (0.2,0.2,0,0)
        _scale("menglong",Range(0,1)) = 0.01
        _alpha("alpha",Range(0,1))=1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _size;
            float _scale;
            float _alpha;
            fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
              
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                fixed2 size = fixed2(_size.x,_size.y);
                fixed2 dis2 = (fixed2(_size.z,_size.w) - i.uv);
                float dx = dis2.x/size.x;
                float dy = dis2.y/size.y;
                float a = sqrt(dx*dx+dy*dy);

  
                col.a = 1-pow(a,8);
                col.r =clamp(col.r+a*_scale,0,1);
                col.g =clamp(col.g+a*_scale,0,1);
                col.b =clamp(col.b+a*_scale,0,1); 

                col.a *= _Color.rgb;
                col.a *= _alpha;

                fixed3 albedo = col.rgb * _Color.rgb;
                fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * albedo;
                fixed3 diffuse = _LightColor0.rgb * albedo;
                return fixed4(diffuse + ambient,col.a);
                //return col;
            }
            ENDCG
        }
    }
}
