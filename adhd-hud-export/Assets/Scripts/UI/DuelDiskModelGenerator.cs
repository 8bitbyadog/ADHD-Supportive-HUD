using UnityEngine;

public class DuelDiskModelGenerator : MonoBehaviour
{
    [Header("Duel Disk Dimensions")]
    [SerializeField] private float baseWidth = 0.3f;
    [SerializeField] private float baseHeight = 0.1f;
    [SerializeField] private float baseDepth = 0.05f;
    [SerializeField] private float cardSlotWidth = 0.08f;
    [SerializeField] private float cardSlotHeight = 0.12f;
    [SerializeField] private float cardSlotDepth = 0.02f;
    [SerializeField] private float holographicDisplayWidth = 0.25f;
    [SerializeField] private float holographicDisplayHeight = 0.15f;
    [SerializeField] private float holographicDisplayDepth = 0.01f;
    [SerializeField] private float goldenRatio = 1.618f;

    [Header("Visual Settings")]
    [SerializeField] private Color baseColor = new Color(0.1f, 0.1f, 0.15f, 1f);
    [SerializeField] private Color accentColor = new Color(0.2f, 0.8f, 1f, 1f);
    [SerializeField] private Color holographicColor = new Color(0.2f, 0.8f, 1f, 0.8f);
    [SerializeField] private float metallic = 0.9f;
    [SerializeField] private float smoothness = 0.8f;
    [SerializeField] private float emissionIntensity = 1.5f;

    [Header("Geometry Settings")]
    [SerializeField] private int baseSegments = 8;
    [SerializeField] private float baseRadius = 0.15f;
    [SerializeField] private float innerRadius = 0.1f;
    [SerializeField] private float heightOffset = 0.02f;

    public Mesh GenerateBaseMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "DuelDiskBase";

        // Create vertices for a circular base with golden ratio proportions
        Vector3[] vertices = new Vector3[baseSegments * 2 + 2]; // Outer ring + inner ring + center
        Vector2[] uvs = new Vector2[vertices.Length];
        Vector3[] normals = new Vector3[vertices.Length];

        // Center vertex
        vertices[0] = Vector3.zero;
        uvs[0] = new Vector2(0.5f, 0.5f);
        normals[0] = Vector3.up;

        // Create outer and inner rings
        for (int i = 0; i < baseSegments; i++)
        {
            float angle = i * (2f * Mathf.PI / baseSegments);
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            // Outer ring
            vertices[i + 1] = new Vector3(cos * baseRadius, heightOffset, sin * baseRadius);
            uvs[i + 1] = new Vector2(0.5f + 0.5f * cos, 0.5f + 0.5f * sin);
            normals[i + 1] = Vector3.up;

            // Inner ring
            vertices[i + baseSegments + 1] = new Vector3(cos * innerRadius, heightOffset, sin * innerRadius);
            uvs[i + baseSegments + 1] = new Vector2(0.5f + 0.3f * cos, 0.5f + 0.3f * sin);
            normals[i + baseSegments + 1] = Vector3.up;
        }

        // Create triangles for the base
        int[] triangles = new int[baseSegments * 6];
        for (int i = 0; i < baseSegments; i++)
        {
            int next = (i + 1) % baseSegments;
            
            // Outer triangles
            triangles[i * 6] = 0;
            triangles[i * 6 + 1] = i + 1;
            triangles[i * 6 + 2] = next + 1;

            // Inner triangles
            triangles[i * 6 + 3] = 0;
            triangles[i * 6 + 4] = next + baseSegments + 1;
            triangles[i * 6 + 5] = i + baseSegments + 1;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.normals = normals;
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();

        return mesh;
    }

    public Mesh GenerateCardSlotMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "CardSlot";

        // Create vertices for a hexagonal card slot
        Vector3[] vertices = new Vector3[7]; // Center + 6 points
        Vector2[] uvs = new Vector2[vertices.Length];
        Vector3[] normals = new Vector3[vertices.Length];

        // Center vertex
        vertices[0] = Vector3.zero;
        uvs[0] = new Vector2(0.5f, 0.5f);
        normals[0] = Vector3.up;

        // Create hexagonal points
        for (int i = 0; i < 6; i++)
        {
            float angle = i * (2f * Mathf.PI / 6);
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            vertices[i + 1] = new Vector3(
                cos * cardSlotWidth * 0.5f,
                heightOffset,
                sin * cardSlotHeight * 0.5f
            );
            uvs[i + 1] = new Vector2(0.5f + 0.5f * cos, 0.5f + 0.5f * sin);
            normals[i + 1] = Vector3.up;
        }

        // Create triangles for the hexagon
        int[] triangles = new int[18];
        for (int i = 0; i < 6; i++)
        {
            int next = (i + 1) % 6;
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = next + 1;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.normals = normals;
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();

        return mesh;
    }

    public Mesh GenerateHolographicDisplayMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "HolographicDisplay";

        // Create vertices for a golden ratio rectangle display
        float displayWidth = holographicDisplayWidth;
        float displayHeight = holographicDisplayWidth / goldenRatio;

        Vector3[] vertices = new Vector3[]
        {
            // Front face
            new Vector3(-displayWidth/2, -displayHeight/2, holographicDisplayDepth/2),
            new Vector3(displayWidth/2, -displayHeight/2, holographicDisplayDepth/2),
            new Vector3(displayWidth/2, displayHeight/2, holographicDisplayDepth/2),
            new Vector3(-displayWidth/2, displayHeight/2, holographicDisplayDepth/2),
        };

        // Create triangles (just the front face)
        int[] triangles = new int[]
        {
            0, 1, 2,
            0, 2, 3,
        };

        // Create UVs with golden ratio proportions
        Vector2[] uvs = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1),
        };

        // Create normals
        Vector3[] normals = new Vector3[]
        {
            Vector3.forward,
            Vector3.forward,
            Vector3.forward,
            Vector3.forward,
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.normals = normals;
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();

        return mesh;
    }

    public Material CreateBaseMaterial()
    {
        Material material = new Material(Shader.Find("Standard"));
        material.color = baseColor;
        material.SetFloat("_Metallic", metallic);
        material.SetFloat("_Glossiness", smoothness);
        material.EnableKeyword("_EMISSION");
        material.SetColor("_EmissionColor", accentColor * emissionIntensity);
        return material;
    }

    public Material CreateSlotMaterial()
    {
        Material material = new Material(Shader.Find("Standard"));
        material.color = baseColor;
        material.SetFloat("_Metallic", metallic);
        material.SetFloat("_Glossiness", smoothness);
        material.EnableKeyword("_EMISSION");
        material.SetColor("_EmissionColor", accentColor * emissionIntensity * 0.5f);
        return material;
    }

    public Material CreateHolographicMaterial()
    {
        Material material = new Material(Shader.Find("Standard"));
        material.color = holographicColor;
        material.SetFloat("_Mode", 3); // Transparent
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 3000;
        material.EnableKeyword("_EMISSION");
        material.SetColor("_EmissionColor", holographicColor * emissionIntensity);
        return material;
    }
} 