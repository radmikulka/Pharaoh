using System.Collections;
using UnityEngine;

namespace GiantGrey.TileWorldCreator.Samples
{
    public class DamageHitFX : MonoBehaviour
    {
        public MeshRenderer meshRenderer;
        public GameObject breakFX;

        public float health = 3;

        void Start()
        {
            // Hit();  
        }

        public float Hit()
        {
            health --;

            meshRenderer.material.SetFloat("_HitFXFlag", 1f);    

            StartCoroutine(Wait());
            return health;
          
        }

        IEnumerator Wait()
        {
            yield return new WaitForSeconds(0.3f);

            meshRenderer.material.SetFloat("_HitFXFlag", 0f);
        }
    }
}