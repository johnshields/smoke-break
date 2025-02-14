using UnityEngine;

namespace _Scripts
{
    public class ItemPickup : MonoBehaviour
    {
        [SerializeField] private int itemAmount = 10;
        [SerializeField] private AudioClip pickupSound;

        private void OnTriggerEnter(Collider other)
        {
            var pistol = other.GetComponent<PistolProfiler>();

            if (pistol != null)
            {
                if (pistol.currentClipAmmo < pistol.storedAmmo)
                {
                    pistol.RefillAmmo(itemAmount);
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                    Destroy(gameObject);
                }
            }
        }
    }
}
