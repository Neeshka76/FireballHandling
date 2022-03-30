using System.Collections;
using ThunderRoad;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;


namespace FireballHandling
{
    public class FireballHandlingLevelModule : LevelModule
    {
        private FireballHandling fireballHandling;
        // Execute at the loading of the level
        public override IEnumerator OnLoadCoroutine()
        {
            // Create fireballHandling class
            fireballHandling = GameManager.local.gameObject.AddComponent<FireballHandling>();
            return base.OnLoadCoroutine();
        }

        public override void Update()
        {
            if (fireballHandling == null)
            {
                fireballHandling = GameManager.local.gameObject.AddComponent<FireballHandling>();
                return;
            }
            /*if (fireballClass == null)
			{
				fireballClass = GameManager.local.gameObject.AddComponent<FireballHandlingFireballClass>();
				return;
			}*/
        }
    }
}