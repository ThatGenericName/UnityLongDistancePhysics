using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChunkController : MonoBehaviour
{

    public Dictionary<(int, int), Chunk> ChunkDict = new Dictionary<(int, int), Chunk>();

    public List<Chunk> ChunkList = new List<Chunk>();

    public GameObject ChunkPrefab;

    public float ChunkSize = ZoningController.ChunkSize;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void UpdateChunks(List<WanderingObject> wObjects)
    {
        ChunkDict.Clear();

        Queue<Chunk> chunkPool = new Queue<Chunk>(ChunkList);

        foreach (var oldChunk in ChunkList)
        {
            oldChunk.Active = false;
            oldChunk.Objects.Clear();
        }
        
        foreach (var wObject in wObjects)
        {
            Vector3 pos = wObject.transform.position;

            Vector3 objectVelocity = wObject.rb.velocity;
            
            Vector3 future = pos + Time.deltaTime * ZoningController.TicksPerUpdate * objectVelocity;

            wObject.NextCyclePosition = future;

            List<(int, int)> chunkCoordinates = GetChunksAlongPath(pos, future);
            
            var first = chunkCoordinates[0];
            
            AddObjectToChunk(first, ChunkDict, chunkPool, wObject);
            wObject.CurrentChunk = first;

            for (int i = 1; i < chunkCoordinates.Count; i++)
            {
                var coordinate = chunkCoordinates[i];
                AddObjectToChunk(coordinate, ChunkDict, chunkPool, wObject);
            }
        }
        
        
    }
    
    private void AddObjectToChunk((int, int) coordinate, Dictionary<(int, int), Chunk> chunks, Queue<Chunk> chunkPool, WanderingObject wObject)
    {
        int xChunk = coordinate.Item1;
        int zChunk = coordinate.Item2;

        if (!chunks.ContainsKey((xChunk, zChunk)))
        {
            Chunk chunkToUse;

            if (chunkPool.Count != 0)
            {
                chunkToUse = chunkPool.Dequeue();
            }
            else
            {
                var chunkGO = Instantiate(ChunkPrefab, Vector3.zero, Quaternion.identity);
                chunkToUse = chunkGO.GetComponent<Chunk>();
                chunkGO.SetActive(true);
                ChunkList.Add(chunkToUse);
                chunkGO.transform.SetParent(this.transform);
            }

            chunkToUse.Active = true;
            chunks[(xChunk, zChunk)] = chunkToUse;

            chunkToUse.x = xChunk;
            chunkToUse.z = zChunk;

            chunkToUse.TLC = new Vector3(
                xChunk * ChunkSize,
                0,
                zChunk * ChunkSize
            );
            chunkToUse.BRC = new Vector3(
                (xChunk + 1) * ChunkSize,
                0,
                (zChunk + 1) * ChunkSize
            );

            chunkToUse.DrawChunkBorders();
        }

        chunks[(xChunk, zChunk)].Add(wObject);
    }
    
    private List<(int, int)> GetChunksAlongPath(Vector3 start, Vector3 end)
    {
        Vector3 path = end - start;

        List<(int, int)> ChunkCoordinates = new List<(int, int)>();

        int numberOfChecks = ZoningController.TicksPerUpdate / 2;

        Vector3 segment = path / numberOfChecks;
        
        for (int i = 0; i < numberOfChecks; i++)
        {
            Vector3 offset = segment * i;
            Vector3 current = start + offset;

            ZoningController.RolloverVector3(ref current);
            
            int xChunk = Mathf.FloorToInt(current.x / ChunkSize);
            int zChunk = Mathf.FloorToInt(current.z / ChunkSize);

            if (ChunkCoordinates.Count != 0)
            {
                (int lastX, int lastZ) = ChunkCoordinates.Last();

                if (lastX == xChunk && lastZ == zChunk)
                {
                    continue;
                }
            }
            
            ChunkCoordinates.Add((xChunk, zChunk));
        }

        return ChunkCoordinates;
    }
}
