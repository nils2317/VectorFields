using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorField : MonoBehaviour
{
    public Vector3Int gridSize;
    public Vector3[,,] vectorFieldDirections;
    public float cellSize;

    public GameObject particle;
    public int particleCount;
    public float particleScale;
    public List<VectorFieldParticle> particles;

    Vector3 fieldUpperBounds, fieldLowerBounds;

    // Start is called before the first frame update
    void Start()
    {
        vectorFieldDirections = new Vector3[gridSize.x, gridSize.y, gridSize.z];
        particles = new List<VectorFieldParticle>();

        for (int i = 0; i < particleCount; ++i)
        {
            spawnParticle();
        }

        fieldUpperBounds.x = this.transform.position.x + gridSize.x * cellSize;// * 0.5f;
        fieldUpperBounds.y = this.transform.position.y + gridSize.y * cellSize;// * 0.5f;
        fieldUpperBounds.z = this.transform.position.z + gridSize.z * cellSize;// * 0.5f;
        fieldLowerBounds.x = this.transform.position.x;// - gridSize.x * cellSize * 0.5f;
        fieldLowerBounds.y = this.transform.position.y;// - gridSize.y * cellSize * 0.5f;
        fieldLowerBounds.z = this.transform.position.z;// - gridSize.z * cellSize * 0.5f;

    }

    // Update is called once per frame
    void Update()
    {
        CalculateVectorFieldDirection();
        ParticleUpdate();
    }

    void spawnParticle()
    {
        Vector3 startPos = new Vector3(Random.Range(this.transform.position.x, this.transform.position.x + gridSize.x * cellSize),
                               Random.Range(this.transform.position.y, this.transform.position.y + gridSize.y * cellSize),
                               Random.Range(this.transform.position.z, this.transform.position.z + gridSize.z * cellSize));

        GameObject particleInstance = (GameObject)Instantiate(particle);
        particleInstance.transform.position = startPos;
        particleInstance.transform.parent = this.transform;
        particleInstance.transform.localScale = new Vector3(particleScale, particleScale, particleScale);
        particles.Add(particleInstance.GetComponent<VectorFieldParticle>());
    }

    void CalculateVectorFieldDirection()
    {
        for (int x = 0; x < gridSize.x; ++x)
        {
            for (int y = 0; y < gridSize.y; ++y)
            {
                for (int z = 0; z < gridSize.z; ++z)
                {
                    //Vector3 direction = new Vector3(y * y * y - 9 * y, x * x * x + 3 * x, 1.0f);
                    Vector3 direction = new Vector3(1f, 0f, 0f);
                    vectorFieldDirections[x, y, z] = Vector3.Normalize(direction);
                }
            }
        }
    }

    void ParticleUpdate()
    {
        foreach (VectorFieldParticle pt in particles)
        {
            Vector3Int posOnGrid = new Vector3Int(Mathf.FloorToInt(Mathf.Clamp((pt.position.x - this.transform.position.x) / cellSize, 0, gridSize.x - 1)),
                                                  Mathf.FloorToInt(Mathf.Clamp((pt.position.y - this.transform.position.y) / cellSize, 0, gridSize.y - 1)),
                                                  Mathf.FloorToInt(Mathf.Clamp((pt.position.z - this.transform.position.z) / cellSize, 0, gridSize.z - 1)));

            pt.ApplyAcceleration(vectorFieldDirections[posOnGrid.x, posOnGrid.y, posOnGrid.z]);

            // Clear and Replace exited particles
            if (pt.position.x > fieldUpperBounds.x || pt.position.y > fieldUpperBounds.y || pt.position.z > fieldUpperBounds.z ||
                pt.position.x < fieldLowerBounds.x || pt.position.y < fieldLowerBounds.y || pt.position.z < fieldLowerBounds.z)
            {
                particles.Remove(pt);
                Destroy(pt.gameObject);
                spawnParticle();
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(this.transform.position + new Vector3(gridSize.x, gridSize.y, gridSize.z) * 0.5f * cellSize, new Vector3(gridSize.x, gridSize.y, gridSize.z) * cellSize);
        /*
        for(int x = 0; x < gridSize.x; ++x)
        {
            for (int y = 0; y < gridSize.y; ++y)
            {
                for (int z = 0; z < gridSize.z; ++z)
                {
                    float weight = (x * x) + (2.0f * y) - z;
                    Vector3 direction = new Vector3(y * y * y - 9 * y, x * x * x + 3 * x, 1.0f);

                    Gizmos.color = new Color(direction.normalized.x, direction.normalized.y, direction.normalized.z);
                    Vector3 pos = new Vector3(x, y, z) + transform.position;
                    Vector3 endPos = pos + Vector3.Normalize(direction);
                    Gizmos.DrawLine(pos, endPos);
                }
            }
        }
        */
    }
    
}
