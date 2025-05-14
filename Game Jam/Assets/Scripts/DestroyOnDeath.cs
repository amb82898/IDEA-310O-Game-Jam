using System.Collections;
using System.Collections.Generic;
using BreadcrumbAi;
using UnityEngine;

public class DestroyOnDeath : MonoBehaviour
{ 
    private GameObject skeletonLord;
    private DemoEnemyControls enemyControls;

    void Start()
    {       
        skeletonLord = GameObject.Find("Character_Skeleton_Lord 1");

        if (skeletonLord == null)
        {
            Debug.LogError("Character_Skeleton_Lord 1 not found in the scene.");
            return;
        }
        enemyControls = skeletonLord.GetComponent<DemoEnemyControls>();

        if (enemyControls == null)
        {
            Debug.LogError("DemoEnemyControls not found on Character_Skeleton_Lord 1.");
        }
    }

    void Update()
    {
        if (enemyControls != null && enemyControls._isDead)
        {
            Destroy(gameObject);
        }
    }
}
