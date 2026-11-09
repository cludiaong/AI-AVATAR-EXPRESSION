using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class SkinBlendShapeModifier : MonoBehaviour
{
    SkinnedMeshRenderer skinnedMeshRenderer;
    public bool SmoothReset = true;
    void Start()
    {
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        
    }
    public void ResetBlendShape(string name)
    {
        Debug.Log($"ResetBlend {name}");
        int skinIDX = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(name);

        if (SmoothReset) 
            StartCoroutine(SmoothResetEnum(skinnedMeshRenderer, skinIDX));
        else 
            ResetBlendShape(skinIDX);
    }
    public void ResetBlendShape(int index)
    {
        if(index < 0)
        {
            Debug.Log($"No Blend {index}");
        }
        Debug.Log($"ResetBlend {index}");
        if(SmoothReset)
            StartCoroutine(SmoothResetEnum(skinnedMeshRenderer, index));
        else
        {
            skinnedMeshRenderer.SetBlendShapeWeight(index, 0);
        }
    }
    public IEnumerator SmoothResetEnum(SkinnedMeshRenderer skinnedMeshRenderer, int index)
    {
        float elapsed = 0;
        float step = 0.02f; // Time step for updating (50 FPS)
        float duration = 1f;
        float start = skinnedMeshRenderer.GetBlendShapeWeight(index);
        float end = 0;
        while (elapsed < duration)
        {
            float t = elapsed / duration; // Normalize time (0 to 1)
            float easedValue = start + (end - start) * MathF.Sin((t * MathF.PI) / 2);

            skinnedMeshRenderer.SetBlendShapeWeight(index, easedValue);

            yield return new WaitForFixedUpdate();
            elapsed += step;
        }
    }
}
