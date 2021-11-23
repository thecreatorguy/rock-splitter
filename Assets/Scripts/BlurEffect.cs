using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlurEffect : MonoBehaviour
{
    public Material material;
    public RockHandler handler;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (handler.State != RockHandler.HandlerState.Focused)
        {
            Graphics.Blit(source, destination);
            return;
        }

        Graphics.Blit(source, destination, material);
    }
    
}
