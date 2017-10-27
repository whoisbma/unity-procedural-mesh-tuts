using UnityEngine;

[RequireComponent(
  typeof(MeshFilter)
)]

public class MeshDeformer : MonoBehaviour {

  public Transform transform;

  public float springForce = 20f;
  public float damping = 5f;
  private float uniformScale = 1f;

  private Mesh deformingMesh;
  private MeshCollider colliderMesh;
  private Vector3[] originalVertices, displacedVertices;
  private Vector3[] vertexVelocities;

	private void Start() {
    transform = GetComponent<Transform>();

		deformingMesh = GetComponent<MeshFilter>().mesh;
    colliderMesh = GetComponent<MeshCollider>();
    originalVertices = deformingMesh.vertices;
    displacedVertices = new Vector3[originalVertices.Length];
    for (int i = 0; i < originalVertices.Length; i++) {
      displacedVertices[i] = originalVertices[i];
    }
    vertexVelocities = new Vector3[originalVertices.Length];
	}

  public void AddDeformingForce(Vector3 point, float force) {
    Debug.DrawLine(Camera.main.transform.position, point);
    point = transform.InverseTransformPoint(point);

    for (int i = 0; i < displacedVertices.Length; i++) {
      AddForceToVertex(i, point, force);
    }
  }

  private void AddForceToVertex(int i, Vector3 point, float force) {
    Vector3 pointToVertex = displacedVertices[i] - point;
    pointToVertex *= uniformScale;
    float attenuatedForce = force / (1f + pointToVertex.sqrMagnitude);
    float velocity = attenuatedForce * Time.deltaTime;
    vertexVelocities[i] += pointToVertex.normalized * velocity;
  }

  private void FixedUpdate() {
    transform.Rotate(Vector3.up * 40f * Time.deltaTime);
  }

  private void Update() {

    if (Input.GetMouseButton(0)) {
      springForce = 20f;
    } else {
      springForce = 0f;
    }

    uniformScale = transform.localScale.x;
    for (int i = 0; i < displacedVertices.Length; i++) {
      UpdateVertex(i);
    }
    deformingMesh.vertices = displacedVertices;
    colliderMesh.sharedMesh = deformingMesh;
    deformingMesh.RecalculateNormals();
  }

  private void UpdateVertex(int i) {
    Vector3 velocity = vertexVelocities[i];
    Vector3 displacement = displacedVertices[i] - originalVertices[i];
    displacement *= uniformScale;
    velocity -= displacement * springForce * Time.deltaTime;
    velocity *= 1f - damping * Time.deltaTime;
    vertexVelocities[i] = velocity;
    displacedVertices[i] += velocity * (Time.deltaTime / uniformScale);
    originalVertices[i] = displacedVertices[i];   // don't need original vs. displaced if i don't actually return to starting position
  }
}
