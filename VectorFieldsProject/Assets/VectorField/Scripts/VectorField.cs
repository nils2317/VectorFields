using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class VectorField : MonoBehaviour
{
    public enum Mode { TRUERANDOM, CONSTANTRANDOM, TEST, WATERFALL, SPIN, UP, OUT };

    public Vector3Int gridSize;
    public Vector3[,,] vectorFieldDirections;
    public float cellSize;

    public GameObject particle;
    public int particleCount;
    public float particleScale;
    [Range(1f, 20f)]
    public float speed = 1f;
    public bool explode = false;
    public bool spawnCenter = false;

    public Mode mode;

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
        Vector3 startPos = new Vector3();
        if (!spawnCenter)
        {
            startPos = new Vector3(Random.Range(this.transform.position.x, this.transform.position.x + gridSize.x * cellSize),
                                   Random.Range(this.transform.position.y, this.transform.position.y + gridSize.y * cellSize),
                                   Random.Range(this.transform.position.z, this.transform.position.z + gridSize.z * cellSize));
        }
        else
        {
            startPos = new Vector3(this.transform.position.x + cellSize * gridSize.x / 2,
                                   this.transform.position.y + cellSize * gridSize.y / 2,
                                   this.transform.position.z + cellSize * gridSize.z / 2);

        }

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
                    if(mode == Mode.CONSTANTRANDOM)
                    {
                        Vector3 direction = new Vector3(y * y * y - 9 * y, x * x * x + 3 * x, 1.0f);
                        direction = Random.insideUnitSphere;
                        vectorFieldDirections[x, y, z] = Vector3.Normalize(direction) * speed;
                    }
                }
            }
        }
    }

    void ParticleUpdate()
    {
        List<VectorFieldParticle> toDestroy = new List<VectorFieldParticle>();
        foreach (VectorFieldParticle pt in particles)
        {
            // Grided Vectors
            if(mode == Mode.CONSTANTRANDOM)
            {
                Vector3Int posOnGrid = new Vector3Int(Mathf.FloorToInt(Mathf.Clamp((pt.position.x - this.transform.position.x) / cellSize, 0, gridSize.x - 1)),
                                                      Mathf.FloorToInt(Mathf.Clamp((pt.position.y - this.transform.position.y) / cellSize, 0, gridSize.y - 1)),
                                                      Mathf.FloorToInt(Mathf.Clamp((pt.position.z - this.transform.position.z) / cellSize, 0, gridSize.z - 1)));
            
                pt.ApplyAcceleration(vectorFieldDirections[posOnGrid.x, posOnGrid.y, posOnGrid.z]);
            }

            //float result;
            //ExpressionEvaluator.Evaluate<float>("4 + 3", out result);
            //Debug.Log(result);

            // Precise Vectors
            else if(mode == Mode.TEST)
            {
                Vector3 direction = new Vector3(pt.position.y * pt.position.y * pt.position.y - 9 * pt.position.y, pt.position.x * pt.position.x * pt.position.x + 3 * pt.position.x, 1.0f);
                pt.ApplyAcceleration(direction.normalized * speed);
            }

            else if (mode == Mode.WATERFALL)
            {
                Vector3 direction = new Vector3(0, -1, 0);
                pt.ApplyAcceleration(direction.normalized * speed);
            }

            else if(mode == Mode.TRUERANDOM)
            {
                Vector3 direction = Random.insideUnitSphere;
                pt.ApplyAcceleration(direction.normalized * speed);
            }
            else if(mode == Mode.SPIN)
            {
                Vector3 direction = new Vector3(pt.position.x, pt.position.y, 0);
                direction.x *= 2;
                direction.y *= 2;
                direction.x -= gridSize.x;
                direction.y -= gridSize.y;
                direction.Normalize();
                float tempX = direction.x, tempY = direction.y;
                direction = new Vector3(-(tempY / (tempX * tempX + tempY * tempY)), tempX / (tempX * tempX + tempY * tempY), 0f);
                pt.ApplyAcceleration(direction.normalized * speed);
            }
            else if(mode == Mode.UP)
            {
                Vector3 direction = new Vector3(0, 1, 0);
                pt.ApplyAcceleration(direction.normalized * speed);
            }
            else if (mode == Mode.OUT)
            {
                Vector3 direction = new Vector3(pt.position.x, pt.position.y, 0);
                direction.x *= 2;
                direction.y *= 2;
                direction.x -= gridSize.x;
                direction.y -= gridSize.y;
                pt.ApplyAcceleration(direction.normalized * speed);
            }



            // Clear and Replace exited particles
            if (pt.position.x > fieldUpperBounds.x || pt.position.y > fieldUpperBounds.y || pt.position.z > fieldUpperBounds.z ||
                pt.position.x < fieldLowerBounds.x || pt.position.y < fieldLowerBounds.y || pt.position.z < fieldLowerBounds.z)
            {
                toDestroy.Add(pt);
            }
        }

        if(!explode)
        {
            foreach (VectorFieldParticle pt in toDestroy)
            {
                particles.Remove(pt);
                Destroy(pt.gameObject);
                spawnParticle();
            }
        }
        else
        {
            foreach (VectorFieldParticle pt in toDestroy)
            {
                particles.Remove(pt);
                pt.Release();
            }
        }

        toDestroy.Clear();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(this.transform.position + new Vector3(gridSize.x, gridSize.y, gridSize.z) * 0.5f * cellSize, new Vector3(gridSize.x, gridSize.y, gridSize.z) * cellSize);
        
        for(int x = 0; x < gridSize.x; ++x)
        {
            for (int y = 0; y < gridSize.y; ++y)
            {
                for (int z = 0; z < gridSize.z; ++z)
                {
                    Vector3 direction = new Vector3();

                    if (mode == Mode.TEST)
                        direction = new Vector3(y * y * y - 9 * y, x * x * x + 3 * x, 1.0f);
                    else if (mode == Mode.WATERFALL)
                        direction = new Vector3(0, -1, 0);
                    else if(mode == Mode.UP)
                        direction = new Vector3(0, 11, 0);
                    else if (mode == Mode.OUT)
                    {
                        direction = new Vector3(x, y, 0);
                        direction.x *= 2;
                        direction.y *= 2;
                        direction.x -= gridSize.x;
                        direction.y -= gridSize.y;
                        direction.Normalize();
                    }
                    else if (mode == Mode.SPIN)
                    {
                        direction = new Vector3(x, y, 0);
                        direction.x *= 2;
                        direction.y *= 2;
                        direction.x -= gridSize.x;
                        direction.y -= gridSize.y;
                        direction.Normalize();
                        float tempX = direction.x, tempY = direction.y;
                        direction = new Vector3(-(tempY / (tempX * tempX + tempY * tempY)), tempX / (tempX * tempX + tempY * tempY), 0f);
                    }


                    Gizmos.color = new Color(direction.normalized.x, direction.normalized.y, direction.normalized.z);
                    Vector3 pos = new Vector3(x, y, z) + transform.position;
                    Vector3 endPos = pos + direction;// Vector3.Normalize(direction);
                    Gizmos.DrawLine(pos, endPos);

                    Gizmos.DrawSphere(endPos, 0.1f);
                }
            }
        }
        
    }
    
}
