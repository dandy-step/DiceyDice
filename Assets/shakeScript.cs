using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shakeScript : MonoBehaviour {
    public bool wantRolledNumber = false;
    public uint rolledNumber = 0;


    void Update() {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (wantRolledNumber && (rb.velocity.x == 0f) && (rb.velocity.y == .0f) && (rb.velocity.z == 0f) && (rb.angularVelocity.x == 0f) && rb.angularVelocity.y == 0f && rb.angularVelocity.z == 0f) {
            Dictionary<string, float> angles = new Dictionary<string, float>();
            angles.Add("4", Vector3.Angle(transform.up, Vector3.up));
            angles.Add("3", Vector3.Angle(-transform.up, Vector3.up));
            angles.Add("1", Vector3.Angle(transform.forward, Vector3.up));
            angles.Add("6", Vector3.Angle(-transform.forward, Vector3.up));
            angles.Add("5", Vector3.Angle(transform.right, Vector3.up));
            angles.Add("2", Vector3.Angle(-transform.right, Vector3.up));

            float lowest = 360f;
            foreach (KeyValuePair<string, float> face in angles) {
                if (face.Value < lowest) {
                    rolledNumber = uint.Parse(face.Key.ToString());
                    lowest = face.Value;
                }
            }
            if (lowest >= 5f) {
                rolledNumber = 0;
                GetComponent<Rigidbody>().AddExplosionForce(500f, Vector3.up, 100f);
            }
        }
    }
}
