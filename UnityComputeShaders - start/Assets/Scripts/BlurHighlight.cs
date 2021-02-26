using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BlurHighlight : BaseCompletePP
{
    private const string cs_HorzPass = "HorzPass";
    private const string cs_Highlight = "Highlight";
    private const string cs_horzOutput = "horzOutput";
    private const string cs_output = "output";
    private const string cs_source = "source";
    private const string cs_radius = "radius";
    private const string cs_edgeWidth = "edgeWidth";
    private const string cs_shade = "shade";
    private const string cs_center = "center";
    private const string cs_blurRadius = "blurRadius";

    [Range(0, 50)]
    public int blurRadius = 20;
    [Range(0.0f, 100.0f)]
    public float radius = 10;
    [Range(0.0f, 100.0f)]
    public float softenEdge = 30;
    [Range(0.0f, 1.0f)]
    public float shade = 0.5f;
    public Transform trackedObject;

    Vector4 center;
    RenderTexture horzOutput = null;
    int kernelHorzPassID;

    protected override void Init()
    {
        center = new Vector4();
        kernelName = cs_Highlight;
        base.Init();
        kernelHorzPassID = shader.FindKernel(cs_HorzPass);

    }

    protected override void CreateTextures()
    {
        base.CreateTextures();
        shader.SetTexture(kernelHorzPassID, cs_source, renderedSource);
        CreateTexture(ref horzOutput);
        shader.SetTexture(kernelHorzPassID, cs_horzOutput, horzOutput);
        shader.SetTexture(kernelHandle, cs_horzOutput, horzOutput);
    }

    private void OnValidate()
    {
        if(!init)
            Init();
           
        SetProperties();
    }

    protected void SetProperties()
    {
        float rad = (radius / 100.0f) * texSize.y;
        shader.SetFloat(cs_radius, rad);
        shader.SetFloat(cs_edgeWidth, rad * softenEdge / 100.0f);
        shader.SetFloat(cs_shade, shade);
        shader.SetInt(cs_blurRadius, blurRadius);
    }

    protected override void DispatchWithSource(ref RenderTexture source, ref RenderTexture destination)
    {
        if (!init) return;

        Graphics.Blit(source, renderedSource);

        shader.Dispatch(kernelHorzPassID, groupSize.x, groupSize.y, 1);
        shader.Dispatch(kernelHandle, groupSize.x, groupSize.y, 1);

        Graphics.Blit(output, destination);
    }

    protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (shader == null)
        {
            Graphics.Blit(source, destination);
        }
        else
        {
            if (trackedObject && thisCamera)
            {
                Vector3 pos = thisCamera.WorldToScreenPoint(trackedObject.position);
                center.x = pos.x;
                center.y = pos.y;
                shader.SetVector(cs_center, center);
            }
            bool resChange = false;
            CheckResolution(out resChange);
            if (resChange) SetProperties();
            DispatchWithSource(ref source, ref destination);
        }
    }

}
