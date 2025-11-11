using UnityEngine;

public class Player : MonoBehaviour
{
    [Tooltip("Maximale Geschwindigkeit in Einheiten pro Sekunde (XZ-Ebene)")]
    public float maxSpeed = 5f;

    [Tooltip("Beschleunigung in Einheiten pro Sekunde^2")]
    public float acceleration = 20f;

    [Tooltip("Drehgeschwindigkeit in Grad pro Sekunde, 0 = keine Drehung")]
    public float rotationSpeed = 360f;

    [Tooltip("Optionales PhysicMaterial f�r den Collider (z.B. niedrige Reibung)")]
    public PhysicsMaterial physicsMaterial;

    private Rigidbody rb;
    private Collider col;
    private Vector3 inputDirection; // x/z-Richtung, L�nge <= 1

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        // Rigidbody-Einstellungen f�r besseres Verhalten
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ; // nur Y-Rotation erlauben falls n�tig

        // Wenn ein PhysicMaterial gesetzt ist, zuweisen
        if (physicsMaterial != null && col != null)
            col.material = physicsMaterial;
    }

    private void Update()
    {
        // WSAD-Input sammeln (unterst�tzt ausschlie�lich auf XZ-Ebene)
        Vector3 dir = Vector3.zero;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) dir += Vector3.forward;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) dir += Vector3.back;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) dir += Vector3.left;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) dir += Vector3.right;

        if (dir.sqrMagnitude > 1f)
            dir.Normalize();

        inputDirection = dir;
    }

    private void FixedUpdate()
    {
        ApplyMovementPhysics();
        ApplyRotation();
    }

    private void ApplyMovementPhysics()
    {
        // Aktuelle horizontale Geschwindigkeit (XZ) beibehalten
        Vector3 velocity = rb.linearVelocity;
        Vector3 horizontalVel = new Vector3(velocity.x, 0f, velocity.z);

        // Zielgeschwindigkeit auf XZ
        Vector3 targetVel = inputDirection * maxSpeed;

        // Ben�tigte �nderung der Geschwindigkeit
        Vector3 velChange = targetVel - horizontalVel;

        // Begrenze die �nderungsgr��e (=Beschleunigung * deltaTime)
        float maxChange = acceleration * Time.fixedDeltaTime;
        Vector3 limitedChange = Vector3.ClampMagnitude(velChange, maxChange);

        // F�gt dem Rigidbody eine sofortige Geschwindigkeits�nderung hinzu (physikalisch konsistent)
        rb.AddForce(limitedChange, ForceMode.VelocityChange);

        // Sicherstellen, dass horizontale Geschwindigkeit nicht �ber maxSpeed steigt (nach AddForce)
        Vector3 newVel = rb.linearVelocity;
        Vector3 newHorizontal = new Vector3(newVel.x, 0f, newVel.z);
        if (newHorizontal.magnitude > maxSpeed)
        {
            newHorizontal = newHorizontal.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(newHorizontal.x, newVel.y, newHorizontal.z);
        }
    }

    private void ApplyRotation()
    {
        if (rotationSpeed <= 0f)
            return;

        if (inputDirection.sqrMagnitude < 0.0001f)
            return;

        // Zielrotation basierend auf Bewegungsrichtung (XZ)
        Quaternion targetRot = Quaternion.LookRotation(inputDirection, Vector3.up);
        float step = rotationSpeed * Time.fixedDeltaTime;
        Quaternion newRot = Quaternion.RotateTowards(rb.rotation, targetRot, step);
        rb.MoveRotation(newRot);
    }
}
