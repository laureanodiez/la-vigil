using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class VisionController : MonoBehaviour
{
    [Header("Configuración")]
    public float visionRadius = 8f;
    public int rayCount = 90;
    public LayerMask wallLayer = 1 << 7;
    
    [Header("Referencias")]
    public Material visionMaterial;
    
    private Mesh visionMesh;
    private MeshFilter meshFilter;
    private List<Vector3> rayPoints = new List<Vector3>();
    
    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        visionMesh = new Mesh();
        meshFilter.mesh = visionMesh;
        
        GetComponent<MeshRenderer>().material = visionMaterial;
        transform.position = new Vector3(0, 0, 0); // Z=0
    }

    private void LateUpdate()
    {
        if (transform.parent != null)
        {
            transform.position = transform.parent.position;
        }
        GenerateVisionMesh();
    }

    private void GenerateVisionMesh()
    {
        rayPoints.Clear();
        Vector3 origin = transform.position;
        rayPoints.Add(Vector3.zero); // Punto central relativo

        float angleStep = 360f / rayCount;
        
        for (int i = 0; i <= rayCount; i++)
        {
            float angle = i * angleStep;
            Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.right;
            
            RaycastHit2D hit = Physics2D.Raycast(
                origin, 
                dir, 
                visionRadius, 
                wallLayer
            );
            
            Vector3 point;
            if (hit.collider != null)
            {
                point = hit.point - (Vector2)origin;
                // Ajuste para evitar "sobresalir" de las paredes
                point *= 0.95f; 
            }
            else
            {
                point = dir * visionRadius;
            }
            rayPoints.Add(point);
        }
        
        CreateMesh();
    }

    private void CreateMesh()
    {
        if (rayPoints.Count < 3) return;
        
        // Crear vértices
        Vector3[] vertices = new Vector3[rayPoints.Count];
        for (int i = 0; i < rayPoints.Count; i++)
        {
            vertices[i] = rayPoints[i];
        }
        
        // Crear triángulos
        int[] triangles = new int[(rayPoints.Count - 1) * 3];
        int triIndex = 0;
        
        for (int i = 1; i < rayPoints.Count - 1; i++)
        {
            triangles[triIndex++] = 0;       // Centro
            triangles[triIndex++] = i;
            triangles[triIndex++] = i + 1;
        }
        
        visionMesh.Clear();
        visionMesh.vertices = vertices;
        visionMesh.triangles = triangles;
        visionMesh.RecalculateNormals();
        visionMesh.RecalculateBounds();
    }
}