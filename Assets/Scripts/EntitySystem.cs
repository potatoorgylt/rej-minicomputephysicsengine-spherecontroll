using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Based on https://docs.unity3d.com/ScriptReference/Graphics.DrawMeshInstancedIndirect.html
public class EntitySystem : MonoBehaviour
{
    public struct Entity
    {
        public Vector3 position;
        public Vector3 velocity;
        public float radius;
        public float massInverse;

        public static int SizeInBytes = (3 + 3 + 1 + 1) * sizeof(float);
    }

    public int instanceCount = 10000;
    public Mesh instanceMesh;
    public Material instanceMaterial;
    public int subMeshIndex = 0;

    public ComputeShader computeShader;

    private int cachedInstanceCount = -1;
    private int cachedSubMeshIndex = -1;
    private ComputeBuffer entityBuffer;
    private ComputeBuffer entityPropsBuffer;
    private ComputeBuffer argsBuffer;
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

    public float gravity = 0.98f;
    public Vector3 wind = Vector3.zero;
    public Bounds worldBounds = new Bounds(Vector3.zero, Vector3.one * 100);
    public float drag = 0.1f;

    private MousePosition mousePosition;
    private Entity[] entities;

    void Start()
    {
        mousePosition = GetComponent<MousePosition>();

        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        UpdateBuffers();
    }

    void Update()
    {
        // Update starting position buffer
        if (cachedInstanceCount != instanceCount || cachedSubMeshIndex != subMeshIndex)
            UpdateBuffers();

        if (Input.GetMouseButtonDown(2))
        {
            Posession(entities[0]);
        }

        //Controll wind with mouse input
        Vector3 windByMousePos = mousePosition.MousePosPoint();
        wind = windByMousePos;

        computeShader.SetFloat("_DeltaTime", Time.deltaTime);
        computeShader.SetVector("_ExternalForce", new Vector3(wind.x, -gravity + wind.z, wind.y));
        computeShader.SetVector("_WorldBoundsMin", worldBounds.min);
        computeShader.SetVector("_WorldBoundsMax", worldBounds.max);
        computeShader.SetFloat("_DragCoefficient", drag);
        computeShader.SetInt("_Count", instanceCount);
        computeShader.SetBuffer(0, "EntityBuffer", entityBuffer);
        computeShader.Dispatch(0, instanceCount / 64 + 1, 1, 1);

        instanceMaterial.SetBuffer("EntityBuffer", entityBuffer);
        instanceMaterial.SetBuffer("EntityPropsBuffer", entityPropsBuffer);

        // Render
        Graphics.DrawMeshInstancedIndirect(instanceMesh, subMeshIndex, instanceMaterial, worldBounds, argsBuffer);
    }

    void OnGUI()
    {
        GUI.Label(new Rect(265, 20, 200, 30), "Instance Count: " + instanceCount.ToString());
        instanceCount = (int)GUI.HorizontalSlider(new Rect(25, 20, 200, 30), (float)instanceCount, 1.0f, 50000.0f);
    }

    void UpdateBuffers()
    {
        // Ensure submesh index is in range
        if (instanceMesh != null)
            subMeshIndex = Mathf.Clamp(subMeshIndex, 0, instanceMesh.subMeshCount - 1);

        // Entities
        if (entityBuffer != null)
            entityBuffer.Release();
    
        entityBuffer = new ComputeBuffer(instanceCount, Entity.SizeInBytes);
        entities = new Entity[instanceCount];

        for (int i = 0; i < instanceCount; i++)
        {
            entities[i].position = Random.insideUnitSphere * 7.0f;
            entities[i].velocity = Random.insideUnitSphere;

            entities[i].radius = Random.Range(0.05f, 0.5f);
            entities[i].massInverse = 1.0f / ((4.0f / 3.0f) * Mathf.PI * Mathf.Pow(entities[i].radius, 3));
        }
        entityBuffer.SetData(entities);
        instanceMaterial.SetBuffer("EntityBuffer", entityBuffer);

        if (entityPropsBuffer != null)
            entityPropsBuffer.Release();
        entityPropsBuffer = new ComputeBuffer(instanceCount, 4 * sizeof(float));
        Vector4[] props = new Vector4[instanceCount];
        for (int i = 0; i < instanceCount; i++)
        {
            props[i].x = 0.5f + 0.5f * Random.value;
            props[i].y = 0.5f + 0.5f * Random.value;
            props[i].z = 0.5f + 0.5f * Random.value;
            props[i].w = Random.value;
        }
        entityPropsBuffer.SetData(props);
        instanceMaterial.SetBuffer("EntityPropsBuffer", entityPropsBuffer);

        // Indirect args
        if (instanceMesh != null)
        {
            args[0] = (uint)instanceMesh.GetIndexCount(subMeshIndex);
            args[1] = (uint)instanceCount;
            args[2] = (uint)instanceMesh.GetIndexStart(subMeshIndex);
            args[3] = (uint)instanceMesh.GetBaseVertex(subMeshIndex);
        }
        else
        {
            args[0] = args[1] = args[2] = args[3] = 0;
        }
        argsBuffer.SetData(args);

        cachedInstanceCount = instanceCount;
        cachedSubMeshIndex = subMeshIndex;
    }

    void Posession(Entity possessedEntity)
    {
        Debug.Log("posessing");

        entities[0].position = Random.insideUnitSphere * 7.0f;
        entities[0].velocity = Random.insideUnitSphere;

        entities[0].radius = Random.Range(0.05f, 0.5f);
        entities[0].massInverse = 1.0f / ((4.0f / 3.0f) * Mathf.PI * Mathf.Pow(possessedEntity.radius, 3));

        for (int i = 1; i < instanceCount; i++)
        {
            entities[i].position = entities[i].position;
            entities[i].velocity = entities[i].velocity;

            entities[i].radius = entities[i].radius;
            entities[i].massInverse = entities[i].massInverse;
        }

        entityBuffer.SetData(entities);
        instanceMaterial.SetBuffer("EntityBuffer", entityBuffer);

        if (entityPropsBuffer != null)
            entityPropsBuffer.Release();
        entityPropsBuffer = new ComputeBuffer(instanceCount, 4 * sizeof(float));
        Vector4[] props = new Vector4[instanceCount];
        for (int i = 0; i < instanceCount; i++)
        {
            props[i].x = 0.5f + 0.5f * Random.value;
            props[i].y = 0.5f + 0.5f * Random.value;
            props[i].z = 0.5f + 0.5f * Random.value;
            props[i].w = Random.value;
        }
        entityPropsBuffer.SetData(props);
        instanceMaterial.SetBuffer("EntityPropsBuffer", entityPropsBuffer);

        // Indirect args
        if (instanceMesh != null)
        {
            args[0] = (uint)instanceMesh.GetIndexCount(subMeshIndex);
            args[1] = (uint)instanceCount;
            args[2] = (uint)instanceMesh.GetIndexStart(subMeshIndex);
            args[3] = (uint)instanceMesh.GetBaseVertex(subMeshIndex);
        }
        else
        {
            args[0] = args[1] = args[2] = args[3] = 0;
        }
        argsBuffer.SetData(args);

        cachedInstanceCount = instanceCount;
        cachedSubMeshIndex = subMeshIndex;
    }

    void OnDisable()
    {
        if (entityBuffer != null)
            entityBuffer.Release();
        entityBuffer = null;

        if (entityPropsBuffer != null)
            entityPropsBuffer.Release();
        entityPropsBuffer = null;


        if (argsBuffer != null)
            argsBuffer.Release();
        argsBuffer = null;
    }
}