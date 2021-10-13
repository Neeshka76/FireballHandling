using System.Collections;
using ThunderRoad;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using SnippetCode;
using System.Linq;
using System.Collections.Generic;

namespace FireballHandling
{
    class FireballHandlingVFX : MonoBehaviour
    {
        GameObject centerPointGameObjectRight;
        Transform centerPointTransRight;
        GameObject emptyGOVFXRight = new GameObject();
        Transform centerPointTransVFXRight;
        EffectData centerPointEffectDataRight;
        EffectInstance centerPointEffectVFXRight;
        bool effectIsPlayingRight = false;
        GameObject centerPointGameObjectLeft;
        Transform centerPointTransLeft;
        GameObject emptyGOVFXLeft = new GameObject();
        Transform centerPointTransVFXLeft;
        EffectData centerPointEffectDataLeft;
        EffectInstance centerPointEffectVFXLeft;
        bool effectIsPlayingLeft = false;

        void Start()
        {
            AsyncOperationHandle<GameObject> handleCenterPoint = Addressables.LoadAssetAsync<GameObject>("Neeshka.FireballHandling.CenterPoint");
            // Right side
            centerPointGameObjectRight = handleCenterPoint.WaitForCompletion();
            centerPointEffectDataRight = Catalog.GetData<EffectData>("CenterPointEffect");
            centerPointGameObjectRight = Instantiate(centerPointGameObjectRight);
            centerPointTransRight = centerPointGameObjectRight.transform;
            centerPointTransVFXRight = emptyGOVFXRight.transform;
            // Left side
            centerPointGameObjectLeft = handleCenterPoint.WaitForCompletion();
            centerPointEffectDataLeft = Catalog.GetData<EffectData>("CenterPointEffect");
            centerPointGameObjectLeft = Instantiate(centerPointGameObjectLeft);
            centerPointTransLeft = centerPointGameObjectLeft.transform;
            centerPointTransVFXLeft = emptyGOVFXLeft.transform;

        }

        void Update()
        {
            if (FireballHandling.rightHandEquipped)
            {
                if (FireballHandling.isCenterPointCreatedRight)
                {
                    centerPointGameObjectRight?.SetActive(true);
                    centerPointTransRight.position = FireballHandling.deltaPositionCenterPointRight;
                    RotateCenterPositionRight();
                    centerPointTransVFXRight.position = centerPointTransRight.position;
                    if (effectIsPlayingRight == false)
                    {
                        centerPointEffectVFXRight = centerPointEffectDataRight.Spawn(centerPointTransVFXRight);
                        centerPointEffectVFXRight?.Play();
                        effectIsPlayingRight = true;
                    }
                }
                else
                {
                    centerPointGameObjectRight?.SetActive(false);
                    centerPointGameObjectRight.transform.rotation = Quaternion.identity;
                    effectIsPlayingRight = false;
                    centerPointEffectVFXRight?.Stop();
                }
            }
            if (FireballHandling.leftHandEquipped)
            {
                if (FireballHandling.isCenterPointCreatedLeft)
                {
                    centerPointGameObjectLeft?.SetActive(true);
                    centerPointTransLeft.position = FireballHandling.deltaPositionCenterPointLeft;
                    RotateCenterPositionLeft();
                    centerPointTransVFXLeft.position = centerPointTransLeft.position;
                    if (effectIsPlayingLeft == false)
                    {
                        centerPointEffectVFXLeft = centerPointEffectDataLeft.Spawn(centerPointTransVFXLeft);
                        centerPointEffectVFXLeft?.Play();
                        effectIsPlayingLeft = true;
                    }
                }
                else
                {
                    centerPointGameObjectLeft?.SetActive(false);
                    centerPointGameObjectLeft.transform.rotation = Quaternion.identity;
                    effectIsPlayingLeft = false;
                    centerPointEffectVFXLeft?.Stop();
                }
            }
            if (FireballHandling.leftHandEquipped == false)
            {
                centerPointGameObjectLeft?.SetActive(false);
                centerPointGameObjectLeft.transform.rotation = Quaternion.identity;
                effectIsPlayingLeft = false;
                centerPointEffectVFXLeft?.Stop();
            }
            if (FireballHandling.rightHandEquipped == false)
            {
                centerPointGameObjectRight?.SetActive(false);
                centerPointGameObjectRight.transform.rotation = Quaternion.identity;
                effectIsPlayingRight = false;
                centerPointEffectVFXRight?.Stop();
            }

        }

        void LateUpdate()
        {

        }

        private void RotateCenterPositionRight()
        {
            centerPointTransRight.Rotate(new Vector3(FireballHandling.distanceCenterFromGripRight.x * 180f, FireballHandling.distanceCenterFromGripRight.y * 180f, FireballHandling.distanceCenterFromGripRight.z * 180f) * Time.deltaTime * FireballHandling.distanceCenterFromGripRight.sqrMagnitude * 200f);
        }
        private void RotateCenterPositionLeft()
        {
            centerPointTransLeft.Rotate(new Vector3(FireballHandling.distanceCenterFromGripLeft.x * 180f, FireballHandling.distanceCenterFromGripLeft.y * 180f, FireballHandling.distanceCenterFromGripLeft.z * 180f) * Time.deltaTime * FireballHandling.distanceCenterFromGripLeft.sqrMagnitude * 200f);
        }
    }
}