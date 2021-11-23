Shader "Blur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            Name "HORIZONTAL_BLUR"

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
            float2 _MainTex_TexelSize;

            fixed4 frag (v2f i) : SV_Target
            {
                // Gaussian blur, I forget where I got these constants from
                const float weight[] = {
                    0.2270270270, 0.1945945946, 0.1216216216,
                    0.0540540541, 0.0162162162
                };

                float2 uv = i.uv;
                float3 col = tex2D(_MainTex, uv) * weight[0];
                for (int i = 1; i < 5; i++) {
                    col += tex2D(_MainTex, uv + float2(i, 0) * _MainTex_TexelSize) * weight[i];
                    col += tex2D(_MainTex, uv - float2(i, 0) * _MainTex_TexelSize) * weight[i];
                }
                
                return fixed4(col, 1.0);
            }
            ENDCG
        }

        GrabPass {"_GrabTex"}

        Pass
        {
            Name "VERTICAL_BLUR"

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

            sampler2D _GrabTex;
            float2 _GrabTex_TexelSize;

            fixed4 frag (v2f i) : SV_Target
            {
                const float weight[] = {
                    0.2270270270, 0.1945945946, 0.1216216216,
                    0.0540540541, 0.0162162162
                };
                const float dimness = 0.5;

                float2 uv = i.uv;
                float3 col = tex2D(_GrabTex, uv) * weight[0];
                for (int i = 1; i < 5; i++) {
                    col += tex2D(_GrabTex, uv + float2(0, i) * _GrabTex_TexelSize) * weight[i];
                    col += tex2D(_GrabTex, uv - float2(0, i) * _GrabTex_TexelSize) * weight[i];
                }

                col *= dimness;
                
                return fixed4(col, 1.0);
            }
            ENDCG
        }
    }
}
