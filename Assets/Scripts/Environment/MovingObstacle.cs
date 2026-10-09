using UnityEngine;

namespace SpaceJet.Environment
{
    public class MovingObstacle : MonoBehaviour
    {
        public enum MoveAxis { Horizontal, Vertical }

        public MoveAxis axis = MoveAxis.Horizontal;
        public float moveSpeed = 2.5f;
        public float amplitude = 6.0f;

        private Vector3 startPos;
        private float randomOffset;

        private void Start()
        {
            startPos = transform.position;
            randomOffset = Random.Range(0f, Mathf.PI * 2f);
        }

        private void OnEnable()
        {
            startPos = transform.position;
            randomOffset = Random.Range(0f, Mathf.PI * 2f);
        }

        private void Update()
        {
            float offset = Mathf.Sin(Time.time * moveSpeed + randomOffset) * amplitude;

            if (axis == MoveAxis.Horizontal)
            {
                transform.position = new Vector3(startPos.x + offset, transform.position.y, transform.position.z);
            }
            else
            {
                transform.position = new Vector3(transform.position.x, startPos.y + offset, transform.position.z);
            }
        }
    }
}
