using UnityEngine;

public class GrainLightFilter : MonoBehaviour
{
    public string lightNamePrefix = "GrainLight_"; // Prefix for the light name

    private Material _grainMaterial; // The material using the shader

    void Start()
    {
        // Get the material from the Renderer component attached to this GameObject
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            _grainMaterial = renderer.material; // Use the material already assigned to the object
        }
        else
        {
            Debug.LogError("No Renderer component found on this GameObject.");
            return;
        }

        // Find all lights in the scene using FindObjectsByType
        Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);

        // Collect indices of lights with the matching name
        Vector4 targetLightIndices = new Vector4(-1, -1, -1, -1); // Initialize with invalid indices
        int indexCount = 0;

        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i].name.StartsWith(lightNamePrefix))
            {
                if (indexCount < 4) // Only store up to 4 indices
                {
                    targetLightIndices[indexCount] = (i - 1);
                    indexCount++;
                }
                else
                {
                    Debug.LogWarning("More than 4 matching lights found. Only the first 4 will be used.");
                    break;
                }
            }
        }

        Debug.Log("Target light indeces" + targetLightIndices);

        // Pass the target light indices to the shader
        _grainMaterial.SetVector("_TargetLightIndices", targetLightIndices);
    }
}